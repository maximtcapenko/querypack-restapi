namespace QueryPack.RestApi.Mvc.Model.Impl
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using RestApi.Model;
    using RestApi.Model.Internal.Criterias;

    internal class RuntimeModelBinder<TModel> : IModelBinder
        where TModel : class
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext, nameof(bindingContext));

            var criteriaBuinderProvider =
                bindingContext.HttpContext.RequestServices.GetService<ICriteriaBinderProvider>();

            var modelMetadataProvider = bindingContext.HttpContext.RequestServices.GetRequiredService<RestApi.Model.Meta.IModelMetadataProvider>();
            var criterias = new List<ICriteria<TModel>>();
            var context = new CriteriaBindingContext<TModel>(bindingContext, criterias, modelMetadataProvider);

            foreach (var binder in criteriaBuinderProvider.GetBinders<TModel>())
                binder.BindModel(context);

            var criteria = new RootCriteria<TModel>(criterias.ToArray());

            bindingContext.Result = ModelBindingResult.Success(criteria);

            return Task.CompletedTask;
        }
    }
}