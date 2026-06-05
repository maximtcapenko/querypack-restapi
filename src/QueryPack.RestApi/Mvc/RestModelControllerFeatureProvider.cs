namespace QueryPack.RestApi.Mvc;

using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using RestApi.Model;

internal class RestModelControllerFeatureProvider(Assembly assembly, IEnumerable<Type> controllerCandidates) : IApplicationFeatureProvider<ControllerFeature>
{
    private readonly Assembly _modelsAssembly = assembly;

    private readonly IEnumerable<Type> _controllerCandidates = controllerCandidates;

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
