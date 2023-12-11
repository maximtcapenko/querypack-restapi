using QueryPack.RestApi.Model.Annotations;

namespace QueryPack.RestApi.Tests.IntegrationTests.Models;

public class Item
{
    public Guid Id { get; set; }
    
    [TextSearch]
    public string Role { get; set; }
    public Item Internal { get; set; }
}