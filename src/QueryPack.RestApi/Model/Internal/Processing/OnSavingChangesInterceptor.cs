namespace QueryPack.RestApi.Model.Internal.Processing
{
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using System.Collections.Concurrent;
    using System.Linq.Expressions;
    using Annotations;

    internal class OnSavingChangesInterceptor : ISaveChangesInterceptor
    {
        private static ConcurrentDictionary<Type, Func<ISavingChangesProcessor>> _processorFactoryContainer = new();

        public ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(SavingChanges(eventData, result));
        }

        public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            eventData.Context.ChangeTracker.DetectChanges();
            foreach (var entry in eventData.Context.ChangeTracker.Entries())
            {
                ProcessEntry(entry);
            }

            return result;
        }

        public int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            return result;
        }

        public ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(result);
        }

        #region SaveChangesFailed
        public void SaveChangesFailed(DbContextErrorEventData eventData)
        {

        }

        public Task SaveChangesFailedAsync(
            DbContextErrorEventData eventData,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
        #endregion

        private static void ProcessEntry(EntityEntry entry)
        {
            var postSave = entry.Entity.GetType()
                                .GetCustomAttributes(typeof(OnSavingChangesAttribute), false)
                                .FirstOrDefault();

            if (postSave is not null)
            {
                var processType = (postSave as OnSavingChangesAttribute)?.ProcessorType;
                var processorFactory = _processorFactoryContainer.GetOrAdd(processType, type =>
                {
                    var @new = Expression.New(type);
                    return Expression.Lambda<Func<ISavingChangesProcessor>>(@new).Compile();
                });

                var processor = processorFactory();
                processor.Process(entry);
            }
        }
    }
}