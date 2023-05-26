namespace QueryPack.RestApi.Mvc
{
    using Microsoft.AspNetCore.Mvc;
    using RestApi.Model;

    [ApiController]
    [Route("[controller]")]
    internal class RestModelController<TModel> : ControllerBase
        where TModel : class
    {
        private readonly IModelReader<TModel> _reader;

        public RestModelController(IModelReader<TModel> reader)
        {
            _reader = reader;
        }

        [HttpGet("single")]
        public Task<TModel> GetAsync([FromQuery] ICriteria<TModel> criteria)
            => _reader.ReadAsync(criteria);

        public Task<IEnumerable<TModel>> GetAsync()
            => _reader.ReadAsync();

        [HttpGet("range")]
        public Task<Range<TModel>> GetRangeAsync([FromQuery] ICriteria<TModel> criteria, [FromQuery] RangeQuery range)
            => _reader.ReadAsync(criteria, range.First, range.Last);

    }
}