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
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelMeta = bindingContext.ModelMetadata;
            var criteriaKeys = new Dictionary<PropertyMetadata, IEnumerable<object>>();

            foreach (var meta in modelMeta.PropertyMetadata)
            {
                if (meta.IsNavigation)
                {
                    var results = new Dictionary<string, IEnumerable<object>>();
                    if (!ResolveParameterValues(bindingContext, meta, new List<PropertyMetadata>(), new List<PropertyMetadata>(), results))
                        return;

                    criteriaKeys[meta] = new List<object> { results };
                }
                else
                {
                    var result = WebModelExtensions.GetValue(bindingContext.ValueProvider, meta.PropertyName);
                    if (result != ValueProviderResult.None)
                    {
                        bindingContext.SetModelValue(meta.PropertyName, result);

                        if (string.IsNullOrEmpty(result.FirstValue))
                        {
                            bindingContext.TryAddModelError(meta.PropertyName, result);

                            return;
                        }
                        else
                        {
                            if (!TryFill(bindingContext, meta, result, criteriaKeys))
                                return;
                        }
                    }
                }
            }

            bindingContext.SetBindingResult(new QueryCriteria<TModel>(modelMeta, criteriaKeys));
        }

        private bool TryFill(ICriteriaBindingContext<TModel> bindingContext,
            PropertyMetadata propertyMetadata, ValueProviderResult valueProviderResult,
            Dictionary<PropertyMetadata, IEnumerable<object>> results)
        {
            var values = new List<object>(valueProviderResult.Count());

            foreach (var value in valueProviderResult)
            {
                if (propertyMetadata.PropertyType.IsEnum)
                {
                    if (propertyMetadata.PropertyType.TryConvertEnum(value, out var parameterValue))
                        values.Add(parameterValue);
                }
                else
                {
                    if (propertyMetadata.PropertyType.TryConvert(value, out var parameterValue))
                        values.Add(parameterValue);
                }

                if (values.Count() == 0 || values.Count() != valueProviderResult.Count())
                {
                    bindingContext.TryAddModelError(propertyMetadata.PropertyName, valueProviderResult);
                    return false;
                }

                results[propertyMetadata] = values;
            }

            return true;
        }
        private bool TryFill(ICriteriaBindingContext<TModel> bindingContext,
                   string propertyName, Type propertyType, ValueProviderResult valueProviderResult,
                   Dictionary<string, IEnumerable<object>> results)
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
            
            if (values.Count() == 0 || values.Count() != valueProviderResult.Count())
            {
                bindingContext.TryAddModelError(propertyName, valueProviderResult);
                return false;
            }

            results[propertyName] = values;

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

                            var tmp = new Dictionary<string, IEnumerable<object>>();
                            if (!TryFill(bindingContext, key, propertyMeta.PropertyType, result, tmp))
                                return false;

                            results.Add(key, tmp[key]);
                        }
                    }
                }

            return true;
        }
    }
}