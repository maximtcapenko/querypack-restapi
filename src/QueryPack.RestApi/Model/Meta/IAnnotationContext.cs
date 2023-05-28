namespace QueryPack.RestApi.Model.Meta
{
    using System.Linq.Expressions;

    public interface IAnnotationContext
    {
        PropertyMetadata PropertyMetadata { get; }
        object Input { get; }
        void SetResult(Expression annotationExpression);
    }
}