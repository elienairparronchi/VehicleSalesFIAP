using VehicleSalesFIAP.Api.Middleware;
using VehicleSalesFIAP.Api.Extensions;
using VehicleSalesFIAP.Application;
using VehicleSalesFIAP.Infrastructure;
using VehicleSalesFIAP.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApiDocumentation();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<VehicleSalesDbContext>("sqlserver");
builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddApiAuthorization();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("OpenApi:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

app.Run();

public partial class Program;
