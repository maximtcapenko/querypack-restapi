namespace QueryPack.RestApi.Model.Annotations
{
    using Meta;
    using System.Linq.Expressions;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Property)]
    public class TextSearchAttribute : Attribute, IAnnotation
    {
        private static readonly MethodInfo LikeMethodInfo = typeof(String).GetMethod(nameof(String.StartsWith),
             new[] {typeof(string)});

        public void Apply(IAnnotationContext context)
        {
            // check if property type is string
            if(context.PropertyMetadata.PropertyType != typeof(string))
                return;

            var likeCallExpression = Expression.Call(context.PropertyMetadata.PropertyExpression, LikeMethodInfo, Expression.Constant(context.Input));
            context.SetResult(likeCallExpression);
        }
    }
}