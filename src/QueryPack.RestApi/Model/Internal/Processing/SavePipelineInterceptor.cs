namespace QueryPack.RestApi.Model.Internal.Processing
{
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Annotations;
    using Meta;

    internal class SavePipelineInterceptor : ISaveChangesInterceptor
    {
        private readonly IServiceProvider _serviceProvider;

        public SavePipelineInterceptor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            eventData.Context.ChangeTracker.DetectChanges();

            foreach (var entry in eventData.Context.ChangeTracker.Entries())
            {
                await ProcessEntryAsync<PreSaveProcessorAttribute>(_serviceProvider, entry);
            }

            return result;
        }

        public async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default(CancellationToken))
        {
            eventData.Context.ChangeTracker.DetectChanges();

            foreach (var entry in eventData.Context.ChangeTracker.Entries())
            {
                await ProcessEntryAsync<PostSaveProcessorAttribute>(_serviceProvider, entry);
            }

            return result;
        }

        private static async Task ProcessEntryAsync<TAnnotation>(IServiceProvider serviceProvider, EntityEntry entry)
            where TAnnotation : class, IPipelineAnnotation
        {
            var annotation = entry.Entity.GetType()
                                  .GetCustomAttributes(typeof(TAnnotation), false)
                                  .FirstOrDefault();

            if (annotation is not null)
            {
                var processType = (annotation as TAnnotation)?.ProcessorType;
                var processor = serviceProvider.GetRequiredService(processType) as IPipelineProcessor;
                await processor.ProcessAsync(entry);
            }
        }
    }
}