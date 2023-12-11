namespace QueryPack.RestApi.Model.Annotations
{
    using Meta;
    
    [AttributeUsage(AttributeTargets.Class)]
    public class PostSaveProcessorAttribute : Attribute, IPipelineAnnotation
    {
        public Type ProcessorType { get; }

        public PostSaveProcessorAttribute(Type processorType)
        {
            ProcessorType = processorType;
        }
    }
}
