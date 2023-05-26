namespace QueryPack.RestApi.Model.Internal.Criterias
{
    internal class RootCriteria<TModel> : ICriteria<TModel>
        where TModel : class
    {
        private readonly IEnumerable<ICriteria<TModel>> _internalCriterias;

        public RootCriteria(params ICriteria<TModel>[] criterias)
        {
            if (criterias == null || criterias.Count() == 0)
                _internalCriterias = new List<ICriteria<TModel>>();
            else
                _internalCriterias = criterias;
        }

        public void Apply(IQueryContainer<TModel> query)
        {
            foreach (var criteria in _internalCriterias)
                criteria.Apply(query);
        }
    }
}