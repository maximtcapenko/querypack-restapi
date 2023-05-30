namespace QueryPack.RestApi.Model.Meta
{
    using System.Linq.Expressions;
    using System.Reflection;

    public static class MetadataExtnsions
    {
        public static PropertyMetadata GetProperty(this ModelMetadata self, MemberInfo member)
           => self.PropertyMetadata.FirstOrDefault(e => (e.PropertyExpression as MemberExpression).Member.Equals(member));

        public static IEnumerable<PropertyMetadata> GetNavigations(this ModelMetadata self)
            => self.PropertyMetadata.Where(e => e.IsNavigation && !e.IsIgnored);

        public static IEnumerable<PropertyMetadata> GetKeys(this ModelMetadata self)
             => self.PropertyMetadata.Where(e => e.IsKey);

        public static IEnumerable<PropertyMetadata> GetRegularProperties(this ModelMetadata self)
             => self.PropertyMetadata.Where(e => e.IsPrimitive);

        public static bool Contains(this ModelMetadata self, PropertyMetadata propertyMetadata)
            => self.PropertyMetadata.Contains(propertyMetadata);
    }
}