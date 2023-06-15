namespace QueryPack.RestApi.Internal
{
    using System.Reflection;
    using System.Runtime.Loader;
    using Humanizer;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.EntityFrameworkCore;

    internal static class CSharpCompilationUtils
    {
        public static Assembly Compile(IEnumerable<string> sourceFiles, params Assembly[] referencedAssemblies)
        {
            var metadataReferences = new List<MetadataReference>();

            foreach (var assembly in referencedAssemblies)
            {
                metadataReferences.Add(MetadataReference.CreateFromFile(assembly.Location));
            }

            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp10);
            var parsedSyntaxTrees = sourceFiles.Select(f => SyntaxFactory.ParseSyntaxTree(f, options));

            var compilation = CSharpCompilation.Create($"DynamicAssembly_{Guid.NewGuid().ToString().Underscore()}",
                    parsedSyntaxTrees,
                    references: CompilationReferences(metadataReferences.ToArray()),
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                        optimizationLevel: OptimizationLevel.Release,
                        assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));

            using var assemblyStream = new MemoryStream();
            var result = compilation.Emit(assemblyStream);
            if (!result.Success)
            {
                var failures = result.Diagnostics
                    .Where(diagnostic => diagnostic.IsWarningAsError ||
                                         diagnostic.Severity == DiagnosticSeverity.Error);

                var error = failures.FirstOrDefault();
                throw new Exception($"{error?.Id}: {error?.GetMessage()}");
            }

            var assemblyLoadContext = new AssemblyLoadContext("DynamicAssembly", isCollectible: true);
            assemblyStream.Seek(0, SeekOrigin.Begin);

            return assemblyLoadContext.LoadFromStream(assemblyStream);
        }

        private static List<MetadataReference> CompilationReferences(params MetadataReference[] metadataReferences)
        {
            var refs = new List<MetadataReference>();
            var referencedAssemblies = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
            refs.AddRange(referencedAssemblies.Select(a => MetadataReference.CreateFromFile(Assembly.Load(a).Location)));
            refs.AddRange(metadataReferences);
            refs.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            refs.Add(MetadataReference.CreateFromFile(typeof(BackingFieldAttribute).Assembly.Location));
            refs.Add(MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location));
            refs.Add(MetadataReference.CreateFromFile(typeof(System.Data.Common.DbConnection).Assembly.Location));
            refs.Add(MetadataReference.CreateFromFile(typeof(System.Linq.Expressions.Expression).Assembly.Location));
            return refs;
        }
    }
}