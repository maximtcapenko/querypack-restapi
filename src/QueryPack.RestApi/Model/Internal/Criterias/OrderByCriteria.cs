namespace QueryPack.RestApi.Model.Internal.Criterias
{
    using Extensions;
    using Meta;

    internal class OrderByCriteria<TModel> : ICriteria<TModel>
        where TModel : class
    {
        private readonly Dictionary<PropertyMetadata, OrderDirection> _orderSelectors;
        private readonly ModelMetadata _modelMetadata;

        public OrderByCriteria(ModelMetadata modelMetadata,
            Dictionary<PropertyMetadata, OrderDirection> orderSelectors)
        {
            _orderSelectors = orderSelectors;
            _modelMetadata = modelMetadata;
        }

        public void Apply(IQuerySet<TModel> queryset)
        {
            foreach (var selector in _orderSelectors)
            {
                if (_modelMetadata.Contains(selector.Key))
                    queryset.Query = queryset.Query.OrderBy(selector.Key, _modelMetadata, selector.Value);
            }
        }
    }
}