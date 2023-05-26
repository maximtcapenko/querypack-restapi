namespace QueryPack.RestApi.Example.Models
{
    using Microsoft.EntityFrameworkCore;

    public class ModelsContext : DbContext
    {
        public DbSet<Entity> Entities { get; set; }
        public DbSet<Version> Versions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("test");
        }
    }
}