namespace QueryPack.RestApi.Exceptions.Internal
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Exceptions;
    using Microsoft.AspNetCore.Diagnostics;

    internal class ExceptionHandlingResultFactoryImpl(IEnumerable<IExceptionHandlingResultBuilder> exceptionHandlerMessageBuilders) : IExceptionHandlingResultFactory
    {
        private readonly IEnumerable<IExceptionHandlingResultBuilder> _exceptionHandlerMessageBuilders = exceptionHandlerMessageBuilders;

        public Task<IExceptionHandlingResult> CreateAsync(HttpContext httpContext)
        {
            var feature = httpContext.Features.Get<IExceptionHandlerFeature>();
            if (feature is null)
                return BuildDefault(new InvalidOperationException("No exception handler feature available on the current context."));

            var exceptionHandlerMessageBuilder = _exceptionHandlerMessageBuilders.FirstOrDefault(e => e.CanBuild(feature.Error.GetType()));
            if (exceptionHandlerMessageBuilder is null)
                return BuildDefault(feature.Error);
            else
                return exceptionHandlerMessageBuilder.BuildAsync(feature.Error);
        }

        private static Task<IExceptionHandlingResult> BuildDefault(Exception ex)
        {
            var exceptoion = ex;
            var errors = new List<string>();

            while (exceptoion != null)
            {
                errors.Add(exceptoion.Message);
                exceptoion = exceptoion.InnerException;
            }

            return Task.FromResult<IExceptionHandlingResult>(new DefaultExceptionMessage
            {
                Errors = errors,
                TraceId = Activity.Current?.Id,
            });
        }

        class DefaultExceptionMessage : IExceptionHandlingResult
        {
            public IEnumerable<string> Errors { get; internal set;}
            public string TraceId { get; internal set;}
            public int Status => 500;
            public string Title => "Internal server error";
        }
    }
}