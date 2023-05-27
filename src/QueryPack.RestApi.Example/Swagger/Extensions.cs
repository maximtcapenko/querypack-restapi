using Swashbuckle.AspNetCore.SwaggerGen;

namespace QueryPack.RestApi.Example.Swagger
{
    public static class SwaggerGenOptionsExtensions
    {
        public static void EnableRestModelAnnotations(this SwaggerGenOptions options)
        {
            options.OperationFilter<RestModelOperationFilter>();
        }
    }
}