using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using QueryPack.RestApi.Tests.IntegrationTests.Models;
using QueryPack.RestApi.Tests.IntegrationTests.Setup;
using Xunit;

namespace QueryPack.RestApi.Tests.IntegrationTests.Tests;

public class CodeFirstInterceptionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _applicationFactory;

    public CodeFirstInterceptionTests(WebApplicationFactory<Program> applicationFactory)
    {
        _applicationFactory = applicationFactory.AsCodeFirstModelContextWebApp<InterceptionContext>();
    }

    [Theory, AutoData]
    public async Task When_create_new_record_with_post_save_processing_processor_should_be_invoked(PostPipelineModel instance)
    {
        using var scope = _applicationFactory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InterceptionContext>();
        await context.AddAsync(instance);
        await context.SaveChangesAsync();

        var processor = scope.ServiceProvider.GetRequiredService<PostSaveModelProcessor>();
        processor.CapturedModelInstance.Should().NotBeNull();
        processor.CapturedModelInstance.Should().BeEquivalentTo(instance);
    }

    [Theory, AutoData]
    public async Task When_create_new_record_with_pre_save_processing_processor_should_be_invoked(PrePipelineModel instance)
    {
        using var scope = _applicationFactory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InterceptionContext>();
        await context.AddAsync(instance);
        await context.SaveChangesAsync();

        instance.Version.Should().Be("v1.0");
    }
}