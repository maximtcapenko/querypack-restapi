namespace QueryPack.RestApi.Internal
{
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using QueryPack.RestApi.Model.Meta;

    internal class ExpressionUtils
    {
        internal static string GetMemberPath(MemberExpression memberExpression)
        {
            var path = new List<string>();
            do
            {
                var memberName = memberExpression.Member.Name;
                path.Add(memberName);
                memberExpression = memberExpression.Expression as MemberExpression;
            }
            while (memberExpression is not null);

            var sb = new StringBuilder();
            var i = path.Count - 1;
            for (; i > 0; --i)
            {
                sb.Append(path[i]);
                sb.Append('.');
            }
            sb.Append(path[i]);
            return sb.ToString();
        }

        internal static Type GetMemberType(Expression candidateExpression)
             => (candidateExpression as MemberExpression).Member switch
             {
                 PropertyInfo p => p.PropertyType,
                 FieldInfo f => f.FieldType,
                 _ => throw new NotSupportedException()
             };

        internal static Action<TEntity, TProperty> CreateSetter<TEntity, TProperty>(Expression propertyExpression,
            Expression instanceExpression)
        {
            var param = Expression.Parameter(typeof(TProperty));
            var tree = GetSetterExpressionTree(propertyExpression as MemberExpression);

            tree.Add(Expression.Invoke(CreateSetterInternal<TEntity, TProperty>(propertyExpression, instanceExpression), instanceExpression, param));

            var block = Expression.Block(tree);

            return Expression.Lambda<Action<TEntity, TProperty>>(block, (ParameterExpression)instanceExpression, param).Compile();
        }

        internal static Func<TEntity, TProperty> CreateGetter<TEntity, TProperty>(Expression propertyExpression,
            Expression instanceExpression)
        {
            var tree = GetGetterExpressionTree(propertyExpression as MemberExpression);
            var block = Expression.Block(tree);

            return Expression.Lambda<Func<TEntity, TProperty>>(block, (ParameterExpression)instanceExpression).Compile();
        }

        internal static Expression BuildPredicateQueryByKeysExpression(ModelMetadata meta, object instance)
        {
            var keys = meta.GetKeys();
            Expression predicate = null;

            foreach (var key in keys)
            {
                var value = key.ValueGetter.GetValue(instance);
                if (value is not null)
                {
                    var expression = Expression.Equal(key.PropertyExpression, Expression.Constant(value));
                    if (predicate is null)
                        predicate = expression;
                    else
                        predicate = Expression.And(predicate, expression);
                }
            }

            return predicate;
        }
        private static Expression<Action<TEntity, TProperty>> CreateSetterInternal<TEntity, TProperty>(Expression propertyExpression,
            Expression instanceExpression)
        {
            var valueParam = Expression.Parameter(typeof(TProperty));
            var body = Expression.Assign(propertyExpression, valueParam);
            return Expression.Lambda<Action<TEntity, TProperty>>(body, (ParameterExpression)instanceExpression, valueParam);
        }

        private static List<Expression> GetGetterExpressionTree(MemberExpression memberExpression)
        {
            var type = (memberExpression.Member as PropertyInfo).PropertyType;
            var returnTarget = Expression.Label(type);
            var steps = new Stack<Expression>();

            steps.Push(Expression.Label(returnTarget, Expression.Default(type)));
            steps.Push(Expression.Return(returnTarget, memberExpression, type));

            do
            {
                var propertyInfo = memberExpression.Member as PropertyInfo;
                if (propertyInfo.PropertyType.IsClass && propertyInfo.PropertyType != typeof(string))
                {
                    var checkExpression = Expression.Equal(memberExpression, Expression.Constant(null));
                    var expression = Expression.IfThen(checkExpression, Expression.Return(returnTarget, Expression.Default(type), type));
                    steps.Push(expression);
                }

                memberExpression = memberExpression.Expression as MemberExpression;
            }
            while (memberExpression is not null);

            return steps.ToList();
        }

        private static List<Expression> GetSetterExpressionTree(MemberExpression memberExpression)
        {
            var tree = new Stack<Expression>();
            do
            {
                var propertyInfo = memberExpression.Member as PropertyInfo;
                if (propertyInfo.PropertyType.IsClass && propertyInfo.PropertyType != typeof(string))
                {
                    var checkExpression = Expression.Equal(memberExpression, Expression.Constant(null));
                    var expression = Expression.IfThen(checkExpression, Expression.Assign(memberExpression, Expression.MemberInit(Expression.New(propertyInfo.PropertyType))));
                    tree.Push(expression);
                }

                memberExpression = memberExpression.Expression as MemberExpression;
            }
            while (memberExpression is not null);

            return tree.ToList();
        }
    }
}