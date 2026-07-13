using System.Text.Json;
using EasyDropApplication.Interfaces;
using EasyDropDomain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyDropInfrastructure.ExternalApi;

public class IsThereAnyDealService : IGiveawayService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IsThereAnyDealService> _logger;
    private readonly string _apiKey;

    public IsThereAnyDealService(
        HttpClient httpClient,
        ILogger<IsThereAnyDealService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["ApiSettings:IsThereAnyDealApiKey"]
            ?? throw new InvalidOperationException("API key not configured");
    }

    public async Task<List<GameSteam>> GetActiveGiveawaysAsync()
    {
        var games = new List<GameSteam>();

        try
        {
            // Запрашиваем бесплатные раздачи из Steam
            // Эндпоинт: /v02/deals/free/?key=API_KEY&shops=steam&limit=100
            var requestUrl = $"v02/deals/free/?key={_apiKey}&shops=steam&limit=100";

            _logger.LogInformation("Fetching giveaways from IsThereAnyDeal: {Url}", requestUrl);

            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var dealsResponse = JsonSerializer.Deserialize<IsThereAnyDealFreeDealsResponse>(json, options);

            if (dealsResponse?.Data == null)
            {
                _logger.LogWarning("No data received from IsThereAnyDeal API");
                return games;
            }

            foreach (var deal in dealsResponse.Data)
            {
                // Проверяем, что это действительно Steam игра
                if (deal.Shop.Name.Equals("Steam", StringComparison.OrdinalIgnoreCase))
                {
                    var game = new GameSteam
                    {
                        Title = deal.Title,
                        Description = $"Free giveaway on Steam. ID: {deal.Id}",
                        ImageUrl = null, // IsThereAnyDeal не всегда отдает картинки
                        StoreUrl = deal.Url,
                        IsActive = true,
                        StartDate = DateTimeOffset.FromUnixTimeSeconds(deal.Timestamp).DateTime,
                        EndDate = null // API не всегда отдает дату окончания
                    };

                    games.Add(game);
                }
            }

            _logger.LogInformation("Fetched {Count} active giveaways from Steam", games.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching giveaways from IsThereAnyDeal");
        }

        return games;
    }
}