using System.ComponentModel.DataAnnotations;
using QueryPack.RestApi.Model.Annotations;

namespace QueryPack.RestApi.Tests.IntegrationTests.Models;

public class Entity
{
    public Guid Id { get; set; }
    
    [TextSearch]
    public string Name { get; set; }

    public Dependency Dependency { get; set; }
    public Item Item { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public ICollection<Version> Versions { get; set; }
}