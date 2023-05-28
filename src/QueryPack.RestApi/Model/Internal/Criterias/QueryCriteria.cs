namespace QueryPack.RestApi.Model.Internal.Criterias
{
    using System.Linq.Expressions;
    using System.Reflection;
    using Meta;
    using RestApi.Internal;

    internal class QueryCriteria<TModel> : ICriteria<TModel>
        where TModel : class
    {
        private static MethodInfo _containsMethod = ReflectionUtils.GetContainsMethod();
        private static MethodInfo _selectMethod = ReflectionUtils.GetSelectMethod();

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
                    var propertyExpression = selector.Key.PropertyExpression;
                    var queryAnnotations = selector.Key.Annotations;

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
                        Expression defaultPredicateExpression = Expression.Equal(propertyExpression, Expression.Constant(value));
                        
                        if (queryAnnotations.Any())
                        {
                            var annotationContext = new QueryAnnotationContext(selector.Key, value);
                            foreach (var annotation in queryAnnotations)
                                annotation.Apply(annotationContext);

                            Expression resultAnnotationExpression = null;
                            foreach (var annotationExpression in annotationContext.GetAnnotationExpressions())
                            {
                                if (resultAnnotationExpression == null)
                                    resultAnnotationExpression = annotationExpression;
                                else
                                    resultAnnotationExpression = Expression.And(resultAnnotationExpression, annotationExpression);
                            }

                            if(resultAnnotationExpression == null)
                                resultAnnotationExpression = defaultPredicateExpression;

                            predicate = Expression.Lambda<Func<TModel, bool>>(resultAnnotationExpression, (ParameterExpression)_modelMetadata.InstanceExpression);
                        }
                        else
                            predicate = Expression.Lambda<Func<TModel, bool>>(defaultPredicateExpression, (ParameterExpression)_modelMetadata.InstanceExpression);
                    }

                    query.Query = query.Query.Where(predicate);
                }
            }
        }

        private Expression<Func<TModel, bool>> BuildContains(PropertyMetadata meta, object[] values)
        {
            var property = meta.PropertyExpression;
            // Make generic
            var contains = _containsMethod.MakeGenericMethod(meta.PropertyType);
            var select = _selectMethod.MakeGenericMethod(typeof(object), meta.PropertyType);

            var objectParameter = Expression.Parameter(typeof(object));

            var selectCall = Expression.Call(null, select, Expression.Constant(values),
            Expression.Lambda(Expression.Convert(objectParameter, (meta.PropertyType)), objectParameter));

            return Expression.Lambda<Func<TModel, bool>>(Expression.Call(null, contains, selectCall, property),
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

        class QueryAnnotationContext : IAnnotationContext
        {
            private List<Expression> _annotationExpressions = new List<Expression>();

            public PropertyMetadata PropertyMetadata { get; }

            public object Input { get; }

            public QueryAnnotationContext(PropertyMetadata propertyMetadata, object input)
            {
                PropertyMetadata = propertyMetadata;
                Input = input;
            }

            public void SetResult(Expression annotationExpression)
            {
                _annotationExpressions.Add(annotationExpression);
            }

            public IEnumerable<Expression> GetAnnotationExpressions() => _annotationExpressions;
        }
    }
}