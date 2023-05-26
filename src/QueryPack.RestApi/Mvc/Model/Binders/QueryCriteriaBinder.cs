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

            var parameters = new Dictionary<PropertyMetadata, ValueProviderResult>();

            foreach (var meta in modelMeta.GetRegularProperties())
            {
                var result = WebModelExtensions.GetValue(bindingContext.ValueProvider, meta.PropertyName);
                if (result != ValueProviderResult.None)
                {
                    parameters[meta] = result;
                }
            }

            var criteriaKeys = new Dictionary<PropertyMetadata, object[]>();

            foreach (var parameter in parameters)
            {
                bindingContext.SetModelValue(parameter.Key.PropertyName, parameter.Value);

                if (string.IsNullOrEmpty(parameter.Value.FirstValue))
                {
                    bindingContext.TryAddModelError(parameter.Key.PropertyName, parameter.Value);

                    return;
                }
                else
                {
                    var values = new List<object>(parameter.Value.Count());
                    
                    foreach (var value in parameter.Value)
                    {
                        if (parameter.Key.PropertyType.IsEnum)
                        {
                            if (parameter.Key.PropertyType.TryConvertEnum(value, out var parameterValue))
                                values.Add(parameterValue);
                        }
                        else
                        {
                            if (parameter.Key.PropertyType.TryConvert(value, out var parameterValue))
                                values.Add(parameterValue);
                        }

                    }

                    if (values.Count() == 0 || values.Count() != parameter.Value.Count())
                    {
                        bindingContext.TryAddModelError(parameter.Key.PropertyName, parameter.Value);

                        return;
                    }

                    criteriaKeys[parameter.Key] = values.ToArray();
                }
            }

            bindingContext.SetBindingResult(new QueryCriteria<TModel>(modelMeta, criteriaKeys));
        }
    }
}