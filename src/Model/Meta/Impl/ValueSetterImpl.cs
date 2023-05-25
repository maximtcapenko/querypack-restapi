namespace QueryPack.RestApi.Model.Meta.Impl
{
    internal class ValueSetterImpl<TModel, TValue> : IValueSetter
        where TModel : class
    {
        private readonly Action<TModel, TValue> _setter;

        public ValueSetterImpl(Action<TModel, TValue> setter)
        {
            _setter = setter;
        }

        public void SetValue(object model, object value)
        {
            _setter((TModel)model, (TValue)value);
        }
    }
}