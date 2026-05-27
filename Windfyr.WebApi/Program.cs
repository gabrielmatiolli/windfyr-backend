using Microsoft.EntityFrameworkCore;
using Windfyr.Domain.Repositories;
using Windfyr.Repository.Context;
using Windfyr.Repository.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Configuração de Serviços (DI Container)
// ==========================================

// Configura o EF Core para usar SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<WindfyrDbContext>(options =>
    options.UseSqlServer(connectionString));

// Injeção de Dependência
builder.Services.AddScoped<IFireAlertRepository, FireAlertRepository>();

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

// ==========================================
// 3. Mapeamento dos Endpoints (Rotas)
// ==========================================

app.MapGet("/api/fires", async (IFireAlertRepository repository, CancellationToken ct) =>
{
    var alerts = await repository.GetActiveAlertsAsync(ct);
    return Results.Ok(alerts);
})
.WithName("GetActiveFires")
.WithSummary("Obtém os focos de incêndio ativos")
.WithDescription("Retorna uma lista de focos de incêndio detectados nas últimas horas, classificados por risco.");

app.Run();