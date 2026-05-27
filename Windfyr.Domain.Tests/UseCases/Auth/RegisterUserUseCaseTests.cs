using Moq;
using Windfyr.Domain.Repositories.Interfaces;
using Windfyr.Domain.Tests.Infrastructure;
using Windfyr.Domain.UseCases.Auth.Implementations;
using Windfyr.Domain.UseCases.Auth.Inputs;
using Xunit;

namespace Windfyr.Domain.Tests.UseCases.Auth;

public sealed class RegisterUserUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenMissingFields_ThrowsInvalidOperation()
    {
        var repo = new Mock<IUserRepository>();
        var useCase = new RegisterUserUseCase(repo.Object);

        var input = new RegisterUserInput("", "", "", "");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(input, CancellationToken.None));

        Assert.Equal("Todos os campos sao obrigatorios.", ex.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPasswordTooShort_ThrowsInvalidOperation()
    {
        var repo = new Mock<IUserRepository>();
        var useCase = new RegisterUserUseCase(repo.Object);

        var input = new RegisterUserInput("Joao", "joao@email.com", "123", "123");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(input, CancellationToken.None));

        Assert.Equal("A senha deve ter no minimo 6 caracteres.", ex.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPasswordConfirmationMismatch_ThrowsInvalidOperation()
    {
        var repo = new Mock<IUserRepository>();
        var useCase = new RegisterUserUseCase(repo.Object);

        var input = new RegisterUserInput("Joao", "joao@email.com", "123456", "654321");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(input, CancellationToken.None));

        Assert.Equal("A confirmacao de senha nao confere.", ex.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmailExists_ThrowsInvalidOperation()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.ExistsByEmailAsync("joao@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = new RegisterUserUseCase(repo.Object);

        var input = new RegisterUserInput("Joao", "joao@email.com", "123456", "123456");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(input, CancellationToken.None));

        Assert.Equal("Email ja cadastrado.", ex.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValid_ReturnsOutput()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        repo.Setup(r => r.AddAsync(It.IsAny<Windfyr.Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback<Windfyr.Domain.Entities.User, CancellationToken>((user, _) => user.Id = 7);

        var useCase = new RegisterUserUseCase(repo.Object);
        var input = new RegisterUserInput("Joao", "joao@email.com", "123456", "123456");

        var output = await useCase.ExecuteAsync(input, CancellationToken.None);

        Assert.Equal("Usuario criado com sucesso", output.Message);
        Assert.Equal(7ul, output.User.Id);
        Assert.Equal("Joao", output.User.Name);
        Assert.Equal("joao@email.com", output.User.Email);
    }
}
