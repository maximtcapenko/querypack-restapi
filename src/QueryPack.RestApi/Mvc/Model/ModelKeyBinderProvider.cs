namespace QueryPack.RestApi.Mvc.Model
{
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using Impl;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using RestApi.Model;

    internal class ModelKeyBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType.IsGenericType
                && context.Metadata.ModelType.GetGenericTypeDefinition() == typeof(ICriteria<>))
            {
                return new TypedModelBinder(context.Metadata.ModelType.GetGenericArguments().First());
            }

            return null;
        }

        class TypedModelBinder : IModelBinder
        {
            private static readonly ConcurrentDictionary<Type, IModelBinder> _internalBindersCache = new();

            private readonly MethodInfo _createMethod =
                 typeof(TypedModelBinder).GetMethod(nameof(Create), BindingFlags.NonPublic | BindingFlags.Static);
            private readonly Type _targetType;

            public TypedModelBinder(Type typeParam)
            {
                _targetType = typeParam;
            }

            public Task BindModelAsync(ModelBindingContext bindingContext)
            {
                var internalBinder = _internalBindersCache.GetOrAdd(_targetType, (type) =>
                {
                    var method = _createMethod.MakeGenericMethod(type);
                    return (IModelBinder)method.Invoke(null, null);
                });

                return internalBinder.BindModelAsync(bindingContext);
            }

            static IModelBinder Create<T>() where T : class
                 => new RuntimeModelBinder<T>();
        }
    }
}
