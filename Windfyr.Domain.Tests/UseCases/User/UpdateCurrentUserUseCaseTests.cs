using Moq;
using Windfyr.Domain.Repositories.Interfaces;
using Windfyr.Domain.Tests.Infrastructure;
using Windfyr.Domain.UseCases.User.Implementations;
using Windfyr.Domain.UseCases.User.Inputs;
using Xunit;

namespace Windfyr.Domain.Tests.UseCases.User;

public sealed class UpdateCurrentUserUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenMissingFields_ThrowsInvalidOperation()
    {
        var repo = new Mock<IUserRepository>();
        var useCase = new UpdateCurrentUserUseCase(repo.Object);

        var input = new UpdateCurrentUserInput(1, "", "");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(input, CancellationToken.None));

        Assert.Equal("Nome e email sao obrigatorios.", ex.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmailExists_ThrowsInvalidOperation()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestDataFactory.CreateUser(email: "joao@email.com"));
        repo.Setup(r => r.ExistsByEmailAsync("outro@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = new UpdateCurrentUserUseCase(repo.Object);

        var input = new UpdateCurrentUserInput(1, "Joao", "outro@email.com");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(input, CancellationToken.None));

        Assert.Equal("Email ja cadastrado.", ex.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPasswordMismatch_ThrowsInvalidOperation()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestDataFactory.CreateUser());

        var useCase = new UpdateCurrentUserUseCase(repo.Object);

        var input = new UpdateCurrentUserInput(1, "Joao", "joao@email.com", "123456", "654321");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(input, CancellationToken.None));

        Assert.Equal("A confirmacao de senha nao confere.", ex.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValid_ReturnsOutput()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestDataFactory.CreateUser());
        repo.Setup(r => r.ExistsByEmailAsync("joao@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var useCase = new UpdateCurrentUserUseCase(repo.Object);

        var input = new UpdateCurrentUserInput(1, "Joao", "joao@email.com");
        var output = await useCase.ExecuteAsync(input, CancellationToken.None);

        Assert.Equal("Perfil atualizado com sucesso.", output.Message);
        Assert.Equal("Joao", output.User.Name);
    }
}
