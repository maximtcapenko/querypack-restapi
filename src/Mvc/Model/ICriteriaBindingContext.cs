namespace QueryPack.RestApi.Mvc.Model
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using RestApi.Model;

    public interface ICriteriaBindingContext<TModel>
        where TModel : class
    {
        IValueProvider ValueProvider { get; }
        RestApi.Model.Meta.ModelMetadata ModelMetadata { get; }
        bool TryAddModelError(string key, ValueProviderResult valueProviderResult);
        void SetModelValue(string key, ValueProviderResult valueProviderResult);
        void SetBindingResult(ICriteria<TModel> result);
    }
}