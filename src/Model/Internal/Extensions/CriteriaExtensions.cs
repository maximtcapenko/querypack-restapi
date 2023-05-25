namespace QueryPack.RestApi.Model.Internal.Extensions
{
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.EntityFrameworkCore;
    using Meta;
    using System.Collections.Concurrent;

    internal static class CriteriaExtensions
    {
        static MethodInfo IncludeMethod = typeof(CriteriaExtensions).GetMethod(nameof(Include),
         BindingFlags.Static | BindingFlags.NonPublic);

        static MethodInfo SetOrderMethod = typeof(CriteriaExtensions).GetMethod(nameof(SetOrder),
         BindingFlags.Static | BindingFlags.NonPublic);

        static ConcurrentDictionary<string, Delegate> _internalMethodsCache
            = new ConcurrentDictionary<string, Delegate>();

        public static IQueryable<TModel> Include<TModel>(this IQueryable<TModel> self,
            PropertyMetadata candidate, ModelMetadata modelMetadata)
            where TModel : class
        {
            var key = $"{IncludeMethod.Name}Of{typeof(TModel).FullName}{candidate.PropertyName}";
            var cachedMethod = _internalMethodsCache.GetOrAdd(key, (key) =>
           {
               var genericInclude = IncludeMethod.MakeGenericMethod(typeof(TModel), candidate.PropertyType);
               return MethodFactory.CreateGenericMethod<IQueryable<TModel>>(genericInclude);
           });

            return ((Func<object, object[], IQueryable<TModel>>)cachedMethod)(null, new object[] { self, candidate.PropertyExpression, modelMetadata.InstanceExpression });
        }

        public static IQueryable<TModel> OrderBy<TModel>(this IQueryable<TModel> self,
            PropertyMetadata candidate, ModelMetadata modelMetadata, OrderDirection direction)
            where TModel : class
        {
            var key = $"{SetOrderMethod.Name}Of{typeof(TModel).FullName}{candidate.PropertyName}";
            var cachedMethod = _internalMethodsCache.GetOrAdd(key, (key) =>
            {
                var genericSetOrder = SetOrderMethod.MakeGenericMethod(typeof(TModel), candidate.PropertyType);
                return MethodFactory.CreateGenericMethod<IQueryable<TModel>>(genericSetOrder);
            });

            return ((Func<object, object[], IQueryable<TModel>>)cachedMethod)(null, new object[] { self, candidate.PropertyExpression, modelMetadata.InstanceExpression, direction });
        }

        private static IQueryable<TModel> Include<TModel, TNavigation>(IQueryable<TModel> query,
            Expression candidateExpression, ParameterExpression instanceExpression)
            where TModel : class
            where TNavigation : class
        {
            var navigation = Expression.Lambda<Func<TModel, TNavigation>>(candidateExpression, instanceExpression);
            return query.Include(navigation);
        }

        private static IQueryable<TModel> SetOrder<TModel, TProperty>(IQueryable<TModel> query, Expression candidateExpression,
            ParameterExpression instanceExpression, OrderDirection direction)
            where TModel : class
        {
            var property = Expression.Lambda<Func<TModel, TProperty>>(candidateExpression, instanceExpression);
            if (query.Expression.Type == typeof(IOrderedQueryable<TModel>))
            {
                if (direction == OrderDirection.Desc)
                    return (query as IOrderedQueryable<TModel>).ThenByDescending(property);
                else
                    return (query as IOrderedQueryable<TModel>).ThenBy(property);
            }
            else
            {
                if (direction == OrderDirection.Desc)
                    return query.OrderByDescending(property);
                else
                    return query.OrderBy(property);
            }
        }
    }
}