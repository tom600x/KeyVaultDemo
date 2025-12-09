using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using KeyVaultDemo.Api.Endpoints;
using KeyVaultDemo.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var app = BuildWebApplication(args);
        app.Run();
    }

    public static WebApplication BuildWebApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configuration binding for API's Key Vault app registration (App A)
        var keyVaultConfig = builder.Configuration.GetSection("KeyVaultClient");
        var tenantId = keyVaultConfig["TenantId"];
        var clientId = keyVaultConfig["ClientId"];
        var clientSecret = keyVaultConfig["ClientSecret"];
        var vaultUrl = keyVaultConfig["VaultUrl"];
        var sqlConnectionSecretName = keyVaultConfig["SqlConnectionSecretName"] ?? "SqlConnectionString";

        // Configure SecretClient used by the API to get DB connection string
        if (!string.IsNullOrEmpty(tenantId) && !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret) && !string.IsNullOrEmpty(vaultUrl))
        {
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var secretClient = new SecretClient(new Uri(vaultUrl), credential);
            builder.Services.AddSingleton(secretClient);
        }

        builder.Services.AddSingleton<SecretCache>();

        // Configure JWT bearer auth for API (App B)
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

        builder.Services.AddAuthorization();

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        // Endpoint mappings defined in separate extension for clarity
        app.MapHealthEndpoints();
        app.MapItemEndpoints(sqlConnectionSecretName);

        return app;
    }
}
