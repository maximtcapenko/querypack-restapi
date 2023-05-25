namespace QueryPack.RestApi.Model.Impl
{
    using Microsoft.EntityFrameworkCore;

    internal class ModelReaderImpl<TModel> : IModelReader<TModel>
        where TModel : class
    {
        private readonly DbContext _dbContext;
        private readonly ILogger<ModelReaderImpl<TModel>> _logger;

        public ModelReaderImpl(DbContext dbContext, ILogger<ModelReaderImpl<TModel>> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public Task<TModel> ReadAsync(ICriteria<TModel> criteria)
        {
            var set = _dbContext.Set<TModel>();
            var container = new ModelReadQueryContainer(set);
            criteria.Apply(container);

            return container.Query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TModel>> ReadAsync()
        {
            var models = await _dbContext.Set<TModel>().ToListAsync();
            return models;
        }

        public async Task<Range<TModel>> ReadAsync(ICriteria<TModel> criteria, int first, int last)
        {
            var count = await _dbContext.Set<TModel>().CountAsync();
            var set = _dbContext.Set<TModel>();

            var container = new ModelReadQueryContainer(set);
            criteria.Apply(container);

            if (first < 0) first = 0;
            if (last < first) last = first;

            var query = container.Query;
            var rangeQuery = (last > 0 && last >= first)
                ? query.Skip(first).Take(last - first + 1)
                : query;

            var results = await rangeQuery.ToListAsync();
            return new Range<TModel>(first, last, results, count);
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