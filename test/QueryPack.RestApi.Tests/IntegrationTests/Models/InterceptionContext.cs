using Microsoft.EntityFrameworkCore;

namespace QueryPack.RestApi.Tests.IntegrationTests.Models;

public class InterceptionContext : DbContext
{
    public InterceptionContext(DbContextOptions<InterceptionContext> options)
     : base(options)
    { }

    public DbSet<PostPipelineModel> PostPipelineModels { get; set; }
    public DbSet<PrePipelineModel> PrePipelineModels { get; set; }
}