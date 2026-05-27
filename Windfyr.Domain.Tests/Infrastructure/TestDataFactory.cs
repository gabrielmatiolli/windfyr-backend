using Windfyr.Domain.Entities;

namespace Windfyr.Domain.Tests.Infrastructure;

public static class TestDataFactory
{
    public static User CreateUser(
        ulong id = 1,
        string name = "Joao",
        string email = "joao@email.com",
        string password = "123456")
    {
        return new User
        {
            Id = id,
            Name = name,
            Email = email,
            Password = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
    }

    public static Crop CreateCrop(ulong id = 1, string type = "Milho")
    {
        return new Crop
        {
            Id = id,
            Type = type,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-5)
        };
    }

    public static Land CreateLand(
        ulong id = 1,
        ulong userId = 1,
        ulong cropId = 1,
        string name = "Fazenda A",
        decimal hectares = 10,
        string city = "Ribeirao Preto",
        string state = "SP")
    {
        return new Land
        {
            Id = id,
            UserId = userId,
            CropId = cropId,
            Name = name,
            Hectares = hectares,
            City = city,
            State = state,
            Latitude = -21.1m,
            Longitude = -47.8m,
            Crop = CreateCrop(cropId),
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };
    }
}
