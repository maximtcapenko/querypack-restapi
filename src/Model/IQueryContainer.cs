namespace QueryPack.RestApi.Model
{
    public interface IQueryContainer<TModel>
        where TModel : class
    {
        IQueryable<TModel> Query { get; set; }
    }
}