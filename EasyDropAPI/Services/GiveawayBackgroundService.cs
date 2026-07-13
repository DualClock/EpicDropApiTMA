using EasyDropApplication.Interfaces;
using EasyDropDomain.Models;
using EasyDropInfrastructure.DataBase;
using EasyDropInfrastructure.ExternalApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EasyDropAPI.Services;

public class GiveawayBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GiveawayBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(30); // Каждые 30 минут

    public GiveawayBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<GiveawayBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🎮 GiveawayBackgroundService started. First fetch will run immediately.");

        // Запускаем первый раз сразу при старте
        await FetchAllGiveaways(stoppingToken);

        // Затем каждые 30 минут
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("⏰ Waiting 30 minutes before next fetch...");
            await Task.Delay(_interval, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
            {
                await FetchAllGiveaways(stoppingToken);
            }
        }

        _logger.LogInformation("👋 GiveawayBackgroundService stopped.");
    }

    private async Task FetchAllGiveaways(CancellationToken stoppingToken)
    {
        _logger.LogInformation(" Starting giveaway fetch at {Time}", DateTime.UtcNow);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var epicService = scope.ServiceProvider.GetRequiredService<IEpicGamesService>();
            var steamService = scope.ServiceProvider.GetRequiredService<IGiveawayService>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // 1. Получаем игры из Epic Games
            _logger.LogInformation("📦 Fetching Epic Games giveaways...");
            var epicGames = await epicService.GetFreeGamesAsync();

            if (epicGames != null && epicGames.Any())
            {
                foreach (var game in epicGames)
                {
                    var existingGame = await context.GameEpics
                        .FirstOrDefaultAsync(g => g.Title == game.Title, stoppingToken);

                    if (existingGame != null)
                    {
                        // Обновляем существующую игру
                        existingGame.Description = game.Description;
                        existingGame.ImageUrl = game.ImageUrl;
                        existingGame.StoreUrl = game.StoreUrl;
                        existingGame.StartDate = game.StartDate;
                        existingGame.EndDate = game.EndDate;
                        existingGame.IsActive = true;
                        _logger.LogInformation("✅ Updated Epic game: {Title}", game.Title);
                    }
                    else
                    {
                        // Добавляем новую игру
                        await context.GameEpics.AddAsync(new GameEpic
                        {
                            Title = game.Title,
                            Description = game.Description,
                            ImageUrl = game.ImageUrl,
                            StoreUrl = game.StoreUrl,
                            StartDate = game.StartDate,
                            EndDate = game.EndDate,
                            IsActive = true
                        }, stoppingToken);
                        _logger.LogInformation("➕ Added new Epic game: {Title}", game.Title);
                    }
                }

                await context.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("💾 Saved {Count} Epic games to database", epicGames.Count());
            }
            else
            {
                _logger.LogInformation("ℹ️ No Epic Games giveaways found");
            }

            // 2. Получаем игры из Steam (IsThereAnyDeal)
            _logger.LogInformation("📦 Fetching Steam giveaways...");
            var steamGames = await steamService.GetFreeGamesAsync();    

            if (steamGames != null && steamGames.Any())
            {
                foreach (var game in steamGames)
                {
                    var existingGame = await context.GameSteams
                        .FirstOrDefaultAsync(g => g.Title == game.Title, stoppingToken);

                    if (existingGame != null)
                    {
                        // Обновляем существующую игру
                        existingGame.Description = game.Description;
                        existingGame.ImageUrl = game.ImageUrl;
                        existingGame.StoreUrl = game.StoreUrl;
                        existingGame.StartDate = game.StartDate;
                        existingGame.EndDate = game.EndDate;
                        existingGame.IsActive = true;
                        _logger.LogInformation("✅ Updated Steam game: {Title}", game.Title);
                    }
                    else
                    {
                        // Добавляем новую игру
                        await context.GameSteams.AddAsync(new GameSteam
                        {
                            Title = game.Title,
                            Description = game.Description,
                            ImageUrl = game.ImageUrl,
                            StoreUrl = game.StoreUrl,
                            StartDate = game.StartDate,
                            EndDate = game.EndDate,
                            IsActive = true
                        }, stoppingToken);
                        _logger.LogInformation("➕ Added new Steam game: {Title}", game.Title);
                    }
                }

                await context.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("💾 Saved {Count} Steam games to database", steamGames.Count());
            }
            else
            {
                _logger.LogInformation("ℹ️ No Steam giveaways found");
            }

            _logger.LogInformation("✨ Giveaway fetch completed successfully at {Time}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during giveaway fetch at {Time}", DateTime.UtcNow);
        }
    }
}