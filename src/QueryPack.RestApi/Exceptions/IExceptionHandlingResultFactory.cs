using Microsoft.AspNetCore.Diagnostics;

namespace QueryPack.RestApi.Exceptions
{
    public interface IExceptionHandlingResultFactory
    {
        Task<IExceptionHandlingResult> CreateAsync(HttpContext httpContext);
    }
}