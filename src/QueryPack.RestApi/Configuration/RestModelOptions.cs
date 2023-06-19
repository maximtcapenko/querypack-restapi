namespace QueryPack.RestApi.Configuration
{
    using System.Text.Json;
    using Exceptions;

    public class RestModelOptions
    {
        public Action<IMvcBuilder> MvcBuilderOptions { get; set; }
        public string GlobalApiPrefix { get; set; }
        public Action<JsonSerializerOptions> SerializerOptions { get; set; }
        public IList<Type> Criterias { get; } = new List<Type>();
        public IList<IExceptionHandlingResultBuilder> ExceptionMessageBuilders { get; } = new List<IExceptionHandlingResultBuilder>();
    }
}