namespace QueryPack.RestApi.Mvc.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using RestApi.Model.Meta;

    internal class ModelKeysOnlyJsonConverterFactory : JsonConverterFactory
    {
        private readonly static ConcurrentDictionary<Type, Func<ModelMetadata, JsonConverter>> _factoryCache = new();

        private readonly ModelMetadata _modelMetadata;

        public ModelKeysOnlyJsonConverterFactory(ModelMetadata modelMetadata)
        {
            _modelMetadata = modelMetadata;
        }

        public override bool CanConvert(Type typeToConvert)
            => typeToConvert == _modelMetadata.ModelType;

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var factory = _factoryCache.GetOrAdd(typeToConvert, (type) =>
            {
                var argType = typeof(ModelMetadata);
                var convertorType = typeof(ModelKeysJsonConverter<>).MakeGenericType(type);
                var parameter = Expression.Parameter(argType);
                var ctor = convertorType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, new[] { argType });
                var @new = Expression.New(ctor, parameter);
                return Expression.Lambda<Func<ModelMetadata, JsonConverter>>(@new, parameter).Compile();
            });

            return factory(_modelMetadata);
        }

        class ModelKeysJsonConverter<TModel> : JsonConverter<TModel>
            where TModel : class
        {
            private readonly ModelMetadata _modelMetadata;

            public ModelKeysJsonConverter(ModelMetadata modelMetadata)
            {
                _modelMetadata = modelMetadata;
            }

            public override TModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }

            public override void Write(Utf8JsonWriter writer, TModel value, JsonSerializerOptions options)
            {
                var keys = _modelMetadata.GetKeys();
                writer.WriteStartObject();
                foreach (var key in keys)
                {
                    writer.WritePropertyName(options.PropertyNamingPolicy.ConvertName(key.PropertyName));
                    var propertyValue = key.ValueGetter.GetValue(value);
                    writer.WriteRawValue(JsonSerializer.Serialize(propertyValue, options));
                }
                writer.WriteEndObject();
            }
        }
    }
}