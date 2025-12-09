using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace KeyVaultDemo.Api.Tests;

public class BasicApiTests
{
    private WebApplicationFactory<Program> _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace SecretClient with a mock that returns a fake connection string
                    var secretClientMock = new Mock<SecretClient>();
                    secretClientMock
                        .Setup(c => c.GetSecretAsync(It.IsAny<string>(), null, default))
                        .ReturnsAsync(Response.FromValue(
                            new KeyVaultSecret("SqlConnectionString", "Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;"),
                            Mock.Of<Response>()));

                    services.AddSingleton(secretClientMock.Object);
                });
            });
    }

    [TearDown]
    public void TearDown()
    {
        _factory.Dispose();
    }

    [Test]
    public async Task Health_Endpoint_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
