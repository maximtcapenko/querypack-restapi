namespace QueryPack.RestApi.Model
{
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    public interface IPipelineProcessor
    {
        Task ProcessAsync(EntityEntry entry);
    }
}