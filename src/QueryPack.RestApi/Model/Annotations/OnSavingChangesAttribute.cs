namespace QueryPack.RestApi.Model.Annotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OnSavingChangesAttribute : Attribute
    {
        public Type ProcessorType { get; }
        public OnSavingChangesAttribute(Type processorType)
        {
            ProcessorType = processorType;
        }
    }
}