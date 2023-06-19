using QueryPack.RestApi.Middlewares;

namespace QueryPack.RestApi.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseCustomExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(builder =>
            {
                builder.Run(ExceptionHandlingMiddleware.HandleAsync);
            });
        }
    }
}