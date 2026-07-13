using EasyDropApplication.Interfaces;
using EasyDropInfrastructure.DataBase;
using EasyDropInfrastructure.ExternalApi;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. РАЗРЕШАЕМ CORS ДЛЯ ЛЮБОГО ЛОКАЛЬНОГО ПОРТА (5173, 5174 и т.д.)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllLocal", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Build connection string from Railway environment variables
// Railway Postgres provides: PGHOST, PGPORT, PGUSER, PGPASSWORD, PGDATABASE
var host = Environment.GetEnvironmentVariable("PGHOST") ?? "localhost";
var port = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
var user = Environment.GetEnvironmentVariable("PGUSER") ?? "postgres";
var password = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "";
var database = Environment.GetEnvironmentVariable("PGDATABASE") ?? "EasyDropDb";

var connectionString = $"Host={host};Port={port};Username={user};Password={password};Database={database};";

// Fallback to config if env vars not set
if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string not found in configuration or environment variables.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHttpClient<IGiveawayService, IsThereAnyDealService>(client =>
{
    client.BaseAddress = new Uri("https://api.isthereanydeal.com/");
});

builder.Services.AddHttpClient<IEpicGamesService, EpicGamesService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 2. ВКЛЮЧАЕМ CORS (СТРОГО ДО UseAuthorization!)
app.UseCors("AllowAllLocal");

// 3. ПОЛНОСТЬЮ ОТКЛЮЧАЕМ HTTPS REDIRECT ДЛЯ ЛОКАЛЬНОЙ РАЗРАБОТКИ
// app.UseHttpsRedirection(); 

app.UseAuthorization();
app.MapControllers();

app.Run();
