using EasyDropApplication.Interfaces;
using EasyDropInfrastructure.DataBase;
using EasyDropInfrastructure.ExternalApi;
using Microsoft.EntityFrameworkCore;
using EasyDropAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. НАСТРОЙКА CORS (Разрешаем все источники для простоты деплоя)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 2. НАСТРОЙКА ПОДКЛЮЧЕНИЯ К БАЗЕ ДАННЫХ (Специально для Railway)
var pgHost = Environment.GetEnvironmentVariable("PGHOST");
var pgPort = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE");
var pgUser = Environment.GetEnvironmentVariable("PGUSER");
var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");

string connectionString;

if (!string.IsNullOrEmpty(pgHost) && !string.IsNullOrEmpty(pgDatabase))
{
    // Собираем строку в формате, который идеально понимает Npgsql
    connectionString = $"Host={pgHost};Port={pgPort};Database={pgDatabase};Username={pgUser};Password={pgPassword};SSL Mode=Require;Trust Server Certificate=true";
    Console.WriteLine("✅ SUCCESS: Built connection string from Railway PG* variables.");
}
else
{
    // Фоллбэк для локальной разработки
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine("⚠️ WARNING: PG* variables not found. Falling back to appsettings.json.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// 3. HTTP КЛИЕНТЫ
builder.Services.AddHttpClient<IGiveawayService, IsThereAnyDealService>(client =>
{
    client.BaseAddress = new Uri("https://api.isthereanydeal.com/");
});

builder.Services.AddHttpClient<IEpicGamesService, EpicGamesService>();
builder.Services.AddHostedService<GiveawayBackgroundService>();
var app = builder.Build();

// 4. АВТОМАТИЧЕСКОЕ ПРИМЕНЕНИЕ МИГРАЦИЙ (Строго ПОСЛЕ builder.Build() и ДО app.Run())
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        Console.WriteLine("Applying migrations...");
        context.Database.Migrate();
        Console.WriteLine("✅ Migrations applied successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Migration error: {ex.Message}");
        throw; // Останавливаем приложение, если база не подключилась
    }
}

// 5. MIDDLEWARE PIPELINE
// Включаем Swagger везде (и для Production на Railway)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EasyDrop API V1");
    c.RoutePrefix = string.Empty; // Swagger открывается по корню сайта
});

// CORS должен быть ДО UseAuthorization и UseEndpoints/MapControllers
app.UseCors("AllowAll");

// app.UseHttpsRedirection(); // Отключено для Railway

app.UseAuthorization();
app.MapControllers();

app.Run();