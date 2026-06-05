namespace QueryPack.RestApi.Model;


[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class RestApiAttribute(string route) : Attribute
{
    public string Route { get; set; } = route;
}