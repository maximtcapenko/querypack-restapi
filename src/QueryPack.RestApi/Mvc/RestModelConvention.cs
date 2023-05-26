namespace QueryPack.RestApi.Mvc
{
    using System.Reflection;
    using Humanizer;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using QueryPack.RestApi.Configuration;
    using RestApi.Model;

    internal class RestModelConvention : IControllerModelConvention
    {
        private readonly RestModelOptions _options;

        public RestModelConvention(RestModelOptions options)
        {
            _options = options;
        }

        public void Apply(ControllerModel controller)
        {
            string resolvePrefix(string globalPrefix) => string.IsNullOrEmpty(globalPrefix)? "/api" : globalPrefix;

            if (controller.ControllerType.IsGenericType)
            {
                var genericType = controller.ControllerType.GenericTypeArguments[0];
                var customNameAttribute = genericType.GetCustomAttribute<RestApiAttribute>();
                var route = string.IsNullOrEmpty(customNameAttribute?.Route) == true 
                    ? $"{resolvePrefix(_options.GlobalApiPrefix)}/{genericType.Name.Pluralize().Kebaberize()}" : customNameAttribute?.Route;

                controller.ControllerName = genericType.Name;
                
                controller.Selectors.Add(new SelectorModel
                {
                    AttributeRouteModel = new AttributeRouteModel
                    {
                        Template = route
                    },
                });
            }
        }
    }
}