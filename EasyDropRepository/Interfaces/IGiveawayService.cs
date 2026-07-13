using EasyDropDomain.Models;

namespace EasyDropApplication.Interfaces;

public interface IGiveawayService
{
    Task<List<GameSteam>> GetActiveGiveawaysAsync();
}