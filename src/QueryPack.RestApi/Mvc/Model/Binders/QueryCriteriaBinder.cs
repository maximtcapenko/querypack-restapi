namespace QueryPack.RestApi.Mvc.Model.Binders
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using RestApi.Model.Meta;
    using RestApi.Model.Internal.Criterias;

    internal class QueryCriteriaBinder<TModel> : ICriteriaBinder<TModel>
          where TModel : class
    {
        public void BindModel(ICriteriaBindingContext<TModel> bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext, nameof(bindingContext));

            var modelMeta = bindingContext.ModelMetadata;
            var criteriaKeys = new Dictionary<PropertyMetadata, IEnumerable<object>>();

            foreach (var propertyMetadata in modelMeta.PropertyMetadata)
            {
                if (propertyMetadata.IsNavigation)
                {
                    var results = new Dictionary<string, IEnumerable<object>>();
                    if (!ResolveParameterValues(bindingContext, propertyMetadata, new List<PropertyMetadata>(), new List<PropertyMetadata>(), results))
                        return;

                    if (results.Any())
                        criteriaKeys[propertyMetadata] = new List<object> { results };
                }
                else
                {
                    var result = WebModelExtensions.GetValue(bindingContext.ValueProvider, propertyMetadata.PropertyName);
                    if (result != ValueProviderResult.None)
                    {
                        bindingContext.SetModelValue(propertyMetadata.PropertyName, result);

                        if (string.IsNullOrEmpty(result.FirstValue))
                        {
                            bindingContext.TryAddModelError(propertyMetadata.PropertyName, result);

                            return;
                        }
                        else
                        {
                            var values = new List<object>();
                            if (!TryFill(bindingContext, propertyMetadata.PropertyName, propertyMetadata.PropertyType, result, values))
                                return;

                            criteriaKeys[propertyMetadata] = values;
                        }
                    }
                }
            }

            bindingContext.SetBindingResult(new QueryCriteria<TModel>(modelMeta, criteriaKeys));
        }

        private static bool TryFill(ICriteriaBindingContext<TModel> bindingContext,
                   string propertyName, Type propertyType, ValueProviderResult valueProviderResult,
                    List<object> results)
        {
            var values = new List<object>(valueProviderResult.Count());

            foreach (var value in valueProviderResult)
            {
                if (propertyType.IsEnum)
                {
                    if (propertyType.TryConvertEnum(value, out var parameterValue))
                        values.Add(parameterValue);
                }
                else
                {
                    if (propertyType.TryConvert(value, out var parameterValue))
                        values.Add(parameterValue);
                }
            }

            if (values.Count == 0 || values.Count != valueProviderResult.Length)
            {
                bindingContext.TryAddModelError(propertyName, valueProviderResult);
                return false;
            }

            results.AddRange(values);

            return true;
        }

        private bool ResolveParameterValues(
            ICriteriaBindingContext<TModel> bindingContext,
            PropertyMetadata propertyMetadata,
            List<PropertyMetadata> parent, List<PropertyMetadata> visited, Dictionary<string, IEnumerable<object>> results)
        {
            visited.Add(propertyMetadata);

            if (!parent.Any()) parent.Add(propertyMetadata);

            var metadataProvider = propertyMetadata.GetModelMetadataProvider();
            var modelMeta = metadataProvider.GetMetadata(propertyMetadata.PropertyType);

            if (modelMeta != null)
            {
                foreach (var propertyMeta in modelMeta.PropertyMetadata)
                {
                    if (visited.Contains(propertyMeta)) continue;

                    if (propertyMeta.IsNavigation)
                    {
                        var current = new List<PropertyMetadata>();
                        current.AddRange(parent);
                        current.Add(propertyMeta);

                        if (!ResolveParameterValues(bindingContext, propertyMeta, current, visited, results))
                            return false;
                    }
                    else
                    {
                        var key = string.Format("{0}.{1}", string.Join(".", parent.Select(e => e.PropertyName)), propertyMeta.PropertyName);
                        var result = WebModelExtensions.GetValue(bindingContext.ValueProvider, "{0}.{1}", string.Join(".", parent.Select(e => e.PropertyName)), propertyMeta.PropertyName);
                        if (result != ValueProviderResult.None)
                        {
                            bindingContext.SetModelValue(key, result);

                            var tmp = new List<object>();
                            if (!TryFill(bindingContext, key, propertyMeta.PropertyType, result, tmp))
                                return false;

                            results.Add(key, tmp);
                        }
                    }
                }
            }

            return true;
        }
    }
}