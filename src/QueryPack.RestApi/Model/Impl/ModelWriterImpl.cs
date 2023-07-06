namespace QueryPack.RestApi.Model.Impl
{
    using System.Collections.Concurrent;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using RestApi.Internal;
    using RestApi.Model.Meta;

    internal class ModelWriterImpl<TModel> : IModelWriter<TModel>
        where TModel : class
    {
        private readonly DbContext _dbContext;
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private static ConcurrentDictionary<Type, Delegate> _internalMethodsCache = new ConcurrentDictionary<Type, Delegate>();

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
                    var entry = _dbContext.Attach<TModel>(model);
                    entry.State = EntityState.Modified;
                    await FetchDependencyTree(entry, new List<EntityEntry> { entry });
                }
                else
                {
                    var entry = _dbContext.Set<TModel>().Attach(model);
                    entry.State = EntityState.Added;
                    await FetchDependencyTree(entry, new List<EntityEntry> { entry });
                }
            }
            else
            {
                var entry = _dbContext.Set<TModel>().Attach(model);
                entry.State = EntityState.Added;
                await FetchDependencyTree(entry, new List<EntityEntry> { entry });
            }

            await _dbContext.SaveChangesAsync();
        }

        static async Task<EntityState> ShouldBeMarkedAsModified<T>(DbContext context, T model, ModelMetadata meta, Expression expression, Expression instanceExpression)
            where T : class
        {
            var where = Expression.Lambda<Func<T, bool>>(expression, instanceExpression as ParameterExpression);
            var currentModel = await context.Set<T>().AsNoTracking().FirstOrDefaultAsync(where);

            if (currentModel == null)
                return EntityState.Added;

            foreach (var property in meta.GetRegularProperties().Where(e => !e.IsReadOnly))
            {
                var getter = property.ValueGetter;
                var newValue = getter.GetValue(model);
                var currentValue = getter.GetValue(currentModel);

                if (newValue?.Equals(currentValue) != true)
                {
                    return EntityState.Modified;
                }
            }

            return EntityState.Unchanged;
        }

        async Task FetchDependencyTree(EntityEntry entity, List<EntityEntry> visited)
        {
            foreach (var navigation in entity.Navigations)
            {
                if (!navigation.Metadata.IsCollection)
                {
                    if (visited.Contains(navigation.EntityEntry)) return;

                    var entityType = _dbContext.Model.FindEntityType(navigation.Metadata.ClrType);
                    if (navigation.CurrentValue != null)
                    {
                        var exists = (Func<object, object[], Task<EntityState>>)_internalMethodsCache.GetOrAdd(entityType.ClrType, type =>
                         {
                             var method = typeof(ModelWriterImpl<TModel>).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                                .FirstOrDefault(e => e.Name == nameof(ShouldBeMarkedAsModified));
                             return MethodFactory.CreateGenericMethod<Task<EntityState>>(method.MakeGenericMethod(entityType.ClrType));
                         });

                        var meta = _modelMetadataProvider.GetMetadata(entityType.ClrType);
                        var keys = meta.GetKeys();
                        Expression predicate = null;

                        foreach (var key in keys)
                        {
                            var value = key.ValueGetter.GetValue(navigation.CurrentValue);
                            if (value != null)
                            {
                                var expression = Expression.Equal(key.PropertyExpression, Expression.Constant(value));
                                if (predicate == null)
                                    predicate = expression;
                                else
                                    predicate = Expression.And(predicate, expression);
                            }
                        }
                        var result = await exists(null, new object[] { _dbContext, navigation.CurrentValue, meta, predicate, meta.InstanceExpression });

                        var @ref = navigation as ReferenceEntry;
                        if (@ref != null)
                        {
                            @ref.TargetEntry.State = result;
                            visited.Add(@ref.TargetEntry);

                            if (result != EntityState.Unchanged)
                                await FetchDependencyTree(@ref.TargetEntry, visited);
                        }
                    }
                }
            }
        }
    }
}