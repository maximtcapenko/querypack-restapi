namespace QueryPack.RestApi.Model.Meta
{
    public interface IModelMetadataProvider
    {
        ModelMetadata GetMetadata(Type modelType);
    }
}