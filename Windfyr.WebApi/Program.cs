using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Windfyr.Domain.Http.Interfaces;
using Windfyr.Domain.Repositories.Interfaces;
using Windfyr.Domain.UseCases.Auth.Implementations;
using Windfyr.Domain.UseCases.Auth.Inputs;
using Windfyr.Domain.UseCases.Auth.Interfaces;
using Windfyr.Domain.UseCases.Crop.Implementations;
using Windfyr.Domain.UseCases.Crop.Interfaces;
using Windfyr.Domain.UseCases.Forecast.Implementations;
using Windfyr.Domain.UseCases.Forecast.Interfaces;
using Windfyr.Domain.UseCases.Irrigation.Implementations;
using Windfyr.Domain.UseCases.Irrigation.Interfaces;
using Windfyr.Domain.UseCases.Land.Implementations;
using Windfyr.Domain.UseCases.Land.Interfaces;
using Windfyr.Domain.UseCases.User.Implementations;
using Windfyr.Domain.UseCases.User.Interfaces;
using Windfyr.Http.OpenWeather;
using Windfyr.Repository.Context;
using Windfyr.Repository.Repositories;
using Windfyr.WebApi.Auth;

var builder = WebApplication.CreateBuilder(args);
var isTesting = builder.Environment.IsEnvironment("Testing");

var envFilePath = FindEnvFile();
if (envFilePath is null)
{
    if (!isTesting)
    {
        throw new InvalidOperationException(
            "Arquivo .env nao encontrado. Crie um .env no projeto (ou em um diretorio pai) com a chave ConnectionStrings__DefaultConnection.");
    }
}
else
{
    LoadDotEnv(envFilePath);
}

// ==========================================
// 1. Configuração de Serviços (DI Container)
// ==========================================

// Configura o EF Core para usar MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    if (isTesting)
    {
        connectionString = "Server=localhost;Database=windfyr;User=root;Password=pass;";
    }
    else
    {
        throw new InvalidOperationException(
            "Connection string nao configurada. Defina ConnectionStrings__DefaultConnection no arquivo .env.");
    }
}

var authSettings = AuthSettings.FromConfiguration(builder.Configuration);
if (isTesting && string.IsNullOrWhiteSpace(authSettings.Key))
{
    authSettings = new AuthSettings
    {
        Key = new string('x', 32),
        Issuer = authSettings.Issuer,
        Audience = authSettings.Audience,
        ExpiresInMinutes = authSettings.ExpiresInMinutes
    };
}
authSettings.Validate();

var openWeatherSettings = new OpenWeatherSettings
{
    ApiKey = builder.Configuration["OPENWEATHER:APIKEY"]
        ?? Environment.GetEnvironmentVariable("OPENWEATHER__APIKEY")
        ?? string.Empty,
    BaseUrl = builder.Configuration["OPENWEATHER:BASEURL"]
        ?? Environment.GetEnvironmentVariable("OPENWEATHER__BASEURL")
        ?? "https://api.openweathermap.org"
};
if (isTesting && string.IsNullOrWhiteSpace(openWeatherSettings.ApiKey))
{
    openWeatherSettings = new OpenWeatherSettings
    {
        ApiKey = "testing",
        BaseUrl = openWeatherSettings.BaseUrl
    };
}
openWeatherSettings.Validate();

if (isTesting)
{
    builder.Services.AddDbContext<WindfyrDbContext>(options =>
        options.UseSqlite("DataSource=:memory:"));
}
else
{
    builder.Services.AddDbContext<WindfyrDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
}
builder.Services.AddSingleton(authSettings);
builder.Services.AddSingleton(openWeatherSettings);
builder.Services.AddHttpClient<IOpenWeatherClient, OpenWeatherClient>((sp, client) =>
{
    var settings = sp.GetRequiredService<OpenWeatherSettings>();
    client.BaseAddress = new Uri(settings.BaseUrl);
});
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSettings.Key)),
            ValidateIssuer = true,
            ValidIssuer = authSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = authSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICropRepository, CropRepository>();
builder.Services.AddScoped<ILandRepository, LandRepository>();
builder.Services.AddScoped<IForecastHistoryRepository, ForecastHistoryRepository>();
builder.Services.AddScoped<IIrrigationHistoryRepository, IrrigationHistoryRepository>();

builder.Services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
builder.Services.AddScoped<ILoginUserUseCase>(sp =>
{
    var userRepository = sp.GetRequiredService<IUserRepository>();
    var settings = sp.GetRequiredService<AuthSettings>();
    var tokenSettings = new AuthTokenSettings
    {
        Key = settings.Key,
        Issuer = settings.Issuer,
        Audience = settings.Audience,
        ExpiresInMinutes = settings.ExpiresInMinutes
    };

    return new LoginUserUseCase(userRepository, tokenSettings);
});

builder.Services.AddScoped<IGetCurrentUserUseCase, GetCurrentUserUseCase>();
builder.Services.AddScoped<IUpdateCurrentUserUseCase, UpdateCurrentUserUseCase>();
builder.Services.AddScoped<IListCropsUseCase, ListCropsUseCase>();
builder.Services.AddScoped<IApplyIrrigationRiskUseCase, ApplyIrrigationRiskUseCase>();
builder.Services.AddScoped<ICaptureCurrentForecastUseCase, CaptureCurrentForecastUseCase>();
builder.Services.AddScoped<CaptureCurrentForecastUseCase>();
builder.Services.AddScoped<IRefreshLandForecastHistoriesUseCase, RefreshLandForecastHistoriesUseCase>();
builder.Services.AddScoped<IListLandsUseCase, ListLandsUseCase>();
builder.Services.AddScoped<IGetLandUseCase, GetLandUseCase>();
builder.Services.AddScoped<ICreateLandUseCase, CreateLandUseCase>();
builder.Services.AddScoped<IUpdateLandUseCase, UpdateLandUseCase>();
builder.Services.AddScoped<IDeleteLandUseCase, DeleteLandUseCase>();
builder.Services.AddScoped<IGetLandForecastUseCase, GetLandForecastUseCase>();

// Configuração do Swagger
builder.Services.AddEndpointsApiExplorer(); // Permite que as Minimal APIs sejam descobertas
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Windfyr API", Version = "v1", Description = "API de monitoramento de focos de incêndio." });
});

var app = builder.Build();

// ==========================================
// 2. Configuração do Pipeline (Middleware)
// ==========================================

// Ativa o Swagger apenas em ambiente de desenvolvimento (boa prática de segurança)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Windfyr API v1");
        // Opcional: Faz o Swagger abrir na raiz (http://localhost:porta/) em vez de /swagger
        // c.RoutePrefix = string.Empty; 
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static string? FindEnvFile()
{
    var searchRoots = new[]
    {
        Directory.GetCurrentDirectory(),
        AppContext.BaseDirectory
    };

    foreach (var root in searchRoots)
    {
        var current = new DirectoryInfo(root);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, ".env");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }
    }

    return null;
}

static void LoadDotEnv(string filePath)
{
    foreach (var rawLine in File.ReadLines(filePath))
    {
        var line = rawLine.Trim();

        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
        {
            continue;
        }

        if (line.StartsWith("export ", StringComparison.OrdinalIgnoreCase))
        {
            line = line[7..].Trim();
        }

        var separatorIndex = line.IndexOf('=');
        if (separatorIndex <= 0)
        {
            continue;
        }

        var key = line[..separatorIndex].Trim();
        if (string.IsNullOrWhiteSpace(key))
        {
            continue;
        }

        var value = line[(separatorIndex + 1)..].Trim();
        if ((value.StartsWith('"') && value.EndsWith('"'))
            || (value.StartsWith('\'') && value.EndsWith('\'')))
        {
            value = value[1..^1];
        }

        Environment.SetEnvironmentVariable(key, value);
    }
}

public partial class Program;
