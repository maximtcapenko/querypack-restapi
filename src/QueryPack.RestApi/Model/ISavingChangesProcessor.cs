namespace QueryPack.RestApi.Model
{
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    
    public interface ISavingChangesProcessor
    {
        void Process(EntityEntry entry);
    }
}