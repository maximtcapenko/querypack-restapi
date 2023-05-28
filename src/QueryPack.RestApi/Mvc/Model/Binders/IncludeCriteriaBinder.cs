namespace QueryPack.RestApi.Mvc.Model.Binders
{
    using Humanizer;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using RestApi.Model.Internal.Criterias;
    using RestApi.Model.Meta;

    internal class IncludeCriteriaBinder<TModel> : ICriteriaBinder<TModel>
          where TModel : class
    {
        private const string IncludeParametrerName = "Include";

        public void BindModel(ICriteriaBindingContext<TModel> bindingContext)
        {
            var includeResult = WebModelExtensions.GetValue(bindingContext.ValueProvider, IncludeParametrerName);
            if (includeResult != ValueProviderResult.None)
            {
                var navigations = bindingContext.ModelMetadata.GetNavigations().Where(e => Compare(e, includeResult));

                bindingContext.SetBindingResult(new IncludeCriteria<TModel>(bindingContext.ModelMetadata, navigations));
            }
        }

        static bool Compare(RestApi.Model.Meta.PropertyMetadata propertyMetadata, ValueProviderResult valueProviderResult)
        {
            var name = propertyMetadata.PropertyName;
            // try origin name
            var result = valueProviderResult.Contains(name);
            if (result) return result;

            result = valueProviderResult.Contains(name.ToLower());
            if (result) return result;

            result = valueProviderResult.Contains(name.ToUpper());
            if (result) return result;

            // camel case  
            result = valueProviderResult.Contains(name.Camelize());
            if (result) return result;

            // snake case
            result = valueProviderResult.Contains(name.Underscore());
            if (result) return result;

            result = valueProviderResult.Contains(name.Kebaberize());
            if (result) return result;

            return false;
        }
    }
}