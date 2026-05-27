using Moq;
using Windfyr.Domain.Repositories.Interfaces;
using Windfyr.Domain.Tests.Infrastructure;
using Windfyr.Domain.UseCases.Land.Implementations;
using Windfyr.Domain.UseCases.Land.Inputs;
using Xunit;

namespace Windfyr.Domain.Tests.UseCases.Land;

public sealed class UpdateLandUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenMissingFields_ThrowsInvalidOperation()
    {
        var repo = new Mock<ILandRepository>();
        var useCase = new UpdateLandUseCase(repo.Object);

        var input = new UpdateLandInput(1, 1, null, null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(input, CancellationToken.None));

        Assert.Equal("Informe ao menos um campo para atualizar.", ex.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLandNotFound_ThrowsKeyNotFound()
    {
        var repo = new Mock<ILandRepository>();
        repo.Setup(r => r.GetByIdAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Windfyr.Domain.Entities.Land?)null);

        var useCase = new UpdateLandUseCase(repo.Object);

        var input = new UpdateLandInput(1, 1, "Fazenda", null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => useCase.ExecuteAsync(input, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_WhenValid_ReturnsOutput()
    {
        var repo = new Mock<ILandRepository>();
        repo.Setup(r => r.GetByIdAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestDataFactory.CreateLand());
        repo.Setup(r => r.UpdateAsync(It.IsAny<Windfyr.Domain.Entities.Land>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.GetByIdAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestDataFactory.CreateLand());

        var useCase = new UpdateLandUseCase(repo.Object);

        var input = new UpdateLandInput(1, 1, "Nova Fazenda", 20);
        var output = await useCase.ExecuteAsync(input, CancellationToken.None);

        Assert.Equal("Nova Fazenda", output.Land.Name);
    }
}
