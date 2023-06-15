namespace QueryPack.RestApi.CodeFirstExample.Models
{
    using Model.Annotations;

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
}