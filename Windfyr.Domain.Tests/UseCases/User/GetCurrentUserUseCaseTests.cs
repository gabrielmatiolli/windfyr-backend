using Moq;
using Windfyr.Domain.Repositories.Interfaces;
using Windfyr.Domain.Tests.Infrastructure;
using Windfyr.Domain.UseCases.User.Implementations;
using Windfyr.Domain.UseCases.User.Inputs;
using Xunit;

namespace Windfyr.Domain.Tests.UseCases.User;

public sealed class GetCurrentUserUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenUserNotFound_ThrowsUnauthorized()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Windfyr.Domain.Entities.User?)null);

        var useCase = new GetCurrentUserUseCase(repo.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => useCase.ExecuteAsync(new GetCurrentUserInput(1), CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserExists_ReturnsOutput()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestDataFactory.CreateUser());

        var useCase = new GetCurrentUserUseCase(repo.Object);

        var output = await useCase.ExecuteAsync(new GetCurrentUserInput(1), CancellationToken.None);

        Assert.Equal(1ul, output.Id);
        Assert.Equal("Joao", output.Name);
    }
}
