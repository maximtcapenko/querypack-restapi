namespace QueryPack.RestApi.CodeFirstExample.Models
{
    using Model.Annotations;

    public class Version
    {
        public Guid Id { get; set; }
        public int Value { get; set; }
        public string Name { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public Entity Entity { get; set; }
    }
}