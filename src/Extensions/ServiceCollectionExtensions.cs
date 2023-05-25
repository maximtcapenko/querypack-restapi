namespace QueryPack.RestApi.Extensions
{
    using Configuration;
    using Microsoft.EntityFrameworkCore;
    using Model;
    using Model.Impl;
    using Model.Meta;
    using Model.Meta.Impl;
    using Mvc;
    using Mvc.Model;
    using Mvc.Model.Binders;
    using Mvc.Model.Impl;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRestModel<TContext>(this IServiceCollection self,
             Action<RestModelOptions> options = default)
            where TContext : DbContext
        {
            var modelOptions = new RestModelOptions();
            options?.Invoke(modelOptions);

            var registeredTypes = self.AddReadWriteModel<TContext>();
            self.AddControllersWithViews(options =>
            {
                options.ModelBinderProviders.Insert(0, new ModelKeyBinderProvider());
                options.Conventions.Add(new RestModelConvention(modelOptions));
            }).AddJsonOptions(options =>
            {
                modelOptions.SerializerOptions?.Invoke(options.JsonSerializerOptions);
            })
              .ConfigureApplicationPartManager(m =>
                    m.FeatureProviders.Add(new RestModelControllerFeatureProvider(typeof(TContext).Assembly, registeredTypes)));

            var criterias = new Type[] { typeof(QueryCriteriaBinder<>), typeof(IncludeCriteriaBinder<>), typeof(OrderByCriteriaBinder<>) };

            self.AddSingleton<ICriteriaBinderProvider>(
                new RuntimeCriteriaBinderProvider(criterias.Concat(modelOptions.Criterias).ToArray()));

            return self;
        }

        internal static IEnumerable<Type> AddReadWriteModel<TContext>(this IServiceCollection self)
            where TContext : DbContext
        {
            self.AddDbContext<TContext>();
            self.AddScoped<DbContext>(s =>
            {
                var ctx = s.GetRequiredService<TContext>();
                ctx.Database.EnsureCreated();
                return ctx;
            });

            var modelTypes = new List<Type>();
            var register = typeof(ServiceCollectionExtensions).GetMethod(nameof(ServiceCollectionExtensions.RegisterModel), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            foreach (var property in typeof(TContext).GetProperties())
            {
                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                {
                    var genericRegister = register.MakeGenericMethod(property.PropertyType.GetGenericArguments());
                    genericRegister.Invoke(null, new object[] { self });
                    modelTypes.AddRange(property.PropertyType.GetGenericArguments());
                }
            }

            self.AddSingleton<IModelMetadataProvider>(new ModelMetadataProviderImpl(modelTypes));

            return modelTypes;
        }

        internal static void RegisterModel<T>(IServiceCollection services)
            where T : class
        {
            services.AddScoped<IModelReader<T>, ModelReaderImpl<T>>();
        }
    }
}