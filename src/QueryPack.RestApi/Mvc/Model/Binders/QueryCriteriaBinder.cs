namespace QueryPack.RestApi.Mvc.Model.Binders
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using RestApi.Model.Meta;
    using RestApi.Model.Internal.Criterias;
    using System.Linq.Expressions;

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
                    var result = ResolveParameterValues(bindingContext, meta, new List<PropertyMetadata>(), new List<PropertyMetadata>());
                    criteriaKeys[meta] = new List<object> { result };
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

        private Dictionary<string, IEnumerable<object>> ResolveParameterValues(
            ICriteriaBindingContext<TModel> bindingContext,
            PropertyMetadata propertyMetadata,
            List<PropertyMetadata> parent, List<PropertyMetadata> visited)
        {
            visited.Add(propertyMetadata);

            if (!parent.Any()) parent.Add(propertyMetadata);

            var metadataProvider = propertyMetadata.GetModelMetadataProvider();
            var modelMeta = metadataProvider.GetMetadata(propertyMetadata.PropertyType);

            var results = new Dictionary<string, IEnumerable<object>>();

            if (modelMeta != null)
                foreach (var propertyMeta in modelMeta.PropertyMetadata)
                {
                    if (visited.Contains(propertyMeta)) continue;

                    if (propertyMeta.IsNavigation)
                    {
                        var current = new List<PropertyMetadata>();
                        current.AddRange(parent);
                        current.Add(propertyMeta);

                        var nesteds = ResolveParameterValues(bindingContext, propertyMeta, current, visited);
                        foreach (var nested in nesteds)
                        {
                            results.Add($"{nested.Key}", nested.Value);
                        }
                    }
                    else
                    {
                        var result = WebModelExtensions.GetValue(bindingContext.ValueProvider, "{0}.{1}", string.Join(".", parent.Select(e => e.PropertyName)), propertyMeta.PropertyName);
                        if (result != ValueProviderResult.None)
                        {
                            var tmp = new Dictionary<PropertyMetadata, IEnumerable<object>>();
                            TryFill(bindingContext, propertyMeta, result, tmp);
                            results.Add(string.Format("{0}.{1}", string.Join(".", parent.Select(e => e.PropertyName)), propertyMeta.PropertyName), tmp[propertyMeta]);
                        }
                    }
                }

            return results;
        }
    }
}