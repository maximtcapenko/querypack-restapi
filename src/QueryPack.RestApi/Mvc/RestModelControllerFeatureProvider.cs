namespace QueryPack.RestApi.Mvc
{
    using System.Collections.Generic;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using RestApi.Model;

    internal class RestModelControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        private readonly Assembly _modelsAssembly;

        private readonly IEnumerable<Type> _controllerCandidates;

        public RestModelControllerFeatureProvider(Assembly assembly, IEnumerable<Type> controllerCandidates)
        {
            _controllerCandidates = controllerCandidates;
             _modelsAssembly = assembly;
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            var candidates = _modelsAssembly.GetExportedTypes()
                                            .Where(x => x.GetCustomAttributes<RestApiAttribute>().Any())
                                            .Concat(_controllerCandidates)
                                            .Distinct();

            foreach (var candidate in candidates)
            {
                feature.Controllers.Add(
                    typeof(RestModelController<>).MakeGenericType(candidate).GetTypeInfo()
                );
            }
        }
    }
}
