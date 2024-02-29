namespace QueryPack.RestApi.Internal
{
    using System.Collections.Concurrent;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.EntityFrameworkCore;
    using QueryPack.RestApi.Model.Meta;

    internal static class QueryUtils
    {
        private static readonly ConcurrentDictionary<Type, Delegate> _internalMethodsCache = new();

        internal static MethodInfo LoadAsyncMethod => typeof(QueryUtils).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                           .FirstOrDefault(e => e.Name == nameof(LoadAsync));
        internal static async Task<object> LoadAsync<T>(DbContext context, object instance, ModelMetadata meta)
          where T : class
        {
            var expression = ExpressionUtils.BuildPredicateQueryByKeysExpression(meta, instance);

            if (expression is null) return null;

            var where = Expression.Lambda<Func<T, bool>>(expression, meta.InstanceExpression as ParameterExpression);
            return await context.Set<T>().FirstOrDefaultAsync(where);
        }

        internal static Func<DbContext, object, ModelMetadata, Task<object>> GetEntityLoader(Type modelType)
        {
            var method =(Func<object, object[], Task<object>>)_internalMethodsCache.GetOrAdd(modelType,
              type => MethodFactory.CreateGenericMethod<Task<object>>(LoadAsyncMethod.MakeGenericMethod(modelType)));
            
            return (context, instance, meta) => method(context, new[] { context, instance, meta });
        }
    }
}