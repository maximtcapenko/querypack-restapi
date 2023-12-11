namespace QueryPack.RestApi.Extensions
{
    using Microsoft.EntityFrameworkCore;
    using Model.Internal.Processing;

    public static class DbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder EnableModelPipelineAnnotations(this DbContextOptionsBuilder self, IServiceProvider serviceProvider)
        {
            self.AddInterceptors(new SavePipelineInterceptor(serviceProvider));
            return self;
        }
    }
}