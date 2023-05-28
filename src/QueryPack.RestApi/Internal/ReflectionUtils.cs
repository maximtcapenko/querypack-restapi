namespace QueryPack.RestApi.Internal
{    
    internal class ReflectionUtils
    {
        public static bool IsImplementsInterface<TInterface>(Type candidate)
            => candidate.GetInterfaces().Any(e => e == typeof(TInterface));
    }
}