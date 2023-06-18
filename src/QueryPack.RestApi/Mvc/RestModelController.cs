namespace QueryPack.RestApi.Mvc
{
    using Microsoft.AspNetCore.Mvc;
    using RestApi.Model;
    using Mvc.Internal;

    [ApiController]
    internal class RestModelController<TModel> : ControllerBase
        where TModel : class
    {
        private readonly IModelReader<TModel> _reader;
        private readonly IModelWriter<TModel> _writer;

        public RestModelController(IModelReader<TModel> reader,
        IModelWriter<TModel> writer)
        {
            _reader = reader;
            _writer = writer;
        }

        [HttpPost, Route("")]
        [KeysResultFilter]
        public async Task<TModel> ModifyAsync(TModel model)
        {
            await _writer.WriteAsync(model);
            return model;
        }


        [HttpGet, Route("{key}")]
        public async Task<ActionResult<TModel>> GetByKeyAsync([FromRoute] ICriteria<TModel> key)
        {
            var result = await _reader.ReadAsync(key);
            if (result == null)
                return NotFound();

            return result;
        }

        [HttpGet, Route("single")]
        public Task<TModel> GetAsync([FromQuery] ICriteria<TModel> criteria)
            => _reader.ReadAsync(criteria);

        [HttpGet, Route("")]
        public Task<IEnumerable<TModel>> GetAsync()
            => _reader.ReadAsync();

        [HttpGet, Route("range")]
        public Task<Range<TModel>> GetRangeAsync([FromQuery] ICriteria<TModel> criteria, [FromQuery] RangeQuery range)
            => _reader.ReadAsync(criteria, range.First, range.Last);
    }
}