namespace QueryPack.RestApi.Example.Models
{
    public class Entity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public ICollection<Version> Versions { get; set; }
    }
}