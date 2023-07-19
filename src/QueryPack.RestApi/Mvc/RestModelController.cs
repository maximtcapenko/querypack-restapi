namespace QueryPack.RestApi.Mvc
{
    using System.Collections.Concurrent;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Mvc.Internal;
    using RestApi.Internal;
    using RestApi.Model;
    using RestApi.Model.Meta;

    [ApiController]
    internal class RestModelController<TModel> : ControllerBase
        where TModel : class
    {
        private static ConcurrentDictionary<Type, Delegate> _internalMethodsCache = new ConcurrentDictionary<Type, Delegate>();

        private readonly DbContext _dbContext;
        private readonly IModelMetadataProvider _modelMetadataProvider;

        public RestModelController(DbContext dbContext,
            IModelMetadataProvider modelMetadataProvider)
        {
            _dbContext = dbContext;
            _modelMetadataProvider = modelMetadataProvider;
        }


        [HttpPost, Route("")]
        [KeysResultFilter]
        public async Task<TModel> CreateAsync(TModel model)
        {
            await VisitNavigations(model);
            await _dbContext.AddAsync(model);
            await _dbContext.SaveChangesAsync();

            return model;
        }


        [HttpPut, Route("{key}")]
        public async Task<ActionResult<TModel>> UpdateAsync([FromRoute] ICriteria<TModel> key, TModel model)
        {
            var set = _dbContext.Set<TModel>();
            var container = new ModelReadQueryContainer(set);
            key.Apply(container);

            var result = await container.Query.FirstOrDefaultAsync();
            if (result == null)
                return NotFound();

            await VisitNavigations(model);
            Map(model, result);
            await _dbContext.SaveChangesAsync();

            return result;
        }

        [HttpDelete, Route("{key}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] ICriteria<TModel> key)
        {
            var set = _dbContext.Set<TModel>();
            var container = new ModelReadQueryContainer(set);
            key.Apply(container);

            var result = await container.Query.FirstOrDefaultAsync();
            if (result == null)
                return NotFound();
            
            _dbContext.Remove(result);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet, Route("{key}")]
        public async Task<ActionResult<TModel>> GetByKeyAsync([FromRoute] ICriteria<TModel> key)
        {
            var set = _dbContext.Set<TModel>();
            var container = new ModelReadQueryContainer(set);
            key.Apply(container);

            var result = await container.Query.FirstOrDefaultAsync();

            if (result == null)
                return NotFound();

            return result;
        }

        [HttpGet, Route("single")]
        public Task<TModel> GetAsync([FromQuery] ICriteria<TModel> criteria)
        {
            var set = _dbContext.Set<TModel>();
            var container = new ModelReadQueryContainer(set);
            criteria.Apply(container);

            return container.Query.FirstOrDefaultAsync();
        }

        [HttpGet, Route("")]
        public async Task<IEnumerable<TModel>> GetCollectionAsync([FromQuery] ICriteria<TModel> criteria)
        {
            var set = _dbContext.Set<TModel>();
            var container = new ModelReadQueryContainer(set);
            criteria.Apply(container);

            var result = await container.Query.ToListAsync();

            return result;
        }

        [HttpGet, Route("range")]
        public async Task<Range<TModel>> GetRangeAsync([FromQuery] ICriteria<TModel> criteria, [FromQuery] RangeQuery range)
        {
            var set = _dbContext.Set<TModel>();

            var container = new ModelReadQueryContainer(set);
            criteria.Apply(container);

            if (range.First < 0) range.First = 0;
            if (range.Last < range.First) range.Last = range.First;

            var query = container.Query;
            var rangeQuery = (range.Last > 0 && range.Last >= range.First)
                ? query.Skip(range.First).Take(range.Last - range.First + 1)
                : query;

            var results = await rangeQuery.ToListAsync();
            var count = await query.CountAsync();

            return new Range<TModel>(range.First, range.Last, results, count);
        }

        void Map(TModel from, TModel to)
        {
            var modelMeta = _modelMetadataProvider.GetMetadata(typeof(TModel));
            foreach (var property in modelMeta.PropertyMetadata.Where(e => !e.IsReadOnly && !e.IsKey))
            {
                var fromValue = property.ValueGetter.GetValue(from);
                property.ValueSetter.SetValue(to, fromValue);
            }
        }

        async Task VisitNavigations(TModel model)
        {
            var modelMeta = _modelMetadataProvider.GetMetadata(typeof(TModel));
            foreach (var navigation in modelMeta.GetNavigations())
            {
                if(navigation.IsCollection) //skip collections now
                    continue;

                // load dependencies if they exists
                var navigationInstance = navigation.ValueGetter.GetValue(model);
                if (navigationInstance is not null)
                {
                    var navigationMeta = _modelMetadataProvider.GetMetadata(navigation.PropertyType);

                    // query db
                    var loadQueryAsync = (Func<object, object[], Task<object>>)_internalMethodsCache.GetOrAdd(navigationMeta.ModelType, type =>
                    {
                        var method = typeof(RestModelController<TModel>).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                           .FirstOrDefault(e => e.Name == nameof(LoadAsync));
                        return MethodFactory.CreateGenericMethod<Task<object>>(method.MakeGenericMethod(navigationMeta.ModelType));
                    });

                    var navigationDbValue = await loadQueryAsync(_dbContext, new[] { _dbContext, navigationInstance, navigationMeta });
                    if (navigationDbValue is not null)
                    {
                        // assign existed value (for now)
                        navigation.ValueSetter.SetValue(model, navigationDbValue);
                    }
                }
            }
        }

        static Expression GetQueryExpression(ModelMetadata meta, object instance)
        {

            var keys = meta.GetKeys();
            Expression predicate = null;

            foreach (var key in keys)
            {
                var value = key.ValueGetter.GetValue(instance);
                if (value != null)
                {
                    var expression = Expression.Equal(key.PropertyExpression, Expression.Constant(value));
                    if (predicate == null)
                        predicate = expression;
                    else
                        predicate = Expression.And(predicate, expression);
                }
            }

            return predicate;
        }

        static async Task<object> LoadAsync<T>(DbContext context, object instance, ModelMetadata meta)
          where T : class
        {
            var expression = GetQueryExpression(meta, instance);

            if (expression is null) return null;

            var where = Expression.Lambda<Func<T, bool>>(expression, meta.InstanceExpression as ParameterExpression);
            return await context.Set<T>().FirstOrDefaultAsync(where);
        }

        class ModelReadQueryContainer : IQueryContainer<TModel>
        {
            public ModelReadQueryContainer(IQueryable<TModel> query)
            {
                Query = query;
            }

            public IQueryable<TModel> Query { get; set; }
        }
    }
}