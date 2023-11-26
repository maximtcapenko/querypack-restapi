namespace QueryPack.RestApi.Internal
{    
    using System.Reflection;

    internal class ReflectionUtils
    {
        public static bool IsImplementsInterface<TInterface>(Type candidate)
            => candidate.GetInterfaces().Any(e => e == typeof(TInterface));

        public static MethodInfo GetContainsMethod() => typeof(Enumerable).GetMethods().FirstOrDefault(e => e.Name == nameof(Enumerable.Contains)
               && e.IsStatic && e.GetParameters().Length == 2);

        public static MethodInfo GetSelectMethod() => typeof(Enumerable).GetMethods().FirstOrDefault(e => e.Name == nameof(Enumerable.Select)
                && e.GetParameters().Length == 2);
    }
}