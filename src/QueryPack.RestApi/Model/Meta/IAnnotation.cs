namespace QueryPack.RestApi.Model.Meta
{
    public interface IAnnotation
    {
        void Apply(IAnnotationContext context);
    }
}