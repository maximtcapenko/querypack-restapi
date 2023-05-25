namespace QueryPack.RestApi.Mvc.Internal
{
#if NET7_0_OR_GREATER
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization.Metadata;
    using Rest.Model.Meta;


    internal class ModelKeysOnlyJsonTypeInfoResolver : DefaultJsonTypeInfoResolver
    {
        private readonly Rest.Model.Meta.ModelMetadata _modelMetadata;

        public ModelKeysOnlyJsonTypeInfoResolver(Rest.Model.Meta.ModelMetadata modelMetadata)
        {
            _modelMetadata = modelMetadata;
        }

        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            var info = base.GetTypeInfo(type, options);
            if (type == _modelMetadata.ModelType)
            {
                var keys = _modelMetadata.GetKeys();

                foreach (var property in info.Properties)
                {
                    if (keys.Any(e => options.PropertyNamingPolicy.ConvertName(e.PropertyName) == property.Name))
                        property.ShouldSerialize = (o, e) => true;
                    else
                        property.ShouldSerialize = (o, e) => false;
                }

            }
            return info;
        }
    }
#endif
}