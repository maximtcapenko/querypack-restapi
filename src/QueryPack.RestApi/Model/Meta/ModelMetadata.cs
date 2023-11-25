namespace QueryPack.RestApi.Model.Meta
{
    using System.Linq.Expressions;

    public sealed class ModelMetadata
    {
        public Type ModelType { get; }

        public IEnumerable<PropertyMetadata> PropertyMetadata { get; }

        public Expression InstanceExpression { get; }

        public Func<object> InstanceFactory { get; }

        internal ModelMetadata(Type modelType, IModelMetadataProvider metadataProvider)
        {
            ModelType = modelType;
            var propertyMeta = new List<PropertyMetadata>();

            PropertyMetadata = propertyMeta;
            InstanceExpression = Expression.Parameter(modelType);
            InstanceFactory = Expression.Lambda<Func<object>>(Expression.New(modelType)).Compile();

            foreach (var property in modelType.GetProperties())
            {
                propertyMeta.Add(new PropertyMetadata(property, this, metadataProvider));
            }
        }
    }
}