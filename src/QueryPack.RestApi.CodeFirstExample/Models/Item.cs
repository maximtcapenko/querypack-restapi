namespace QueryPack.RestApi.CodeFirstExample.Models
{
    using Model.Annotations;

    public class Item
    {
        public Guid Id { get; set; }
        [TextSearch]
        public string Role { get; set; }
        public Item Internal { get; set; }
    }
}