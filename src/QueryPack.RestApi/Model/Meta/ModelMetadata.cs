namespace QueryPack.RestApi.Model.Meta
{
    using System.Collections.Concurrent;
    using System.Linq.Expressions;

    public sealed class ModelMetadata
    {
        private static ConcurrentDictionary<Type, ModelMetadata> _metaCache
           = new ConcurrentDictionary<Type, ModelMetadata>();

        public Type ModelType { get; }

        public IEnumerable<PropertyMetadata> PropertyMetadata { get; }

        public Expression InstanceExpression { get; }

        internal ModelMetadata(Type modelType, IModelMetadataProvider metadataProvider)
        {
            ModelType = modelType;
            var propertyMeta = new List<PropertyMetadata>();

            PropertyMetadata = propertyMeta;
            InstanceExpression = Expression.Parameter(modelType);
            
            foreach (var property in modelType.GetProperties())
            {
                propertyMeta.Add(new PropertyMetadata(property, this, metadataProvider));
            }
        }
    }
}