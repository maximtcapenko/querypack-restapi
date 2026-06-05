namespace QueryPack.RestApi.Model.Meta.Impl;

using System.Linq.Expressions;

internal class QueryAnnotationContext(ModelMetadata modelMetadata,
IModelMetadataProvider modelMetadataProvider,
MemberExpression propertyExpression,
Type propertyType,
 object input) : IAnnotationContext
{
    private readonly List<Expression> _annotationExpressions = [];

    public ModelMetadata ModelMetadata { get; } = modelMetadata;
    public IModelMetadataProvider ModelMetadataProvider { get; } = modelMetadataProvider;
    public MemberExpression PropertyExpression { get; set; } = propertyExpression;
    public Type PropertyType { get; set; } = propertyType;
    public object Input { get; } = input;

    public static QueryAnnotationContext Create(PropertyMetadata propertyMetadata, object input)
         => new(propertyMetadata.ModelMetadata, propertyMetadata.GetModelMetadataProvider(),
         propertyMetadata.PropertyExpression as MemberExpression, propertyMetadata.PropertyType, input);

    public void SetResult(Expression annotationExpression)
        => _annotationExpressions.Add(annotationExpression);
    
    public IEnumerable<Expression> GetAnnotationExpressions() => _annotationExpressions;
}