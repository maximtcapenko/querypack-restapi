namespace QueryPack.RestApi.Mvc.Model.Binders
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using QueryPack.RestApi.Model.Internal.Criterias;
    using RestApi.Model;
    using RestApi.Model.Meta;

    internal class OrderByCriteriaBinder<TModel> : ICriteriaBinder<TModel>
          where TModel : class
    {
        private const string OrderByParameterName = "OrderBy";

        public void BindModel(ICriteriaBindingContext<TModel> bindingContext)
        {
            var properties = bindingContext.ModelMetadata.GetRegularProperties();
            var parameters = new Dictionary<PropertyMetadata, ValueProviderResult>();

            foreach (var property in properties)
            {
                var orderbyResult = Resolve(bindingContext.ValueProvider, OrderByParameterName, property.PropertyName);
                if (orderbyResult != ValueProviderResult.None)
                {
                    parameters[property] = orderbyResult;
                }
            }

            var keys = new Dictionary<PropertyMetadata, OrderDirection>();

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
                    if (typeof(OrderDirection).TryConvertEnum(parameter.Value.FirstValue, out var parameterValue))
                        keys[parameter.Key] = (OrderDirection)parameterValue;
                }

                if (!keys.ContainsKey(parameter.Key))
                {
                    bindingContext.TryAddModelError(parameter.Key.PropertyName, parameter.Value);

                    return;
                }
            }

            bindingContext.SetBindingResult(new OrderByCriteria<TModel>(bindingContext.ModelMetadata, keys));
        }

        private ValueProviderResult Resolve(IValueProvider valueProvider, string queryParameter, string name)
        {
            var patterns = new[] { "{0}[{1}]", "{0}.{1}" };

            foreach (var pattern in patterns)
            {
                var result = WebModelExtensions.GetValue(valueProvider, pattern, queryParameter, name);
                if (result != ValueProviderResult.None)
                    return result;
            }

            return ValueProviderResult.None;
        }
    }
}
