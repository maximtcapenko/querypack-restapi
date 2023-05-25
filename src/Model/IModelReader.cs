namespace QueryPack.RestApi.Model
{
    public interface IModelReader<TModel>
         where TModel : class
    {
        Task<TModel> ReadAsync(ICriteria<TModel> criteria);
        Task<IEnumerable<TModel>> ReadAsync();
        Task<Range<TModel>> ReadAsync(ICriteria<TModel> criteria, int first, int last);
    }
}