using Microsoft.EntityFrameworkCore.ChangeTracking;
using QueryPack.RestApi.Model;
using QueryPack.RestApi.Model.Annotations;

namespace QueryPack.RestApi.Tests.IntegrationTests.Models;

[PostSaveProcessor(typeof(PostSaveModelProcessor))]
public class PostPipelineModel
{
    public Guid Id { get; set; }

    public string Value { get; set; }
}


public class PostSaveModelProcessor : IPipelineProcessor
{
    public PostPipelineModel CapturedModelInstance { get; set; }

    public Task ProcessAsync(EntityEntry entry)
    {
        CapturedModelInstance = entry.Entity as PostPipelineModel;

        return Task.CompletedTask;
    }
}