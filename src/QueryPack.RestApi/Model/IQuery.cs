namespace QueryPack.RestApi.Model
{
    public interface IQuery<TModel>
        where TModel : class
    {
        IQueryable<TModel> Query { get; set; }
    }
}