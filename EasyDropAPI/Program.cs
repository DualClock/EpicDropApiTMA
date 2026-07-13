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

// Get connection string from environment or configuration
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

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
