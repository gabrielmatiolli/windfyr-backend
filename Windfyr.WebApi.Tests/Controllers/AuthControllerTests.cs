using System.Net;
using System.Net.Http.Json;
using Windfyr.WebApi.Tests.Infrastructure;
using Xunit;

namespace Windfyr.WebApi.Tests.Controllers;

public sealed class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ReturnsOk()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "Maria",
            email = "maria@email.com",
            password = "123456",
            password_confirmation = "123456"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_WhenDuplicateEmail_ReturnsConflict()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "Joao",
            email = "joao@email.com",
            password = "123456",
            password_confirmation = "123456"
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsOk()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "joao@email.com",
            password = "123456"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
