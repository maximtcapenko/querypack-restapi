namespace QueryPack.RestApi.Model.Internal.Criterias
{
    using Extensions;
    using Microsoft.EntityFrameworkCore;
    using Meta;

    internal class IncludeCriteria<TModel>(ModelMetadata modelMetadata, IEnumerable<PropertyMetadata> navigations) : ICriteria<TModel>
        where TModel : class
    {
        private readonly IEnumerable<PropertyMetadata> _navigations = navigations;
        private readonly ModelMetadata _modelMetadata = modelMetadata;

        public void Apply(IQuerySet<TModel> queryset)
        {
            foreach (var navigation in _navigations)
            {
                if (_modelMetadata.Contains(navigation))
                    queryset.Query = queryset.Query.Include(navigation, _modelMetadata);
            }
        }
    }
}