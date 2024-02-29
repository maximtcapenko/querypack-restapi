namespace QueryPack.RestApi.Mvc
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Mvc.Internal;
    using RestApi.Internal;
    using RestApi.Model.Meta.Extensions;
    using RestApi.Model;
    using RestApi.Model.Meta;

    [ApiController]
    internal class RestModelController<TModel> : ControllerBase
        where TModel : class
    {
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
            await ProcessNavigationsAsync(model);
            await _dbContext.AddAsync(model);
            await _dbContext.SaveChangesAsync();

            return model;
        }

        [HttpPut, Route("{key}")]
        public async Task<ActionResult<TModel>> UpdateAsync([FromRoute] ICriteria<TModel> key, TModel model)
        {
            var queryset = new ReadOnlyQuerySet(_dbContext.Set<TModel>());
            key.Apply(queryset);

            var result = await queryset.Query.FirstOrDefaultAsync();
            if (result is null)
                return NotFound();

            await ProcessNavigationsAsync(model);
            MapProperties(model, result);
            await _dbContext.SaveChangesAsync();

            return result;
        }

        [HttpDelete, Route("{key}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] ICriteria<TModel> key)
        {
            var queryset = new ReadOnlyQuerySet(_dbContext.Set<TModel>());
            key.Apply(queryset);

            var result = await queryset.Query.FirstOrDefaultAsync();
            if (result is null)
                return NotFound();

            _dbContext.Remove(result);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet, Route("{key}")]
        public async Task<ActionResult<TModel>> GetByKeyAsync([FromRoute] ICriteria<TModel> key)
        {
            var queryset = new ReadOnlyQuerySet(_dbContext.Set<TModel>());
            key.Apply(queryset);

            var result = await queryset.Query.FirstOrDefaultAsync();

            if (result is null)
                return NotFound();

            return result;
        }

        [HttpGet, Route("single")]
        public async Task<ActionResult<TModel>> GetAsync([FromQuery] ICriteria<TModel> criteria)
        {
            var queryset = new ReadOnlyQuerySet(_dbContext.Set<TModel>());
            criteria.Apply(queryset);

            var result = await queryset.Query.FirstOrDefaultAsync();
            if (result is null) return NotFound();

            return result;
        }

        [HttpGet, Route("")]
        public async Task<IEnumerable<TModel>> GetCollectionAsync([FromQuery] ICriteria<TModel> criteria)
        {
            var queryset = new ReadOnlyQuerySet(_dbContext.Set<TModel>());
            criteria.Apply(queryset);

            var result = await queryset.Query.ToListAsync();

            return result;
        }

        [HttpGet, Route("range")]
        public async Task<Range<TModel>> GetRangeAsync([FromQuery] ICriteria<TModel> criteria, [FromQuery] RangeQuery range)
        {
            var queryset = new ReadOnlyQuerySet(_dbContext.Set<TModel>());
            criteria.Apply(queryset);

            if (range.First < 0) range.First = 0;
            if (range.Last < range.First) range.Last = range.First;

            var rangeQuery = (range.Last > 0 && range.Last >= range.First)
                ? queryset.Query.Skip(range.First).Take(range.Last - range.First + 1)
                : queryset.Query;

            var results = await rangeQuery.ToListAsync();
            var count = await queryset.Query.CountAsync();

            return new Range<TModel>(range.First, range.Last, results, count);
        }

        private void MapProperties(TModel from, TModel to)
        {
            var modelMeta = _modelMetadataProvider.GetMetadata(typeof(TModel));
            foreach (var property in modelMeta.PropertyMetadata.Where(e => !e.IsReadOnly && !e.IsKey))
            {
                var fromValue = property.ValueGetter.GetValue(from);
                var originValue = property.ValueGetter.GetValue(to);

                if (property.IsNavigation && fromValue is null) continue;

                if (fromValue != originValue)
                    property.ValueSetter.SetValue(to, fromValue);
            }
        }

        private async Task ProcessNavigationsAsync(TModel model)
        {
            var modelMeta = _modelMetadataProvider.GetMetadata(typeof(TModel));
            foreach (var navigation in modelMeta.GetNavigations())
            {
                if (navigation.IsCollection)
                {
                    await navigation.ProcessCollectionNavigationAsync(_dbContext, model, _modelMetadataProvider);
                    return;
                }

                // load dependencies if they exists
                var navigationInstance = navigation.ValueGetter.GetValue(model);
                if (navigationInstance is not null)
                {
                    var navigationMeta = _modelMetadataProvider.GetMetadata(navigation.PropertyType);

                    // query db
                    var navigationLoader = QueryUtils.GetEntityLoader(navigationMeta.ModelType);
                    var navigationDbValue = await navigationLoader(_dbContext, navigationInstance, navigationMeta);

                    if (navigationDbValue is not null)
                    {
                        navigation.ValueSetter.SetValue(model, navigationDbValue);
                    }
                }
            }
        }

        private sealed class ReadOnlyQuerySet : IQuerySet<TModel>
        {
            public ReadOnlyQuerySet(IQueryable<TModel> query)
            {
                Query = query;
            }

            public IQueryable<TModel> Query { get; set; }
        }
    }
}