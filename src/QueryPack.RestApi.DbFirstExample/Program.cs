using System.Text.Json.Serialization;
using QueryPack.RestApi.DbFirstExample.Internal;
using QueryPack.RestApi.DbFirstExample.Swagger;
using QueryPack.RestApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connetionString = builder.Configuration["ConnectionString"];
builder.Services.AddRestModel(new SqlScaffoldService(new SqlServerServicesOptions(connetionString)),
 options =>
{
    options.GlobalApiPrefix = "/api";
    options.SerializerOptions = SerializerOptions =>
    {
        SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    };
    
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(s => s.EnableRestModelAnnotations());
builder.Services.AddCors(options => options.AddPolicy("local_test", builder => builder
				.SetIsOriginAllowed(_ => true)
				.AllowAnyHeader()
				.AllowAnyMethod()
				.AllowCredentials()));
				
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
