using EasyDropApplication.Interfaces;
using EasyDropInfrastructure.DataBase;
using EasyDropInfrastructure.ExternalApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyDropAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GiveawayController : ControllerBase
{
    private readonly IGiveawayService _steamService;
    private readonly IEpicGamesService _epicGamesService;
    private readonly AppDbContext _dbContext;

    public GiveawayController(
        IGiveawayService steamService,
        IEpicGamesService epicGamesService,
        AppDbContext dbContext)
    {
        _steamService = steamService;
        _epicGamesService = epicGamesService;
        _dbContext = dbContext;
    }

    #region Steam Endpoints

    [HttpGet("steam/all")]
    public async Task<IActionResult> GetAllSteamGiveaways()
    {
        try
        {
            var games = await _dbContext.GameSteams
                .Where(g => g.IsActive)
                .OrderByDescending(g => g.StartDate)
                .ToListAsync();

            return Ok(games);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "An error occurred while fetching Steam giveaways", Details = ex.Message });
        }
    }

    [HttpPost("steam/fetch")]
    public async Task<IActionResult> FetchAndSaveSteamGiveaways()
    {
        try
        {
            var giveaways = await _steamService.GetActiveGiveawaysAsync();

            if (!giveaways.Any())
            {
                return Ok(new { Message = "No active giveaways found on Steam" });
            }

            var addedCount = 0;
            var updatedCount = 0;

            foreach (var giveaway in giveaways)
            {
                var existingGame = await _dbContext.GameSteams
                    .FirstOrDefaultAsync(g => g.StoreUrl == giveaway.StoreUrl);

                if (existingGame == null)
                {
                    await _dbContext.GameSteams.AddAsync(giveaway);
                    addedCount++;
                }
                else
                {
                    existingGame.Title = giveaway.Title;
                    existingGame.Description = giveaway.Description;
                    existingGame.ImageUrl = giveaway.ImageUrl;
                    existingGame.IsActive = giveaway.IsActive;
                    existingGame.StartDate = giveaway.StartDate;
                    existingGame.EndDate = giveaway.EndDate;
                    updatedCount++;
                }
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                Message = "Successfully processed Steam giveaways",
                Added = addedCount,
                Updated = updatedCount,
                Total = giveaways.Count
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "An error occurred while fetching Steam giveaways", Details = ex.Message });
        }
    }

    #endregion

    #region Epic Games Endpoints

    [HttpGet("epic/all")]
    public async Task<IActionResult> GetAllEpicGames()
    {
        try
        {
            var games = await _dbContext.GameEpics
                .Where(g => g.IsActive)
                .OrderByDescending(g => g.StartDate)
                .ToListAsync();

            return Ok(games);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "An error occurred while fetching Epic Games", Details = ex.Message });
        }
    }

    [HttpPost("epic/fetch")]
    public async Task<IActionResult> FetchAndSaveEpicGames()
    {
        try
        {
            var games = await _epicGamesService.GetFreeGamesAsync();

            if (!games.Any())
            {
                return Ok(new { Message = "No free games found on Epic Games" });
            }

            var addedCount = 0;
            var updatedCount = 0;

            foreach (var game in games)
            {
                var existingGame = await _dbContext.GameEpics
                    .FirstOrDefaultAsync(g => g.StoreUrl == game.StoreUrl);

                if (existingGame == null)
                {
                    await _dbContext.GameEpics.AddAsync(game);
                    addedCount++;
                }
                else
                {
                    existingGame.Title = game.Title;
                    existingGame.Description = game.Description;
                    existingGame.ImageUrl = game.ImageUrl;
                    existingGame.IsActive = game.IsActive;
                    existingGame.StartDate = game.StartDate;
                    existingGame.EndDate = game.EndDate;
                    updatedCount++;
                }
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                Message = "Successfully processed Epic Games",
                Added = addedCount,
                Updated = updatedCount,
                Total = games.Count
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "An error occurred while fetching Epic Games", Details = ex.Message });
        }
    }

    #endregion

    #region Combined Endpoints

    [HttpGet("all")]
    public async Task<IActionResult> GetAllGiveaways()
    {
        try
        {
            var steamGames = await _dbContext.GameSteams
                .Where(g => g.IsActive)
                .Select(g => new
                {
                    Store = "Steam",
                    g.Title,
                    g.Description,
                    g.ImageUrl,
                    g.StoreUrl,
                    g.StartDate,
                    g.EndDate
                })
                .ToListAsync();

            var epicGames = await _dbContext.GameEpics
                .Where(g => g.IsActive)
                .Select(g => new
                {
                    Store = "Epic Games",
                    g.Title,
                    g.Description,
                    g.ImageUrl,
                    g.StoreUrl,
                    g.StartDate,
                    g.EndDate
                })
                .ToListAsync();

            var allGames = steamGames
                .Concat(epicGames)
                .OrderByDescending(g => g.StartDate)
                .ToList();

            return Ok(allGames);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "An error occurred while fetching all giveaways", Details = ex.Message });
        }
    }

    [HttpGet("active/count")]
    public async Task<IActionResult> GetActiveGiveawaysCount()
    {
        try
        {
            var steamCount = await _dbContext.GameSteams.CountAsync(g => g.IsActive);
            var epicCount = await _dbContext.GameEpics.CountAsync(g => g.IsActive);

            return Ok(new
            {
                SteamCount = steamCount,
                EpicCount = epicCount,
                TotalCount = steamCount + epicCount
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "An error occurred while counting giveaways", Details = ex.Message });
        }
    }

    #endregion
}