using EasyDropInfrastructure.DataBase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyDropAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GiveawayController : ControllerBase
{
    private readonly AppDbContext _context;

    public GiveawayController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllGiveaways()
    {
        var steamGames = await _context.GameSteams
            .Where(g => g.IsActive)
            .Select(g => new { Store = "Steam", g.Title, g.Description, g.ImageUrl, g.StoreUrl, g.StartDate, g.EndDate })
            .ToListAsync();

        var epicGames = await _context.GameEpics
            .Where(g => g.IsActive)
            .Select(g => new { Store = "Epic Games", g.Title, g.Description, g.ImageUrl, g.StoreUrl, g.StartDate, g.EndDate })
            .ToListAsync();

        return Ok(steamGames.Concat(epicGames).OrderBy(g => g.EndDate).ToList());
    }

    [HttpGet("steam/all")]
    public async Task<IActionResult> GetSteamGiveaways()
    {
        var games = await _context.GameSteams
            .Where(g => g.IsActive)
            .Select(g => new { Store = "Steam", g.Title, g.Description, g.ImageUrl, g.StoreUrl, g.StartDate, g.EndDate })
            .OrderBy(g => g.EndDate)
            .ToListAsync();
        return Ok(games);
    }

    [HttpGet("epic/all")]
    public async Task<IActionResult> GetEpicGiveaways()
    {
        var games = await _context.GameEpics
            .Where(g => g.IsActive)
            .Select(g => new { Store = "Epic Games", g.Title, g.Description, g.ImageUrl, g.StoreUrl, g.StartDate, g.EndDate })
            .OrderBy(g => g.EndDate)
            .ToListAsync();
        return Ok(games);
    }
}
