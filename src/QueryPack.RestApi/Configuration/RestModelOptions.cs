namespace QueryPack.RestApi.Configuration
{
    using System.Text.Json;

    public class RestModelOptions
    {
        public bool UseAnnotationRestrictions { get; set; }
        public Action<IMvcBuilder> MvcBuilderOptions { get; set; }
        public string GlobalApiPrefix { get; set; }
        public Action<JsonSerializerOptions> SerializerOptions { get; set; }
        public IList<Type> Criterias { get; } = new List<Type>();
    }
}