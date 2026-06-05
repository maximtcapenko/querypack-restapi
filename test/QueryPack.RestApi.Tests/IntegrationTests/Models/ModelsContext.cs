using Microsoft.EntityFrameworkCore;

namespace QueryPack.RestApi.Tests.IntegrationTests.Models;

public class ModelsContext(DbContextOptions<ModelsContext> options) : DbContext(options)
{
    public DbSet<Entity> Entities { get; set; }
    public DbSet<Version> Versions { get; set; }
    public DbSet<Dependency> Dependencies { get; set; }
    public DbSet<Item> Items { get; set; }
}
