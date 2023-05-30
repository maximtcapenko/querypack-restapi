namespace QueryPack.RestApi.Model.Annotations
{
    using Meta;
    using System.Linq.Expressions;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Property)]
    public class TextSearchAttribute : Attribute, IAnnotation
    {
        private static readonly MethodInfo _likeMethod = typeof(String).GetMethod(nameof(String.StartsWith),
             new[] {typeof(string)});

        public void Apply(IAnnotationContext context)
        {
            // check if property type is string
            if(context.PropertyType != typeof(string) && context.Input.GetType() != typeof(string))
                return;

            var likeCallExpression = Expression.Call(context.PropertyExpression, _likeMethod, Expression.Constant(context.Input));
            context.SetResult(likeCallExpression);
        }
    }
}