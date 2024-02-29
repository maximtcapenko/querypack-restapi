namespace QueryPack.RestApi.Model.Meta.Extensions
{
    using System.Collections;
    using Microsoft.EntityFrameworkCore;
    using RestApi.Internal;

    internal static class PropertyMetadataExtensions
    {
        internal static async Task ProcessCollectionNavigationAsync(this PropertyMetadata propertyMetadata, DbContext dbContext, object rootInstance, IModelMetadataProvider modelMetadataProvider)
        {
            var navigationValues = new List<object>();

            var propertyValue = propertyMetadata.ValueGetter.GetValue(rootInstance);
            if (propertyValue is null) return;

            var enumeable = propertyValue as IEnumerable;
            var enumertor = enumeable.GetEnumerator();

            while (enumertor.MoveNext())
            {
                var navigationInstance = enumertor.Current;
                var navigationMeta = modelMetadataProvider.GetMetadata(navigationInstance.GetType());

                var navigationLoader = QueryUtils.GetEntityLoader(navigationMeta.ModelType);
                var navigationDbValue = await navigationLoader(dbContext, navigationInstance, navigationMeta);

                if (navigationDbValue is not null)
                    navigationValues.Add(navigationDbValue);
                else
                    navigationValues.Add(navigationInstance);
            }

            propertyMetadata.ValueSetter.SetValue(rootInstance, navigationValues);
        }
    }
}