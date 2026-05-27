using Moq;
using Windfyr.Domain.Http.Interfaces;
using Windfyr.Domain.Http.Models;
using Windfyr.Domain.Repositories.Interfaces;
using Windfyr.Domain.Tests.Infrastructure;
using Windfyr.Domain.UseCases.Forecast.Implementations;
using Windfyr.Domain.UseCases.Forecast.Inputs;
using Windfyr.Domain.UseCases.Irrigation.Interfaces;
using Xunit;

namespace Windfyr.Domain.Tests.UseCases.Forecast;

public sealed class CaptureCurrentForecastUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenLandNotFound_ThrowsKeyNotFound()
    {
        var landRepo = new Mock<ILandRepository>();
        landRepo.Setup(r => r.GetByIdAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Windfyr.Domain.Entities.Land?)null);

        var forecastRepo = new Mock<IForecastHistoryRepository>();
        var weatherClient = new Mock<IOpenWeatherClient>();
        var irrigation = new Mock<IApplyIrrigationRiskUseCase>();

        var useCase = new CaptureCurrentForecastUseCase(
            landRepo.Object,
            forecastRepo.Object,
            weatherClient.Object,
            irrigation.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => useCase.ExecuteAsync(new CaptureCurrentForecastInput(1, 1), CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_WhenForecastChanges_AddsHistory()
    {
        var land = TestDataFactory.CreateLand();

        var landRepo = new Mock<ILandRepository>();
        landRepo.Setup(r => r.GetByIdAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(land);

        var forecastRepo = new Mock<IForecastHistoryRepository>();
        forecastRepo.Setup(r => r.GetLatestAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Windfyr.Domain.Entities.ForecastHistory?)null);
        forecastRepo.Setup(r => r.AddAsync(It.IsAny<Windfyr.Domain.Entities.ForecastHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var weatherClient = new Mock<IOpenWeatherClient>();
        weatherClient.Setup(c => c.GetCurrentWeatherAsync(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OpenWeatherCurrentWeather(25, 1010, 40, "clear", "01d", DateTimeOffset.UtcNow, "Ribeirao"));

        var irrigation = new Mock<IApplyIrrigationRiskUseCase>();
        irrigation.Setup(i => i.ExecuteAsync(It.IsAny<Windfyr.Domain.UseCases.Irrigation.Inputs.ApplyIrrigationRiskInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Windfyr.Domain.UseCases.Irrigation.Outputs.ApplyIrrigationRiskOutput());

        var useCase = new CaptureCurrentForecastUseCase(
            landRepo.Object,
            forecastRepo.Object,
            weatherClient.Object,
            irrigation.Object);

        await useCase.ExecuteAsync(new CaptureCurrentForecastInput(1, 1), CancellationToken.None);

        forecastRepo.Verify(r => r.AddAsync(It.IsAny<Windfyr.Domain.Entities.ForecastHistory>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
