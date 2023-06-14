using Microsoft.EntityFrameworkCore.Scaffolding;

namespace QueryPack.RestApi.Model
{
    public interface IScaffoldService
    {
        ScaffoldedModel ScaffoldModel(ModelCodeGenerationOptions options);
    }
}