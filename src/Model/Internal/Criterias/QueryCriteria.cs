namespace QueryPack.RestApi.Model.Internal.Criterias
{
    using System.Linq.Expressions;
    using Meta;

    internal class QueryCriteria<TModel> : ICriteria<TModel>
        where TModel : class
    {
        private readonly Dictionary<PropertyMetadata, object[]> _predicateSelectos;
        private readonly ModelMetadata _modelMetadata;

        public QueryCriteria(ModelMetadata modelMetadata, Dictionary<PropertyMetadata, object[]> predicateSelectors)
        {
            _predicateSelectos = predicateSelectors;
            _modelMetadata = modelMetadata;
        }

        public void Apply(IQueryContainer<TModel> query)
        {
            foreach (var selector in _predicateSelectos)
            {
                if (_modelMetadata.Contains(selector.Key))
                {
                    Expression<Func<TModel, bool>> predicate = null;
                    var property = selector.Key.PropertyExpression;
                    if (selector.Value.Count() > 1)
                    {
                        if (selector.Key.IsDate)
                            predicate = BuildDateBetween(selector.Key, selector.Value);
                        else
                            predicate = BuildContains(selector.Key, selector.Value);
                    }
                    else
                    {
                        var value = selector.Value[0];
                        predicate = Expression.Lambda<Func<TModel, bool>>(Expression.Equal(property, Expression.Constant(value)), (ParameterExpression)_modelMetadata.InstanceExpression);
                    }

                    query.Query = query.Query.Where(predicate);
                }
            }
        }

        private Expression<Func<TModel, bool>> BuildContains(PropertyMetadata meta, object[] values)
        {
            var property = meta.PropertyExpression;

            // Get helper methods
            var contains = typeof(Enumerable).GetMethods().FirstOrDefault(e => e.Name == nameof(Enumerable.Contains)
               && e.IsStatic && e.GetParameters().Count() == 2);
            var select = typeof(Enumerable).GetMethods().FirstOrDefault(e => e.Name == nameof(Enumerable.Select)
                && e.GetParameters().Count() == 2);
            // Make generic
            var containsGeneric = contains.MakeGenericMethod(meta.PropertyType);
            var selectGeneric = select.MakeGenericMethod(typeof(object), meta.PropertyType);

            var objectParameter = Expression.Parameter(typeof(object));

            var selectCall = Expression.Call(null, selectGeneric, Expression.Constant(values),
            Expression.Lambda(Expression.Convert(objectParameter, (meta.PropertyType)), objectParameter));

            return Expression.Lambda<Func<TModel, bool>>(Expression.Call(null, containsGeneric, selectCall, property),
                (ParameterExpression)meta.ModelMetadata.InstanceExpression);
        }

        private Expression<Func<TModel, bool>> BuildDateBetween(PropertyMetadata meta, object[] values)
        {
            var property = meta.PropertyExpression;
            // take the  minimal and maximum dates
            var start = values.Min();
            var end = values.Max();

            var greater = Expression.GreaterThan(meta.PropertyExpression, Expression.Constant(start));
            if (start != end)
            {
                var less = Expression.LessThanOrEqual(meta.PropertyExpression, Expression.Constant(end));
                return Expression.Lambda<Func<TModel, bool>>(Expression.And(less, greater),
                    (ParameterExpression)meta.ModelMetadata.InstanceExpression);

            }
            else
            {
                return Expression.Lambda<Func<TModel, bool>>(greater, (ParameterExpression)meta.ModelMetadata.InstanceExpression);
            }
        }
    }
}