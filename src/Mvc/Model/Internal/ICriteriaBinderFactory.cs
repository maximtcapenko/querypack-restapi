namespace QueryPack.RestApi.Mvc.Model.Intrnal
{
    interface ICriteriaBinderFactory
    {
        bool CanCreate(Type type);
    }

    internal interface ICriteriaBinderFactory<TModel> : ICriteriaBinderFactory
        where TModel : class
    {
        ICriteriaBinder<TModel> Create();
    }
}