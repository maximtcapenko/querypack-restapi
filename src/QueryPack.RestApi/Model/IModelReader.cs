namespace QueryPack.RestApi.Model
{
    internal interface IModelReader<TModel>
         where TModel : class
    {
        Task<TModel> ReadAsync(ICriteria<TModel> criteria);
        Task<IEnumerable<TModel>> ReadAsync();
        Task<Range<TModel>> ReadAsync(ICriteria<TModel> criteria, int first, int last);
    }
}