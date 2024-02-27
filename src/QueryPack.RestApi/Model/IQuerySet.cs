namespace QueryPack.RestApi.Model
{
    public interface IQuerySet<TModel>
        where TModel : class
    {
        IQueryable<TModel> Query { get; set; }
    }
}