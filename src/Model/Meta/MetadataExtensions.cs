namespace QueryPack.RestApi.Model.Meta
{
    public static class MetadataExtnsions
    {
        public static IEnumerable<PropertyMetadata> GetNavigations(this ModelMetadata self)
            => self.PropertyMetadata.Where(e => e.IsNavigation && !e.IsIgnored);

        public static IEnumerable<PropertyMetadata> GetKeys(this ModelMetadata self)
             => self.PropertyMetadata.Where(e => e.IsKey);

        public static IEnumerable<PropertyMetadata> GetRegularProperties(this ModelMetadata self)
             => self.PropertyMetadata.Where(e => e.IsPrimitive);
    }
}