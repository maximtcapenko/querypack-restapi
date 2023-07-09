namespace QueryPack.RestApi.Extensions
{
    using Microsoft.EntityFrameworkCore;
    using Model.Internal.Processing;

    public static class DbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder EnableOnSavingChangesAnnotations(this DbContextOptionsBuilder self)
        {
            self.AddInterceptors(new OnSavingChangesInterceptor());
            return self;
        }
    }
}