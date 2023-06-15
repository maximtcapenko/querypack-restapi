using Swashbuckle.AspNetCore.SwaggerGen;

namespace QueryPack.RestApi.DbFirstExample.Swagger
{
    public static class SwaggerGenOptionsExtensions
    {
        public static void EnableRestModelAnnotations(this SwaggerGenOptions options)
        {
            options.OperationFilter<RestModelOperationFilter>();
        }
    }
}