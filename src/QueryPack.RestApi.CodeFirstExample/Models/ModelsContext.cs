namespace QueryPack.RestApi.CodeFirstExample.Models
{
    using Microsoft.EntityFrameworkCore;

    public class ModelsContext : DbContext
    {
        public ModelsContext(DbContextOptions<ModelsContext> options) : base(options) 
        { }
        public DbSet<Entity> Entities { get; set; }
        public DbSet<Version> Versions { get; set; }
        public DbSet<Dependency> Dependencies { get; set; }
        public DbSet<Item> Items { get; set; }
    }
}