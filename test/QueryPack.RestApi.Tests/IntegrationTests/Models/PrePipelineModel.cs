using Microsoft.EntityFrameworkCore.ChangeTracking;
using QueryPack.RestApi.Model;
using QueryPack.RestApi.Model.Annotations;

namespace QueryPack.RestApi.Tests.IntegrationTests.Models;

[PreSaveProcessor(typeof(PreSaveModelProcessor))]
public class PrePipelineModel
{
    public Guid Id { get; set; }

    public string Value { get; set; }
    public string Version { get; set; }
}


public class PreSaveModelProcessor : IPipelineProcessor
{
    public Task ProcessAsync(EntityEntry entry)
    {
        var instance = entry.Entity as PrePipelineModel;
        instance.Version = "v1.0";
        
        return Task.CompletedTask;
    }
}