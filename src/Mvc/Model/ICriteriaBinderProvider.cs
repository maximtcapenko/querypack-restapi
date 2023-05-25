namespace QueryPack.RestApi.Mvc.Model
{
    public interface ICriteriaBinderProvider
    {
        IEnumerable<ICriteriaBinder<TModel>> GetBinders<TModel>()
            where TModel : class;
    }
}