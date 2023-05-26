namespace QueryPack.RestApi.Example.Tasks
{
    using Microsoft.EntityFrameworkCore;
    using Models;

    internal class DbSeedTask : IHostedService
    {
        private readonly IServiceProvider _factory;

        public DbSeedTask(IServiceProvider factory)
        {
            _factory = factory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var random = new Random();
            using var scope = _factory.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ModelsContext>();

            var entities = Enumerable.Range(0, 100).Select((index, e) => new Entity
            {
                Id = Guid.NewGuid(),
                Name = $"ent_{index}",
                CreatedAt = DateTimeOffset.UtcNow,
                Versions = GenerateVersions(random.Next(0, 9)).ToList()
            });
            foreach (var entity in entities)
            {
                context.Add(entity);
            }

            await context.SaveChangesAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        IEnumerable<Version> GenerateVersions(int count) =>
     Enumerable.Range(0, count).Select((index, e) => new Version
     {
         Id = Guid.NewGuid(),
         Value = index,
         Name = $"ver_{index}",
         CreatedAt = DateTimeOffset.UtcNow,
     });
    }
}