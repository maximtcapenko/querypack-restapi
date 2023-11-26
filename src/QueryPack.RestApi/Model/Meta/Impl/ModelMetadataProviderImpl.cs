namespace QueryPack.RestApi.Model.Meta.Impl
{
    internal class ModelMetadataProviderImpl : IModelMetadataProvider
    {
        private Dictionary<Type, ModelMetadata> _metaCache = new();

        public ModelMetadataProviderImpl(IEnumerable<Type> modelTypes)
        {
            foreach(var modelType in modelTypes)
            {
                _metaCache[modelType] = new  ModelMetadata(modelType, this);
            }
        }

        public ModelMetadata GetMetadata(Type modelType)
        {
            if(_metaCache.TryGetValue(modelType, out var meta))
                return meta;
            
            return null;
        }
    }
}