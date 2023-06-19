namespace QueryPack.RestApi.Exceptions
{
    using Middlewares;

    public static class ExceptionMiddlewareExtensions
    {
        public static void UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(builder =>
            {
                builder.Run(ExceptionHandlingMiddleware.HandleAsync);
            });
        }
    }
}