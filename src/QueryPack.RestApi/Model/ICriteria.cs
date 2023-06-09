namespace QueryPack.RestApi.Model
{
    public interface ICriteria<TModel> 
        where TModel : class
    {
        void Apply(IQueryContainer<TModel> query);
    }
}