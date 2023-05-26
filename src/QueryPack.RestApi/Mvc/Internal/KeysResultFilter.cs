namespace QueryPack.RestApi.Mvc.Internal
{
    using System.Text.Json;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.Extensions.Options;

    internal class KeysResultFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var modelMetadataProvider = context.HttpContext.RequestServices.GetRequiredService<RestApi.Model.Meta.IModelMetadataProvider>();

            if (context.Result is ObjectResult objectResult)
            {
                var jsonOptions = context.HttpContext.RequestServices.GetService<IOptions<JsonOptions>>();
                var serializerOptions = new JsonSerializerOptions(jsonOptions.Value.JsonSerializerOptions);

                var result = new ObjectResult(objectResult.Value);
                var modelType = objectResult.Value.GetType();

                var modelMeta = modelMetadataProvider.GetMetadata(modelType);
                if (modelMeta != null)
                {
                    #if NET7_0_OR_GREATER
                    serializerOptions.TypeInfoResolver = new ModelKeysOnlyJsonTypeInfoResolver(modelMeta);
                    #else
                    serializerOptions.Converters.Add(new ModelKeysOnlyJsonConverterFactory(modelMeta));
                    #endif
                    var formatter = new SystemTextJsonOutputFormatter(serializerOptions);
                    result.Formatters.Add(formatter);

                    context.Result = result;
                }
            }

            base.OnActionExecuted(context);
        }
    }
}