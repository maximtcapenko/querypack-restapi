using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using QueryPack.RestApi.Extensions;
using QueryPack.RestApi.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options => options.AddPolicy("local_test", builder => builder
				.SetIsOriginAllowed(_ => true)
				.AllowAnyHeader()
				.AllowAnyMethod()
				.AllowCredentials()));
				
var app = builder.Build();

app.UseCustomExceptionHandler();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }