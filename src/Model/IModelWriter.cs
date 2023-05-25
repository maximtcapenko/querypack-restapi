namespace QueryPack.RestApi.Model
{
    public interface IModelWriter<TModel> where TModel : class
    {
        Task WriteAsync(TModel model);
        Task Delete(TModel model);
    }
}