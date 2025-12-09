using Microsoft.AspNetCore.Builder;

namespace KeyVaultDemo.Api.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/", () => Results.Ok(new
        {
            name = "KeyVault Demo API",
            version = "1.0",
            endpoints = new[]
            {
                new { path = "/health", method = "GET", auth = false, description = "Health check endpoint" },
                new { path = "/items", method = "GET", auth = true, description = "Get sample items from Azure SQL (requires JWT bearer token)" }
            }
        }));

        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
    }
}
