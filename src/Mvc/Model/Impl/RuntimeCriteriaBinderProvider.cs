namespace QueryPack.RestApi.Mvc.Model.Impl
{
    using System.Collections.Concurrent;
    using Intrnal;

    internal class RuntimeCriteriaBinderProvider : ICriteriaBinderProvider
    {
        private readonly IEnumerable<Type> _binders;

        public RuntimeCriteriaBinderProvider(params Type[] binders)
        {
            _binders = binders;
        }

        private static ConcurrentDictionary<Type, IEnumerable<ICriteriaBinderFactory>> _binderFactoryCache 
            = new ConcurrentDictionary<Type, IEnumerable<ICriteriaBinderFactory>>();

        public IEnumerable<ICriteriaBinder<TModel>> GetBinders<TModel>() where TModel : class
        {
            var factories = _binderFactoryCache.GetOrAdd(typeof(TModel), (type) 
                => _binders.Select(e => new RuntimeCriteriaBinderFactory<TModel>(e)));

            return factories.Where(e => e.CanCreate(typeof(TModel)))
                            .OfType<ICriteriaBinderFactory<TModel>>()
                            .Select(e => e.Create());
        }
    }
}