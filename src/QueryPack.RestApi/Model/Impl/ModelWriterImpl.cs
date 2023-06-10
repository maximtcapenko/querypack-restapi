namespace QueryPack.RestApi.Model.Impl
{
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using RestApi.Internal;
    using RestApi.Model.Meta;

    internal class ModelWriterImpl<TModel> : IModelWriter<TModel>
        where TModel : class
    {
        private readonly DbContext _dbContext;
        private readonly IModelMetadataProvider _modelMetadataProvider;

        public ModelWriterImpl(DbContext dbContext,
            IModelMetadataProvider modelMetadataProvider)
        {
            _dbContext = dbContext;
            _modelMetadataProvider = modelMetadataProvider;
        }

        public async Task Delete(TModel model)
        {
            var meta = _modelMetadataProvider.GetMetadata(model.GetType());

            var values = new List<object>();
            foreach (var key in meta.GetKeys())
            {
                values.Add(key.ValueGetter.GetValue(model));
            }

            if (values.Count() == 0)
            {
                // throw notfound exception
            }
            var dbModel = await _dbContext.Set<TModel>().FindAsync(values.ToArray());
            if (dbModel == null)
            {
                // throw notfound or argument null exception
            }
            _dbContext.Set<TModel>().Remove(dbModel);
            await _dbContext.SaveChangesAsync();
        }

        public async Task WriteAsync(TModel model)
        {
            var meta = _modelMetadataProvider.GetMetadata(model.GetType());
            var instance = (TModel)meta.InstanceFactory();

            var values = new List<object>();
            foreach (var key in meta.GetKeys())
            {
                values.Add(key.ValueGetter.GetValue(model));
            }

            if (values.Count > 0)
            {
                var dbModel = await _dbContext.Set<TModel>().FindAsync(values.ToArray());
                if (dbModel != null)
                {
                    Map(meta, dbModel, model);
                }
                else
                    await _dbContext.Set<TModel>().AddAsync(model);
            }
            else
                await _dbContext.Set<TModel>().AddAsync(model);
            await _dbContext.SaveChangesAsync();
        }

        void Map(ModelMetadata metadata, object left, object right)
        {
            bool ShouldBeMapped(object left, object right) => !left.Equals(right) && !right.Equals(default);

            foreach (var property in metadata.PropertyMetadata.Where(e => !e.IsReadOnly))
            {
                if (property.IsNavigation)
                {
                    var metaProvider = property.GetModelMetadataProvider();

                    var leftNavigationValue = property.ValueGetter.GetValue(left);
                    var rightNavigationValue = property.ValueGetter.GetValue(right);

                    if (rightNavigationValue != null)
                    {
                        var lazyLoader = _dbContext.GetService<ILazyLoader>();
                        lazyLoader.Load(left, ExpressionUtils.GetMemberPath(property.PropertyExpression as MemberExpression));
                        leftNavigationValue = property.ValueGetter.GetValue(left);

                        if (leftNavigationValue == null)
                            property.ValueSetter.SetValue(left, rightNavigationValue);
                        else
                        {
                            if (property.IsCollection) 
                            {
                                //skip for now
                            }
                            else
                                Map(metaProvider.GetMetadata(property.PropertyType), leftNavigationValue, rightNavigationValue);
                        }
                    }
                }
                else
                {
                    var leftValue = property.ValueGetter.GetValue(left);
                    var rightValue = property.ValueGetter.GetValue(right);

                    if (ShouldBeMapped(leftValue, rightValue))
                        property.ValueSetter.SetValue(left, rightValue);
                }
            }
        }
    }
}