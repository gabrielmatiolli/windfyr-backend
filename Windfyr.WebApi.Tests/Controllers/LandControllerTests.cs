using System.Net;
using System.Net.Http.Json;
using Windfyr.WebApi.Tests.Infrastructure;
using Xunit;

namespace Windfyr.WebApi.Tests.Controllers;

public sealed class LandControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LandControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetLands_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/lands");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetLand_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/lands/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateLand_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/lands", new
        {
            crop_id = 1,
            name = "Fazenda Nova",
            hectares = 12,
            state = "SP",
            city = "Ribeirao Preto",
            neighborhood = "Zona Rural",
            street = "Estrada",
            number = "100",
            latitude = -21.1,
            longitude = -47.8
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task UpdateLand_ReturnsOk()
    {
        var response = await _client.PutAsJsonAsync("/api/lands/1", new
        {
            name = "Fazenda Atualizada",
            hectares = 15
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteLand_ReturnsNoContentOrNotFound()
    {
        var landId = await CreateLandAsync();
        var response = await _client.DeleteAsync($"/api/lands/{landId}");

        Assert.True(response.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetForecast_ReturnsOk()
    {
        var landId = await CreateLandAsync();
        var response = await _client.GetAsync($"/api/lands/{landId}/forecast");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<ulong> CreateLandAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/lands", new
        {
            crop_id = 1,
            name = "Fazenda Teste",
            hectares = 12,
            state = "SP",
            city = "Ribeirao Preto",
            neighborhood = "Zona Rural",
            street = "Estrada",
            number = "100",
            latitude = -21.1,
            longitude = -47.8
        });

        response.EnsureSuccessStatusCode();

        var location = response.Headers.Location?.ToString();
        if (string.IsNullOrWhiteSpace(location))
        {
            throw new InvalidOperationException("Location header ausente ao criar land.");
        }

        var idSegment = location.Split('/').Last();
        if (!ulong.TryParse(idSegment, out var landId))
        {
            throw new InvalidOperationException($"Nao foi possivel extrair o id da land: {location}.");
        }

        return landId;
    }
}
