namespace QueryPack.RestApi.Mvc.Model
{
    public interface ICriteriaBinder<TModel>
        where TModel : class
    {
        void BindModel(ICriteriaBindingContext<TModel> bindingContext);
    }
}