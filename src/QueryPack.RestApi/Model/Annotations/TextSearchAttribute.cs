namespace QueryPack.RestApi.Model.Annotations
{
    using Meta;
    using System.Linq.Expressions;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Property)]
    public class TextSearchAttribute : Attribute, IAnnotation
    {
        private static readonly MethodInfo _likeMethod = typeof(String).GetMethod(nameof(String.StartsWith),
             new[] { typeof(string) });

        public void Apply(IAnnotationContext context)
        {
            // check if property type is string
            if (context.PropertyType != typeof(string))
                return;

            if (context.Input is IEnumerable<object> inputs)
            {
                // build OR statement
                Expression resultExpression = null;
                foreach (var input in inputs)
                {
                    if(input is not string) continue;

                    if (resultExpression == null)
                        resultExpression = Expression.Call(context.PropertyExpression, _likeMethod, Expression.Constant(input));
                    else
                        resultExpression = Expression.Or(resultExpression, Expression.Call(context.PropertyExpression, _likeMethod, Expression.Constant(input)));
                }

                if (resultExpression != null)
                    context.SetResult(resultExpression);
            }
            else
            {
                if(context.Input is not string) return;

                var likeCallExpression = Expression.Call(context.PropertyExpression, _likeMethod, Expression.Constant(context.Input));
                context.SetResult(likeCallExpression);
            }
        }
    }
}