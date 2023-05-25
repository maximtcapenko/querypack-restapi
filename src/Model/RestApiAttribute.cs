namespace QueryPack.RestApi.Model
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RestApiAttribute : Attribute
    {
        public string Route { get; set; }

        public RestApiAttribute(string route)
        {
            Route = route;
        }
    }
}