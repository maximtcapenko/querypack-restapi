namespace QueryPack.RestApi.Exceptions
{
    public interface IExceptionHandlingResult
    {
        int Status { get; }
    }

    public interface IExceptionHandlingResultBuilder
    {
        Task<IExceptionHandlingResult> BuildAsync(Exception exception);
        bool CanBuild(Type exceptionType);
    }
}