namespace QueryPack.RestApi.Swagger
{
    using Microsoft.OpenApi.Models;
    using Model.Meta;
    using Swashbuckle.AspNetCore.SwaggerGen;

    internal class RestModelOperationFilter : IOperationFilter
    {
        private readonly IModelMetadataProvider _modelMetadataProvider;

        public RestModelOperationFilter(IModelMetadataProvider modelMetadataProvider)
        {
            _modelMetadataProvider = modelMetadataProvider;
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var criteriaParameter = context.MethodInfo.GetParameters().FirstOrDefault(p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof(RestApi.Model.ICriteria<>));
            if (criteriaParameter != null)
            {
                var target = operation.Parameters.FirstOrDefault(e => e.Name == criteriaParameter.Name);
                operation.Parameters.Remove(target);
                var parameterDescriptor = context.ApiDescription.ActionDescriptor.Parameters.FirstOrDefault(e => e.Name == target.Name);
                if (parameterDescriptor.BindingInfo.BindingSource == Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource.Path)
                {
                    var modelMetadata = _modelMetadataProvider.GetMetadata(criteriaParameter.ParameterType.GetGenericArguments()[0]);
                    var keys = modelMetadata.GetKeys();
                    foreach (var key in keys)
                    {
                        var parameter = new OpenApiParameter
                        {
                            Name = parameterDescriptor.Name,
                            In = target.In,
                            Schema = new OpenApiSchema()
                        };

                        VisitParameterSchema(key.PropertyType, parameter.Schema);
                        operation.Parameters.Add(parameter);
                    }
                }
                else
                {
                    var modelMetadata = _modelMetadataProvider.GetMetadata(criteriaParameter.ParameterType.GetGenericArguments()[0]);
                    foreach (var propertyMeta in modelMetadata.GetRegularProperties())
                    {
                        var parameter = new OpenApiParameter
                        {
                            Name = propertyMeta.PropertyName,
                            In = target.In,
                            Schema = new OpenApiSchema()
                        };

                        VisitParameterSchema(propertyMeta.PropertyType, parameter.Schema);
                        operation.Parameters.Add(parameter);
                    }
                }
            }
        }

        void VisitParameterSchema(Type type, OpenApiSchema parameterSchema, bool nullable = false)
        {
            if (type == typeof(Guid))
            {
                parameterSchema.Type = "string";
                parameterSchema.Format = "uuid";
            }
            
            if (type == typeof(string))
                parameterSchema.Type = "string";

            if (type == typeof(int) || type == typeof(long))
                parameterSchema.Type = "integer";

            if (type == typeof(bool))
                parameterSchema.Type = "boolean";

            if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
                parameterSchema.Type = "number";

            if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
            {
                parameterSchema.Type = "string";
                parameterSchema.Format = "date-time";
            }

            if (nullable == true)
                parameterSchema.Nullable = true;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                VisitParameterSchema(type.GetGenericArguments().First(), parameterSchema, true);
        }
    }
}