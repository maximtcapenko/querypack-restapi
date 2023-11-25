namespace QueryPack.RestApi.Model.Internal.Criterias
{
    using System.Linq.Expressions;
    using System.Reflection;
    using Meta;
    using Meta.Impl;
    using RestApi.Internal;

    internal class QueryCriteria<TModel> : ICriteria<TModel>
        where TModel : class
    {
        private static MethodInfo _containsMethod = ReflectionUtils.GetContainsMethod();
        private static MethodInfo _selectMethod = ReflectionUtils.GetSelectMethod();

        private readonly Dictionary<PropertyMetadata, IEnumerable<object>> _predicateSelectos;
        private readonly ModelMetadata _modelMetadata;

        public QueryCriteria(ModelMetadata modelMetadata, Dictionary<PropertyMetadata, IEnumerable<object>> predicateSelectors)
        {
            _predicateSelectos = predicateSelectors;
            _modelMetadata = modelMetadata;
        }

        public void Apply(IQueryContainer<TModel> query)
        {
            foreach (var selector in _predicateSelectos)
            {
                Expression resultExpressionPredicate = null;

                if (_modelMetadata.Contains(selector.Key))
                {
                    var propertyExpression = selector.Key.PropertyExpression;
                    var queryAnnotations = selector.Key.Annotations;

                    if (queryAnnotations.Any())
                    {
                        var annotationContext = QueryAnnotationContext.Create(selector.Key, selector.Value);
                        var resultAnnotationExpression = ProcessAnnotations(selector.Key, annotationContext);

                        resultExpressionPredicate = resultAnnotationExpression;
                    }
                    else if (selector.Key.IsNavigation)
                    {
                        resultExpressionPredicate = BuildNavigationPredicate(selector.Key, selector.Value);
                    }
                    else
                    {
                        if (selector.Value.Count() > 1)
                        {
                            if (selector.Key.IsDate)
                                resultExpressionPredicate = BuildDateBetween(selector.Key.PropertyExpression, selector.Value);
                            else
                                resultExpressionPredicate = BuildContains(selector.Key.PropertyExpression, selector.Value);
                        }
                        else
                        {
                            var value = selector.Value.First();
                            resultExpressionPredicate = Expression.Equal(propertyExpression, Expression.Constant(value));
                        }
                    }
                }

                ArgumentNullException.ThrowIfNull(resultExpressionPredicate);

                var predicate = Expression.Lambda<Func<TModel, bool>>(resultExpressionPredicate, (ParameterExpression)_modelMetadata.InstanceExpression);
                query.Query = query.Query.Where(predicate);
            }
        }

        private static Expression BuildContains(Expression propertyExpression, IEnumerable<object> values)
        {
            // Make generic
            var contains = _containsMethod.MakeGenericMethod(ExpressionUtils.GetMemberType(propertyExpression));
            var select = _selectMethod.MakeGenericMethod(typeof(object), ExpressionUtils.GetMemberType(propertyExpression));

            var objectParameter = Expression.Parameter(typeof(object));

            var selectCall = Expression.Call(null, select, Expression.Constant(values),
            Expression.Lambda(Expression.Convert(objectParameter, ExpressionUtils.GetMemberType(propertyExpression)), objectParameter));

            return Expression.Call(null, contains, selectCall, propertyExpression);
        }

        private static Expression BuildDateBetween(Expression propertyExpression, IEnumerable<object> values)
        {
            // take the  minimal and maximum dates
            var start = values.Min();
            var end = values.Max();

            var greater = Expression.GreaterThan(propertyExpression, Expression.Constant(start));
            if (start != end)
            {
                var less = Expression.LessThanOrEqual(propertyExpression, Expression.Constant(end));
                return Expression.And(less, greater);
            }
            else
                return greater;
        }

        private static Expression ProcessAnnotations(PropertyMetadata propertyMetadata, QueryAnnotationContext annotationContext)
        {
            Expression resultAnnotationExpression = null;

            foreach (var annotation in propertyMetadata.Annotations)
                annotation.Apply(annotationContext);

            foreach (var annotationExpression in annotationContext.GetAnnotationExpressions())
            {
                resultAnnotationExpression = And(resultAnnotationExpression, annotationExpression);
            }

            if (resultAnnotationExpression is null)
                throw new NotImplementedException($"Property {propertyMetadata.PropertyName} is annotated but annotations are not implemented");

            return resultAnnotationExpression;
        }

        private static Expression BuildNavigationPredicate(PropertyMetadata meta, IEnumerable<object> inputObject)
        {
            var input = inputObject.OfType<Dictionary<string, IEnumerable<object>>>();
            Expression resultExpressionPredicate = null;

            foreach (var item in input.First())
            {
                var key = item.Key;

                var member = GetMemberExpressionFromPath(key,
                    (ParameterExpression)meta.ModelMetadata.InstanceExpression);

                var declaredType = member.Member.DeclaringType;
                var containerModelMetadata = meta.GetModelMetadataProvider().GetMetadata(declaredType);
                ArgumentNullException.ThrowIfNull(containerModelMetadata, $"Type {declaredType.FullName} is not registered in metadata container");

                var propertyMetadata = containerModelMetadata.GetProperty(member.Member);

                if (item.Value.Count() > 1)
                {
                    if (propertyMetadata.IsDate)
                        resultExpressionPredicate = BuildDateBetween(member, item.Value);
                    else
                        resultExpressionPredicate = BuildContains(member, item.Value);
                }
                else
                {
                    var value = item.Value.First();
                    Expression defaultResultExpression = Expression.Equal(member, Expression.Constant(value));

                    if (propertyMetadata.Annotations.Count() > 0)
                    {
                        var annotationContext = new QueryAnnotationContext(meta.ModelMetadata, meta.GetModelMetadataProvider(),
                         member, propertyMetadata.PropertyType, value);

                        var resultAnnotationExpression = ProcessAnnotations(propertyMetadata, annotationContext);
                        resultExpressionPredicate = And(resultExpressionPredicate, resultAnnotationExpression);
                    }
                    else
                    {
                        resultExpressionPredicate = And(resultExpressionPredicate, defaultResultExpression);
                    }
                }
            }

            return resultExpressionPredicate;
        }

        private static Expression And(Expression left, Expression right)
        {
            if (left is null)
                left = right;
            else
                left = Expression.And(left, right);

            return left;
        }

        private static MemberExpression GetMemberExpressionFromPath(string propertyPath, ParameterExpression instanceExpression)
        {
            if (string.IsNullOrEmpty(propertyPath))
                throw new ArgumentException("Invalid empty member path", nameof(propertyPath));

            MemberExpression memberExpression = null;
            var memberExpressions = new List<MemberExpression>();
            foreach (var member in propertyPath.Split('.'))
            {
                memberExpression = memberExpression == null
                    ? Expression.PropertyOrField(instanceExpression, member)
                    : Expression.PropertyOrField(memberExpression, member);

                memberExpressions.Add(memberExpression);
            }

            return memberExpressions.Last();
        }
    }
}