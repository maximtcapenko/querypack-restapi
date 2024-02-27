namespace QueryPack.RestApi.Model
{
    public interface ICriteria<TModel> 
        where TModel : class
    {
        void Apply(IQuerySet<TModel> queryset);
    }
}