using Microsoft.EntityFrameworkCore;
using Starter.Api.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "Missing connection string 'ConnectionStrings:Default' (env var ConnectionStrings__Default). See apps/api/src/Starter.Api/appsettings.Development.json.");

builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => Results.Ok(new { message = "Hello world" }));
app.MapHealthChecks("/health");

app.Run();

// Exposes Program to WebApplicationFactory in the test project.
public partial class Program { }
