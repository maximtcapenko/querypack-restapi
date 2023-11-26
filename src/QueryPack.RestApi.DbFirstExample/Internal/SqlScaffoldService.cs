namespace QueryPack.RestApi.DbFirstExample.Internal
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.EntityFrameworkCore.Scaffolding;
    using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
    using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;
    using Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal;
    using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
    using Microsoft.EntityFrameworkCore.Storage;
    using RestApi.Model;

    public record SqlServerServicesOptions(string ConnectionString);

    internal class SqlScaffoldService : IScaffoldService
    {
        private readonly SqlServerServicesOptions _options;

        public SqlScaffoldService(SqlServerServicesOptions options)
        {
            _options = options;
        }

        public ScaffoldedModel ScaffoldModel(ModelCodeGenerationOptions options)
        {
            var scaffolder = Create();
            var dbModelFactoryOptions = new DatabaseModelFactoryOptions();
            var modelReverseEngineerOptions = new ModelReverseEngineerOptions();

            return scaffolder.ScaffoldModel(_options.ConnectionString, dbModelFactoryOptions, modelReverseEngineerOptions, options);
        }

        private static IReverseEngineerScaffolder Create() =>
        new ServiceCollection()
            .AddEntityFrameworkSqlServer()
            .AddLogging()
            .AddEntityFrameworkDesignTimeServices()
            .AddSingleton<LoggingDefinitions, SqlServerLoggingDefinitions>()
            .AddSingleton<IRelationalTypeMappingSource, SqlServerTypeMappingSource>()
            .AddSingleton<IAnnotationCodeGenerator, AnnotationCodeGenerator>()
            .AddSingleton<IDatabaseModelFactory, SqlServerDatabaseModelFactory>()
            .AddSingleton<IProviderConfigurationCodeGenerator, SqlServerCodeGenerator>()
            .AddSingleton<IScaffoldingModelFactory, RelationalScaffoldingModelFactory>()
            .AddSingleton<IPluralizer, Bricelam.EntityFrameworkCore.Design.Pluralizer>()
            .AddSingleton<ProviderCodeGeneratorDependencies>()
            .AddSingleton<AnnotationCodeGeneratorDependencies>()
            .BuildServiceProvider()
            .GetRequiredService<IReverseEngineerScaffolder>();

        // issue: add correct name resolver
        private static string ResolveMigrationName(DbContext context)
        {
            return $"Modify_{context.GetType().Name}";
        }
    }
}