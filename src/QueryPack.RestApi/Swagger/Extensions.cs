namespace QueryPack.RestApi.Swagger
{
    using Swashbuckle.AspNetCore.SwaggerGen;

    public static class SwaggerGenOptionsExtensions
    {
        public static void EnableRestModelAnnotations(this SwaggerGenOptions options)
        {
            options.OperationFilter<RestModelOperationFilter>();
        }
    }
}