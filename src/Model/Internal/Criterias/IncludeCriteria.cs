namespace QueryPack.RestApi.Model.Internal.Criterias
{
    using Extensions;
    using Microsoft.EntityFrameworkCore;
    using Meta;

    internal class IncludeCriteria<TModel> : ICriteria<TModel>
        where TModel : class
    {
        private readonly IEnumerable<PropertyMetadata> _navigations;
        private readonly ModelMetadata _modelMetadata;

        public IncludeCriteria(ModelMetadata modelMetadata, IEnumerable<PropertyMetadata> navigations)
        {
            _navigations = navigations;
            _modelMetadata = modelMetadata;
        }

        public void Apply(IQueryContainer<TModel> query)
        {
            foreach (var navigation in _navigations)
            {
                if (_modelMetadata.Contains(navigation))
                    query.Query = query.Query.Include(navigation, _modelMetadata);
            }
        }
    }
}