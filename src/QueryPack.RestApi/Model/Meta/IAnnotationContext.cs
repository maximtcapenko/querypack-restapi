namespace QueryPack.RestApi.Model.Meta
{
    using System.Linq.Expressions;

    public interface IAnnotationContext
    {
        ModelMetadata ModelMetadata { get; }
        IModelMetadataProvider ModelMetadataProvider { get; }
        MemberExpression PropertyExpression { get; set; }
        Type PropertyType { get; set; }
        object Input { get; }
        void SetResult(Expression annotationExpression);
    }
}