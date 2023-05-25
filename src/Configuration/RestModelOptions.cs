namespace QueryPack.RestApi.Configuration
{
    using System.Text.Json;

    public class RestModelOptions
    {
        public string GlobalPrefix { get; set; }
        public Action<JsonSerializerOptions> SerializerOptions { get; set; }
        public IList<Type> Criterias { get; } = new List<Type>();
    }
}