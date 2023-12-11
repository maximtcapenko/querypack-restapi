namespace QueryPack.RestApi.Model.Annotations
{
    using Meta;

    [AttributeUsage(AttributeTargets.Class)]
    public class PreSaveProcessorAttribute : Attribute, IPipelineAnnotation
    {
        public Type ProcessorType { get; }

        public PreSaveProcessorAttribute(Type processorType)
        {
            ProcessorType = processorType;
        }
    }
}