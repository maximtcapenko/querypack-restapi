namespace QueryPack.RestApi.Example.Models
{
    using Microsoft.EntityFrameworkCore;

    public class ModelsContext : DbContext
    {
        public DbSet<Entity> Entities { get; set; }
        public DbSet<Version> Versions { get; set; }
        public DbSet<Dependency> Dependencies { get; set; }
        public DbSet<Item> Items { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("test");
        }
    }
}