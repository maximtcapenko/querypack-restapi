using System.Net;
using System.Net.Http.Json;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using QueryPack.RestApi.Model;
using QueryPack.RestApi.Tests.IntegrationTests.Models;
using QueryPack.RestApi.Tests.IntegrationTests.Setup;
using Xunit;

namespace QueryPack.RestApi.Tests.IntegrationTests.Tests;

public class CodeFirstTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string BasePath = "/api";
    private readonly WebApplicationFactory<Program> _applicationFactory;

    public CodeFirstTests(WebApplicationFactory<Program> applicationFactory)
    {
        _applicationFactory = applicationFactory.AsCodeFirstModelContextWebApp<ModelsContext>();
    }

    [Fact]
    public async Task Get_all_records_should_return_success()
    {
        var client = _applicationFactory.CreateClient();

        await InitTestDatabaseAsync(_applicationFactory);

        var readResponse = await client.GetAsync($"{BasePath}/entities");
        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var entities = await readResponse.Content.ReadFromJsonAsync<IEnumerable<Entity>>();
        entities.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Get_range_with_include_nested_and_ordered_desc_should_return_success()
    {
        var client = _applicationFactory.CreateClient();

        await InitTestDatabaseAsync(_applicationFactory);

        var readResponse = await client.GetAsync($"{BasePath}/entities/range?include=versions&first=0&last=9&order_by[name]=desc");
        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var range = await readResponse.Content.ReadFromJsonAsync<Range<Entity>>();
        range.ResultCount.Should().Be(10);
        range.Results.All(e => e.Versions.Any()).Should().BeTrue();

        var first = range.Results.First();
        first.Name.Should().Be("ent_99");
        first.Versions.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Get_all_records_where_name_starts_from_and_ordered_desc_should_return_success()
    {
        var client = _applicationFactory.CreateClient();

        await InitTestDatabaseAsync(_applicationFactory);

        var readResponse = await client.GetAsync($"{BasePath}/entities?include=versions&name=ent_2&order_by[name]=desc");
        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var entities = await readResponse.Content.ReadFromJsonAsync<IEnumerable<Entity>>();
        entities.Should().NotBeNullOrEmpty();
        entities.All(e => e.Name.StartsWith("ent_2")).Should().BeTrue();
        entities.All(e => e.Versions.Any()).Should().BeTrue();
    }

    [Fact]
    public async Task Get_records_where_name_starts_from_variants_and_ordered_desc_should_return_success()
    {
        var client = _applicationFactory.CreateClient();

        await InitTestDatabaseAsync(_applicationFactory);

        var readResponse = await client.GetAsync($"{BasePath}/entities?include=versions&name=ent_2&name=ent_5&order_by[name]=desc");
        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var entities = await readResponse.Content.ReadFromJsonAsync<IEnumerable<Entity>>();
        entities.Should().NotBeNullOrEmpty();
        entities.Any(e => e.Name.StartsWith("ent_2")).Should().BeTrue();
        entities.Any(e => e.Name.StartsWith("ent_5")).Should().BeTrue();
        entities.All(e => e.Versions.Any()).Should().BeTrue();
    }

    [Fact]
    public async Task Get_record_by_id_should_return_success()
    {
        var client = _applicationFactory.CreateClient();

        await InitTestDatabaseAsync(_applicationFactory);

        var readResponse = await client.GetAsync($"{BasePath}/entities/range?first=0&last=1");
        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var range = await readResponse.Content.ReadFromJsonAsync<Range<Entity>>();
        var firstEntity = range.Results.First();

        readResponse = await client.GetAsync($"{BasePath}/entities/{firstEntity.Id}");
        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var entity = await readResponse.Content.ReadFromJsonAsync<Entity>();
        firstEntity.Should().BeEquivalentTo(entity);
    }

    [Fact]
    public async Task Get_record_by_id_should_return_notfound()
    {
        var client = _applicationFactory.CreateClient();

        await InitTestDatabaseAsync(_applicationFactory);

        var readResponse = await client.GetAsync($"{BasePath}/entities/{Guid.NewGuid()}");
        readResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [AutoData, Theory]
    public async Task When_create_new_record_then_should_return_success(string name)
    {
        var record = new Entity
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTimeOffset.UtcNow,
            Versions = GenerateVersions(3).ToList()
        };

        var client = _applicationFactory.CreateClient();
        var response = await client.PostAsJsonAsync($"{BasePath}/entities", record);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var keys = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        keys.Should().NotBeEmpty();
        keys.Should().ContainKey("id");

        var readResponse = await client.GetAsync($"{BasePath}/entities/single?id={keys["id"]}&include=versions");
        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var entity = await readResponse.Content.ReadFromJsonAsync<Entity>();
        entity.Should().NotBeNull();
        entity.Id.Should().Be(keys["id"]);
        entity.Name.Should().Be(name);
        entity.Versions.Should().HaveCount(3);
    }

    [AutoData, Theory]
    public async Task When_create_new_record_with_attachment_then_should_return_success(string name)
    {
        var client = _applicationFactory.CreateClient();
        await InitTestDatabaseAsync(_applicationFactory);

        var versions = await GetAllVersions(client);

        var record = new Entity
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTimeOffset.UtcNow,
            Versions = versions.Take(3).ToList()
        };

        var response = await client.PostAsJsonAsync($"{BasePath}/entities", record);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var keys = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        keys.Should().NotBeEmpty();
        keys.Should().ContainKey("id");

        var readResponse = await client.GetAsync($"{BasePath}/entities/single?id={keys["id"]}&include=versions");
        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var entity = await readResponse.Content.ReadFromJsonAsync<Entity>();
        entity.Should().NotBeNull();
        entity.Id.Should().Be(keys["id"]);
        entity.Name.Should().Be(name);
        entity.Versions.Should().HaveCount(3);

        var checkVersions = await GetAllVersions(client);
        checkVersions.Should().HaveCount(versions.Count());
    }

    [AutoData, Theory]
    public async Task When_create_new_record_with_attachment_then_should_add_new_attachemnet_and_return_success(string name)
    {
        var client = _applicationFactory.CreateClient();
        await InitTestDatabaseAsync(_applicationFactory);

        var versions = await GetAllVersions(client);

        var record = new Entity
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTimeOffset.UtcNow,
            Versions = versions.Take(3).Concat(new[] { new Models.Version
            {
                 Id = Guid.NewGuid(),
                  Name = name,
                  CreatedAt = DateTimeOffset.UtcNow
            } }).ToList()
        };

        var response = await client.PostAsJsonAsync($"{BasePath}/entities", record);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var keys = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        keys.Should().NotBeEmpty();
        keys.Should().ContainKey("id");

        var readResponse = await client.GetAsync($"{BasePath}/entities/single?id={keys["id"]}&include=versions");
        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var entity = await readResponse.Content.ReadFromJsonAsync<Entity>();
        entity.Should().NotBeNull();
        entity.Id.Should().Be(keys["id"]);
        entity.Name.Should().Be(name);
        entity.Versions.Should().HaveCount(4);

        var checkVersions = await GetAllVersions(client);
        checkVersions.Should().HaveCount(versions.Count() + 1);
    }

    private static Task<IEnumerable<Models.Version>> GetAllVersions(HttpClient client)
        => client.GetFromJsonAsync<IEnumerable<Models.Version>>($"{BasePath}/versions");

    private static async Task InitTestDatabaseAsync(WebApplicationFactory<Program> webApplicationFactory)
    {
        using var scope = webApplicationFactory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ModelsContext>();

        var random = new Random();
        var entities = Enumerable.Range(1, 100).Select((index, e) => new Entity
        {
            Id = Guid.NewGuid(),
            Name = $"ent_{index}",
            CreatedAt = DateTimeOffset.UtcNow,
            Versions = GenerateVersions(random.Next(1, 9)).ToList()
        });

        foreach (var entity in entities)
        {
            context.Add(entity);
        }

        await context.SaveChangesAsync();
    }

    private static IEnumerable<Models.Version> GenerateVersions(int count)
        => Enumerable.Range(0, count).Select((index, e) => new Models.Version
            {
                Id = Guid.NewGuid(),
                Value = index,
                Name = $"ver_{index}",
                CreatedAt = DateTimeOffset.UtcNow,
            });
}