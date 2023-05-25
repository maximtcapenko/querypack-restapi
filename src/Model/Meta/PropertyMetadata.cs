namespace QueryPack.RestApi.Model.Meta
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq.Expressions;
    using System.Reflection;
    using Impl;
    using RestApi.Internal;

    public sealed class PropertyMetadata
    {
        record class Accessors(IValueGetter ValueGetter)
        {
            public IValueSetter ValueSetter { get; set; }
        };

        private readonly PropertyInfo _propertyInfo;
        private readonly IModelMetadataProvider _metadataProvider;

        public ModelMetadata ModelMetadata { get; }
        public Type PropertyType => _propertyInfo.PropertyType;
        public string PropertyName => _propertyInfo.Name;
        public Expression PropertyExpression { get; }
        public bool IsKey { get; }
        public bool IsPrimitive { get; }
        public bool IsNumber { get; }
        public bool IsDate { get; }
        public bool IsNavigation => _metadataProvider.GetMetadata(PropertyType) != null;
        public bool IsReadOnly => _propertyInfo.CanWrite;
        public bool IsIgnored { get; }
        public IValueGetter ValueGetter { get; }
        public IValueSetter ValueSetter { get; }

        public PropertyMetadata(PropertyInfo propertyInfo, ModelMetadata modelMetadata,
            IModelMetadataProvider metadataProvider)
        {
            ModelMetadata = modelMetadata;
            _propertyInfo = propertyInfo;
            _metadataProvider = metadataProvider;
            PropertyExpression = Expression.PropertyOrField(modelMetadata.InstanceExpression, propertyInfo.Name);
            IsKey = ResolveIsKey(propertyInfo, modelMetadata);
            IsPrimitive = ResolveIsPrimitive(propertyInfo);
            IsIgnored = ResolveIsIgnored(propertyInfo);
            IsDate = ResolveIsDate(propertyInfo);
            var buildGetter = GetType().GetMethod(nameof(BuildAccessors), BindingFlags.Static | BindingFlags.NonPublic);
            var accessors = (Accessors)buildGetter.MakeGenericMethod(modelMetadata.ModelType, PropertyType)
                .Invoke(null, new object[] { propertyInfo, PropertyExpression, modelMetadata.InstanceExpression });

            ValueGetter = accessors.ValueGetter;
            ValueSetter = accessors.ValueSetter;
        }


        private bool ResolveIsPrimitive(PropertyInfo property)
        {
            bool isPrimitive(Type type) => type.IsPrimitive
                || type == typeof(string)
                || type.IsEnum
                || type == typeof(DateTime)
                || type == typeof(DateTimeOffset)
                || type == typeof(TimeSpan);

            return isPrimitive(property.PropertyType)
                || (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)
                && isPrimitive(property.PropertyType.GetGenericArguments()[0]));
        }

        private bool ResolveIsKey(PropertyInfo property, ModelMetadata modelMetadata)
        {
            if (property.GetCustomAttribute<KeyAttribute>() != null)
            {
                return true;
            }
            if (property.Name.Equals($"{modelMetadata.ModelType.Name}{property.Name}",
                StringComparison.OrdinalIgnoreCase) ||
                property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private bool ResolveIsIgnored(PropertyInfo property)
            => property.GetCustomAttribute<NotMappedAttribute>() != null;

        private bool ResolveIsDate(PropertyInfo property)
        {
            bool isDate(Type type) => type == typeof(DateTime)
              || type == typeof(DateTimeOffset)
              || type == typeof(TimeSpan);

            return isDate(property.PropertyType)
               || (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)
               && isDate(property.PropertyType.GetGenericArguments()[0]));
        }

        private static Accessors BuildAccessors<TModel, TProperty>(PropertyInfo propertyInfo, Expression propertyExpression, Expression instanceExpression)
            where TModel : class
        {
            var getter = ExpressionUtils.CreateGetter<TModel, TProperty>(propertyExpression, instanceExpression);
            var accessors = new Accessors(new ValueGetterImpl<TModel, TProperty>(getter));

            if (propertyInfo.CanWrite)
            {
                var setter = ExpressionUtils.CreateSetter<TModel, TProperty>(propertyExpression, instanceExpression);
                accessors.ValueSetter = new ValueSetterImpl<TModel, TProperty>(setter);
            }

            return accessors;
        }
    }
}