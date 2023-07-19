namespace QueryPack.RestApi.Extensions
{
    using System.Reflection;
    using Configuration;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Scaffolding;
    using Model;
    using Model.Impl;
    using Model.Meta;
    using Model.Meta.Impl;
    using Mvc;
    using Mvc.Model;
    using Mvc.Model.Binders;
    using Mvc.Model.Impl;
    using Internal;
    using Exceptions;
    using Exceptions.Internal;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRestModel<TContext>(this IServiceCollection self,
             Action<RestModelOptions> options = default)
            where TContext : DbContext
        {
            var modelOptions = new RestModelOptions();
            options?.Invoke(modelOptions);

            var registeredTypes = self.AddReadWriteModel<TContext>(modelOptions);
            var mvcBuilder = self.AddControllersWithViews(options =>
            {
                options.ModelBinderProviders.Insert(0, new ModelKeyBinderProvider());
                options.Conventions.Add(new RestModelConvention(modelOptions));
            }).AddJsonOptions(options =>
            {
                modelOptions.SerializerOptions?.Invoke(options.JsonSerializerOptions);
            })
              .ConfigureApplicationPartManager(m =>
                    m.FeatureProviders.Add(new RestModelControllerFeatureProvider(typeof(TContext).Assembly, registeredTypes)));

            modelOptions.MvcBuilderOptions?.Invoke(mvcBuilder);

            var criterias = new Type[] { typeof(QueryCriteriaBinder<>), typeof(IncludeCriteriaBinder<>), typeof(OrderByCriteriaBinder<>), typeof(KeyCriteriaBinder<>) };

            self.AddSingleton<ICriteriaBinderProvider>(
                new RuntimeCriteriaBinderProvider(criterias.Concat(modelOptions.Criterias).ToArray()));

            self.AddSingleton<IExceptionHandlingResultFactory>(new ExceptionHandlingResultFactoryImpl(modelOptions.ExceptionMessageBuilders));

            return self;
        }

        public static IServiceCollection AddRestModel(this IServiceCollection self,
            IScaffoldService scaffolder, Action<RestModelOptions> options = default)
        {
            var scaffoldedContextClassName = "ScaffoldedContext";
            var rootNamesapce = "QueryPack.RestApi.Model.Models";

            var modelCodeGenerationOptions = new ModelCodeGenerationOptions
            {
                RootNamespace = rootNamesapce,
                ContextName = scaffoldedContextClassName,
                ContextNamespace = rootNamesapce,
                ModelNamespace = rootNamesapce,
                UseDataAnnotations = true,
                SuppressConnectionStringWarning = true,
            };

            var scaffoldedModel = scaffolder.ScaffoldModel(modelCodeGenerationOptions);
            var referencedAssemblies = scaffolder.GetType().Assembly.GetReferencedAssemblies()
                           .Select(a => Assembly.Load(a));

            var dynamicContextAssembly = CSharpCompilationUtils.Compile(scaffoldedModel.AdditionalFiles.Select(e => e.Code).Concat(new[] { scaffoldedModel.ContextFile.Code }),
                 referencedAssemblies.ToArray());

            var dynamicContext = GetContext(dynamicContextAssembly, rootNamesapce, scaffoldedContextClassName);

            var addRestModel = typeof(ServiceCollectionExtensions).GetMethods().FirstOrDefault(e => e.Name == nameof(AddRestModel)
            && e.IsGenericMethod);

            var addRestModelGeneric = addRestModel.MakeGenericMethod(dynamicContext.GetType());
            addRestModelGeneric.Invoke(null, new object[] { self, options });

            return self;
        }

        internal static IEnumerable<Type> AddReadWriteModel<TContext>(this IServiceCollection self, RestModelOptions restModelOptions)
            where TContext : DbContext
        {
            self.AddDbContext<TContext>(options => restModelOptions.ContextOptionsBuilder?.Invoke(options));
            self.AddScoped<DbContext>(s =>
            {
                var ctx = s.GetRequiredService<TContext>();
                return ctx;
            });

            var modelTypes = new List<Type>();

            foreach (var property in typeof(TContext).GetProperties())
            {
                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                {
                    modelTypes.AddRange(property.PropertyType.GetGenericArguments());
                }
            }

            self.AddSingleton<IModelMetadataProvider>(new ModelMetadataProviderImpl(modelTypes));

            return modelTypes;
        }

        internal static DbContext GetContext(Assembly assembly, string rootNamesapce, string contextClassName)
        {
            var type = assembly.GetType($"{rootNamesapce}.{contextClassName}");
            _ = type ?? throw new Exception("DataContext type not found");

            var constr = type.GetConstructor(Type.EmptyTypes);
            _ = constr ?? throw new Exception("DataContext ctor not found");

            return (DbContext)constr.Invoke(null);
        }
    }
}