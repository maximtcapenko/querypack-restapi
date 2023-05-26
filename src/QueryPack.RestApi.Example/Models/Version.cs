namespace QueryPack.RestApi.Example.Models
{
    public class Version
    {
        public Guid Id {get;set;}
        public int Value { get; set; }
        public string Name { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public Entity Entity { get; set; }
    }
}