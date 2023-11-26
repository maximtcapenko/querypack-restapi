namespace QueryPack.RestApi.Model.Meta.Impl
{
    using System.Linq.Expressions;

    internal class QueryAnnotationContext : IAnnotationContext
    {
        private readonly List<Expression> _annotationExpressions = new();

        public ModelMetadata ModelMetadata { get; }
        public IModelMetadataProvider ModelMetadataProvider { get; }
        public MemberExpression PropertyExpression { get; set; }
        public Type PropertyType { get; set; }
        public object Input { get; }
        
        public static QueryAnnotationContext Create(PropertyMetadata propertyMetadata, object input)
             => new(propertyMetadata.ModelMetadata, propertyMetadata.GetModelMetadataProvider(),
             propertyMetadata.PropertyExpression as MemberExpression, propertyMetadata.PropertyType, input);

        public QueryAnnotationContext(ModelMetadata modelMetadata, 
        IModelMetadataProvider modelMetadataProvider,
        MemberExpression propertyExpression,
        Type propertyType,
         object input)
        {
            ModelMetadata = modelMetadata;
            ModelMetadataProvider = modelMetadataProvider;
            PropertyExpression = propertyExpression;
            PropertyType = propertyType;
            Input = input;
        }

        public void SetResult(Expression annotationExpression)
            => _annotationExpressions.Add(annotationExpression);
        
        public IEnumerable<Expression> GetAnnotationExpressions() => _annotationExpressions;
    }
}