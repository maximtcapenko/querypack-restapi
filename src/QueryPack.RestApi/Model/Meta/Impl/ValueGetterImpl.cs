namespace QueryPack.RestApi.Model.Meta.Impl;

internal class ValueGetterImpl<TModel, TValue>(Func<TModel, TValue> getter) : IValueGetter
    where TModel : class
{
    private readonly Func<TModel, TValue> _getter = getter;

    public object GetValue(object model) => _getter((TModel)model);
}