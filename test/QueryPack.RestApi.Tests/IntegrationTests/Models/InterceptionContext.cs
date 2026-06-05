using Microsoft.EntityFrameworkCore;

namespace QueryPack.RestApi.Tests.IntegrationTests.Models;

public class InterceptionContext(DbContextOptions<InterceptionContext> options) : DbContext(options)
{
    public DbSet<PostPipelineModel> PostPipelineModels { get; set; }
    public DbSet<PrePipelineModel> PrePipelineModels { get; set; }
}