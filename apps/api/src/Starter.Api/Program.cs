using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Starter.Api.Auth;
using Starter.Api.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "Missing connection string 'ConnectionStrings:Default' (env var ConnectionStrings__Default). See apps/api/src/Starter.Api/appsettings.Development.json.");

builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
builder.Services.AddSupabaseAuth();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}

app.UseAuthentication();
app.UseAuthorization();

// Everything requires a signed-in user unless it opts out (fallback policy in AddSupabaseAuth).
app.MapGet("/", () => Results.Ok(new { message = "Hello world" })).AllowAnonymous();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapGet("/me", (ClaimsPrincipal user) => TypedResults.Ok(new { id = user.FindFirstValue("sub") }));

app.Run();

// Exposes Program to WebApplicationFactory in the test project.
public partial class Program { }
