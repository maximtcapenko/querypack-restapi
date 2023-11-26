namespace QueryPack.RestApi.Mvc.Model.Impl
{
    using System.Linq.Expressions;
    using Intrnal;

    internal class RuntimeCriteriaBinderFactory<TModel> : ICriteriaBinderFactory<TModel>
        where TModel : class
    {
        private readonly Type _binderType;
        private readonly Func<ICriteriaBinder<TModel>> _factory;

        public RuntimeCriteriaBinderFactory(Type binderType)
        {
            _binderType = binderType;
            _factory = BuildFactoryInternal();
        }

        public ICriteriaBinder<TModel> Create()
            => _factory?.Invoke();

        public bool CanCreate(Type type) => typeof(TModel) == type && _factory != null;

        private Func<ICriteriaBinder<TModel>> BuildFactoryInternal()
        {
            Type binderType = null;

            if (_binderType.IsGenericType)
                binderType = _binderType.MakeGenericType(typeof(TModel));
            else
            {
                if (_binderType.GetInterfaces().Contains(typeof(ICriteriaBinder<TModel>)))
                    binderType = _binderType;
            }
            if (binderType != null)
            {
                var expression = Expression.Lambda<Func<ICriteriaBinder<TModel>>>(Expression.New(binderType));
                return expression.Compile();
            }

            return null;
        }
    }
}