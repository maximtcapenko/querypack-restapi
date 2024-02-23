namespace QueryPack.RestApi.Mvc.Model.Binders
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using QueryPack.RestApi.Model.Internal.Criterias;
    using RestApi.Model.Meta;

    internal class KeyCriteriaBinder<TModel> : ICriteriaBinder<TModel>
        where TModel : class
    {
        private const string KeyParameterName = "key";

        public void BindModel(ICriteriaBindingContext<TModel> bindingContext)
        {
            var criteriaKeys = new Dictionary<PropertyMetadata, IEnumerable<object>>();
            var keys = bindingContext.ModelMetadata.GetKeys();

            var result = WebModelExtensions.GetValue(bindingContext.ValueProvider, KeyParameterName);
            if (result != ValueProviderResult.None)
            {
                bindingContext.SetModelValue(KeyParameterName, result);

                if (string.IsNullOrEmpty(result.FirstValue))
                {
                    bindingContext.TryAddModelError(KeyParameterName, result);
                    return;
                }
                else
                {
                    if (keys.Count() != result.Length)
                    {
                        bindingContext.TryAddModelError(KeyParameterName, result);
                        return;
                    }

                    var keyEnum = keys.GetEnumerator();
                    var valueEnum = result.Values.GetEnumerator();

                    while (keyEnum.MoveNext() && valueEnum.MoveNext())
                    {
                        var values = new List<object>();
                        var key = keyEnum.Current;
                        var value = valueEnum.Current;

                        if (key.PropertyType.IsEnum)
                        {
                            if (key.PropertyType.TryConvertEnum(value, out var parameterValue))
                                values.Add(parameterValue);
                        }
                        else
                        {
                            if (key.PropertyType.TryConvert(value, out var parameterValue))
                                values.Add(parameterValue);
                        }

                        if (values.Count == 0)
                        {
                            bindingContext.TryAddModelError(KeyParameterName, result);
                            return;
                        }

                        criteriaKeys[key] = values;
                    }

                }
            }

            bindingContext.SetBindingResult(new QueryCriteria<TModel>(bindingContext.ModelMetadata, criteriaKeys));
        }
    }
}