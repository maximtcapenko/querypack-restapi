namespace QueryPack.RestApi.Swagger;

using Microsoft.OpenApi;
using Model.Meta;
using Swashbuckle.AspNetCore.SwaggerGen;

internal class RestModelOperationFilter(IModelMetadataProvider modelMetadataProvider) : IOperationFilter
{
    private readonly IModelMetadataProvider _modelMetadataProvider = modelMetadataProvider;

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var criteriaParameter = context.MethodInfo.GetParameters().FirstOrDefault(p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof(RestApi.Model.ICriteria<>));
        if (criteriaParameter != null)
        {
            var target = operation.Parameters.FirstOrDefault(e => e.Name == criteriaParameter.Name);
            if (target is null) return;
            operation.Parameters.Remove(target);
            var parameterDescriptor = context.ApiDescription.ActionDescriptor.Parameters.FirstOrDefault(e => e.Name == target.Name);
            if (parameterDescriptor is null) return;
            if (parameterDescriptor.BindingInfo.BindingSource == Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource.Path)
            {
                var modelMetadata = _modelMetadataProvider.GetMetadata(criteriaParameter.ParameterType.GetGenericArguments()[0]);
                var keys = modelMetadata.GetKeys();
                foreach (var key in keys)
                {
                    var schema = new OpenApiSchema();
                    VisitParameterSchema(key.PropertyType, schema);
                    var parameter = new OpenApiParameter
                    {
                        Name = parameterDescriptor.Name,
                        In = target.In,
                        Schema = schema
                    };
                    operation.Parameters.Add(parameter);
                }
            }
            else
            {
                var modelMetadata = _modelMetadataProvider.GetMetadata(criteriaParameter.ParameterType.GetGenericArguments()[0]);
                foreach (var propertyMeta in modelMetadata.GetRegularProperties())
                {
                    var schema = new OpenApiSchema();
                    VisitParameterSchema(propertyMeta.PropertyType, schema);
                    var parameter = new OpenApiParameter
                    {
                        Name = propertyMeta.PropertyName,
                        In = target.In,
                        Schema = schema
                    };
                    operation.Parameters.Add(parameter);
                }
            }
        }
    }

    private static void VisitParameterSchema(Type type, OpenApiSchema parameterSchema, bool nullable = false)
    {
        if (type == typeof(Guid))
        {
            parameterSchema.Type = JsonSchemaType.String;
            parameterSchema.Format = "uuid";
        }

        if (type == typeof(string))
            parameterSchema.Type = JsonSchemaType.String;

        if (type == typeof(int) || type == typeof(long))
            parameterSchema.Type = JsonSchemaType.Integer;

        if (type == typeof(bool))
            parameterSchema.Type = JsonSchemaType.Boolean;

        if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
            parameterSchema.Type = JsonSchemaType.Number;

        if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
        {
            parameterSchema.Type = JsonSchemaType.String;
            parameterSchema.Format = "date-time";
        }

        if (nullable)
            parameterSchema.Type = (parameterSchema.Type ?? JsonSchemaType.Null) | JsonSchemaType.Null;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            VisitParameterSchema(type.GetGenericArguments().First(), parameterSchema, true);
    }
}