using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Windfyr.Http.OpenWeather;
using Windfyr.Repository.Context;
using Windfyr.WebApi.Auth;
using Windfyr.WebApi.Crop;
using Windfyr.WebApi.Land;
using Windfyr.WebApi.User;

var envFilePath = FindEnvFile();
if (envFilePath is null)
{
    throw new InvalidOperationException(
        "Arquivo .env nao encontrado. Crie um .env no projeto (ou em um diretorio pai) com a chave ConnectionStrings__DefaultConnection.");
}

LoadDotEnv(envFilePath);

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Configuração de Serviços (DI Container)
// ==========================================

// Configura o EF Core para usar MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string nao configurada. Defina ConnectionStrings__DefaultConnection no arquivo .env.");
}

var authSettings = AuthSettings.FromConfiguration(builder.Configuration);
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
openWeatherSettings.Validate();

builder.Services.AddDbContext<WindfyrDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
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

app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapCropEndpoints();
app.MapLandEndpoints();

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