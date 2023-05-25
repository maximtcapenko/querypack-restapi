namespace QueryPack.RestApi.Model.Meta.Impl
{
    internal class ValueGetterImpl<TModel, TValue> : IValueGetter
        where TModel : class
    {
        private readonly Func<TModel, TValue> _getter;

        public ValueGetterImpl(Func<TModel, TValue> getter)
        {
            _getter = getter;
        }

        public object GetValue(object model) => _getter((TModel)model);
    }
}