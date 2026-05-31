namespace QueryPack.RestApi.Exceptions;

public interface IExceptionHandlingResultFactory
{
    Task<IExceptionHandlingResult> CreateAsync(HttpContext httpContext);
}