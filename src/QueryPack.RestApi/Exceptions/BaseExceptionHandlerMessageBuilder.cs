namespace QueryPack.RestApi.Exceptions
{
    public abstract class BaseExceptionHandlerMessageBuilder<TException> : IExceptionHandlingResultBuilder
        where TException : Exception
    {
        public Task<IExceptionHandlingResult> BuildAsync(Exception exception) 
            => BuildAsync(exception);

        public bool CanBuild(Type exceptionType) => typeof(TException) == exceptionType;

        protected abstract Task<IExceptionHandlingResult> BuildAsync(TException exception);
    }
}