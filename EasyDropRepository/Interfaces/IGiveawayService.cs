using EasyDropDomain.Models;

namespace EasyDropApplication.Interfaces;

public interface IGiveawayService
{
    Task<IEnumerable<GameSteam>> GetFreeGamesAsync();
}