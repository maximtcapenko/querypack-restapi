namespace QueryPack.RestApi.Middlewares
{
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Exceptions;

    internal class ExceptionHandlingMiddleware
    {
        public static async Task HandleAsync(HttpContext context)
        {
            var requestServices = context.RequestServices;
            var messageFactory = requestServices.GetRequiredService<IExceptionHandlingResultFactory>();

            if (messageFactory != null)
            {
                var selector = context.RequestServices.GetRequiredService<OutputFormatterSelector>();
                var writerFactory = context.RequestServices.GetRequiredService<IHttpResponseStreamWriterFactory>();

                var message = await messageFactory.CreateAsync(context);

                var formatterContext = new OutputFormatterWriteContext(context, writerFactory.CreateWriter, message.GetType(), message);
                var selectedFormatter = selector.SelectFormatter(formatterContext, Array.Empty<IOutputFormatter>(), new MediaTypeCollection());
                
                context.Response.StatusCode = message.Status;
                context.Response.ContentType = context.Request.ContentType;
                await selectedFormatter.WriteAsync(formatterContext);
            }
        }
    }
}