namespace QueryPack.RestApi.Configuration
{
    using System.Text.Json;
    using Exceptions;
    using Microsoft.EntityFrameworkCore;

    public class RestModelOptions
    {
        public Action<IMvcBuilder> MvcBuilderOptions { get; set; }
        public string GlobalApiPrefix { get; set; }
        public Action<JsonSerializerOptions> SerializerOptions { get; set; }
        public Action<IServiceProvider, DbContextOptionsBuilder> ContextOptionsBuilder { get; set; }
        public IList<Type> Criterias { get; } = new List<Type>();
        public IList<IExceptionHandlingResultBuilder> ExceptionMessageBuilders { get; } = new List<IExceptionHandlingResultBuilder>();
    }
}