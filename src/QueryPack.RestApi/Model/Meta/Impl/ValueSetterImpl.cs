using System.Collections;
using System.Reflection;
using QueryPack.RestApi.Internal;

namespace QueryPack.RestApi.Model.Meta.Impl
{
    internal class ValueSetterImpl<TModel, TValue> : IValueSetter
        where TModel : class
    {
        private readonly Action<TModel, TValue> _setter;
        private readonly Func<object, object[], TValue> _collectionSetter;

        public ValueSetterImpl(Action<TModel, TValue> setter)
        {
            _setter = setter;
            if (IsEnumearable())
                _collectionSetter = ResolveCollectionSetter();
        }

        public void SetValue(object model, object value)
        {
            if (_collectionSetter is not null)
            {
                _setter((TModel)model, _collectionSetter(null, new[] { value }));
            }
            else
                _setter((TModel)model, (TValue)value);
        }

        private static bool IsEnumearable()
            => typeof(TValue).GetInterfaces()
                             .Any(e => e.IsGenericType && e.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                                        && e.GetGenericArguments().All(i => i.IsClass));

        private static List<T> Convert<T>(IEnumerable source) => source.Cast<T>().ToList();

        private static Func<object, object[], TValue> ResolveCollectionSetter()
        {
            var itemType = typeof(TValue).GetGenericArguments().First();
            var methodinfo = typeof(ValueSetterImpl<TModel, TValue>).GetMethod(nameof(Convert), BindingFlags.Static | BindingFlags.NonPublic);
            return MethodFactory.CreateGenericMethod<TValue>(methodinfo.MakeGenericMethod(itemType));
        }
    }
}