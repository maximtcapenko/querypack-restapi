namespace QueryPack.RestApi.Model.Impl
{
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using QueryPack.RestApi.Model.Meta;

    internal class ModelWriterImpl<TModel> : IModelWriter<TModel>
        where TModel : class
    {
        private readonly DbContext _dbContext;
        private readonly IModelMetadataProvider _modelMetadataProvider;

        public ModelWriterImpl(DbContext dbContext, IModelMetadataProvider modelMetadataProvider)
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
                    // var delta = new Delta<TModel>(model);
                    //delta.Apply(dbModel);
                }
                else
                    await _dbContext.Set<TModel>().AddAsync(model);
            }
            else
                await _dbContext.Set<TModel>().AddAsync(model);
            await _dbContext.SaveChangesAsync();
        }
    }
}