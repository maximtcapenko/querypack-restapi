# QueryPack.RestApi 
Simple implementation of web access to data models based on the `entity framework`. The sdk dynamically generates a web interface based on model definitions. Generated api supports filtering by model properties, including loading of nested properties and pagination.

## Getting Started
1. Create data models and data base context using entity framework api
2. Add rest api configuration in your `Startup` or `Program`
```c#
builder.Services.AddRestModel<ModelsContext>(options =>
{
    options.GlobalApiPrefix = "/api";
    options.SerializerOptions = SerializerOptions =>
    {
        SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    };
});
```
3. Run the app and make sure your web api is ready to use
