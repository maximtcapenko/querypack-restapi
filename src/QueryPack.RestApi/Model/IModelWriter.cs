namespace QueryPack.RestApi.Model
{
    internal interface IModelWriter<TModel> where TModel : class
    {
        Task WriteAsync(TModel model);
        Task Delete(TModel model);
    }
}