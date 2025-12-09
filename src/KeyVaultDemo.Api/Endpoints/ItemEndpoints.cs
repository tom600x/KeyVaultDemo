using Azure.Security.KeyVault.Secrets;
using KeyVaultDemo.Api.Models;
using KeyVaultDemo.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;

namespace KeyVaultDemo.Api.Endpoints;

public static class ItemEndpoints
{
    public static void MapItemEndpoints(this WebApplication app, string sqlConnectionSecretName)
    {
        app.MapGet("/items", async (SecretClient secretClient, SecretCache cache) =>
        {
            var connectionString = await cache.GetOrAddAsync(sqlConnectionSecretName, async () =>
            {
                var secret = await secretClient.GetSecretAsync(sqlConnectionSecretName);
                return secret.Value.Value;
            });

            var items = new List<SampleItem>();

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("SELECT TOP (10) Id, Name, CreatedAt FROM SampleItems ORDER BY CreatedAt DESC", connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(new SampleItem
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    CreatedAt = reader.GetDateTime(2)
                });
            }

            return Results.Ok(items);
        }).RequireAuthorization();
    }
}
