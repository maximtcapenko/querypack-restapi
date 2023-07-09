namespace QueryPack.RestApi.Model
{
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    public abstract class BaseSavingChangesProcessor<TModel> : ISavingChangesProcessor
        where TModel : class
    {
        public void Process(EntityEntry entry)
        {
            if (typeof(TModel) == entry.Entity.GetType())
            {
                Process(entry.Entity as TModel);
            }
        }

        protected abstract void Process(TModel model);
    }
}