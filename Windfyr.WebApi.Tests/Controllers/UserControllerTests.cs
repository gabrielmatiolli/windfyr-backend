using System.Net;
using System.Net.Http.Json;
using Windfyr.WebApi.Tests.Infrastructure;
using Xunit;

namespace Windfyr.WebApi.Tests.Controllers;

public sealed class UserControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UserControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetMe_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMe_ReturnsOk()
    {
        var response = await _client.PutAsJsonAsync("/api/users/me", new
        {
            name = "Joao Atualizado",
            email = "joao@email.com"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
