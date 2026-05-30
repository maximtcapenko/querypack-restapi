namespace QueryPack.RestApi.Model.Annotations
{
    using Meta;
    
    [AttributeUsage(AttributeTargets.Class)]
    public class PostSaveProcessorAttribute(Type processorType) : Attribute, IPipelineAnnotation
    {
        public Type ProcessorType { get; } = processorType;
    }
}
