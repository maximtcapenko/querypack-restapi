namespace QueryPack.RestApi.Mvc.Model
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using RestApi.Model;

    internal class CriteriaBindingContext<TModel> : ICriteriaBindingContext<TModel>
        where TModel : class
    {
        private readonly ModelBindingContext _bindingContext;
        private readonly List<ICriteria<TModel>> _criterias;
        private readonly RestApi.Model.Meta.IModelMetadataProvider _modelMetadataProvider;

        public CriteriaBindingContext(ModelBindingContext bindingContext,
            List<ICriteria<TModel>> criterias, RestApi.Model.Meta.IModelMetadataProvider modelMetadataProvider)
        {
            _bindingContext = bindingContext;
            _criterias = criterias;
            _modelMetadataProvider = modelMetadataProvider;
        }

        public IValueProvider ValueProvider => _bindingContext.ValueProvider;

        public RestApi.Model.Meta.ModelMetadata ModelMetadata => _modelMetadataProvider.GetMetadata(typeof(TModel));

        public bool TryAddModelError(string key, ValueProviderResult valueProviderResult)
        {
            return _bindingContext.ModelState.TryAddModelError(key,
                            _bindingContext.ModelMetadata.ModelBindingMessageProvider.ValueIsInvalidAccessor(
                                valueProviderResult.ToString()));
        }

        public void SetModelValue(string key, ValueProviderResult valueProviderResult)
        {
            _bindingContext.ModelState.SetModelValue(key, valueProviderResult);
        }

        public void SetBindingResult(ICriteria<TModel> result)
        {
            _criterias.Add(result);
        }
    }
}