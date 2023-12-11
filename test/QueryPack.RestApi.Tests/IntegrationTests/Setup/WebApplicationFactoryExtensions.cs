using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using QueryPack.RestApi.Extensions;

namespace QueryPack.RestApi.Tests.IntegrationTests.Setup;

public static class WebApplicationFactoryExtensions
{
    public static WebApplicationFactory<Program> AsCodeFirstModelContextWebApp<TContext>(this WebApplicationFactory<Program> applicationFactory)
       where TContext : DbContext
       => applicationFactory.WithWebHostBuilder(builder =>
       {
           builder.ConfigureTestServices(AddTestCodeFirstRestModel<TContext>);
       });

    public static void AddTestCodeFirstRestModel<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        services.AddRestModel<TContext>(options =>
        {
            options.GlobalApiPrefix = "/api";
            options.ContextOptionsBuilder = (servieProvider, dbContextOptionsBuilder)
                => dbContextOptionsBuilder.UseInMemoryDatabase("test")
                                          .EnableModelPipelineAnnotations(servieProvider);

            options.SerializerOptions = SerializerOptions =>
            {
                SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            };
        });
    }
}