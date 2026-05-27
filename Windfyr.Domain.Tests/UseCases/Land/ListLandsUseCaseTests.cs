using Moq;
using Windfyr.Domain.Repositories.Interfaces;
using Windfyr.Domain.Tests.Infrastructure;
using Windfyr.Domain.UseCases.Forecast.Inputs;
using Windfyr.Domain.UseCases.Forecast.Interfaces;
using Windfyr.Domain.UseCases.Land.Implementations;
using Windfyr.Domain.UseCases.Land.Inputs;
using Xunit;

namespace Windfyr.Domain.Tests.UseCases.Land;

public sealed class ListLandsUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_RefreshesForecastsAndMapsOutput()
    {
        var landRepo = new Mock<ILandRepository>();
        var refreshForecast = new Mock<IRefreshLandForecastHistoriesUseCase>();

        var lands = new List<Windfyr.Domain.Entities.Land>
        {
            TestDataFactory.CreateLand(id: 1),
            TestDataFactory.CreateLand(id: 2, name: "Fazenda B")
        };

        landRepo.Setup(r => r.GetAllByUserAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lands);

        refreshForecast.Setup(r => r.ExecuteAsync(It.IsAny<RefreshLandForecastHistoriesInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Windfyr.Domain.UseCases.Forecast.Outputs.RefreshLandForecastHistoriesOutput());

        var useCase = new ListLandsUseCase(landRepo.Object, refreshForecast.Object);

        var output = await useCase.ExecuteAsync(new ListLandsInput(1), CancellationToken.None);

        Assert.Equal(2, output.Lands.Count);
        refreshForecast.Verify(r => r.ExecuteAsync(It.IsAny<RefreshLandForecastHistoriesInput>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
