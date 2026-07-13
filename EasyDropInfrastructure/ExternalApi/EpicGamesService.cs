using System.Text.Json;
using EasyDropDomain.Models;

namespace EasyDropInfrastructure.ExternalApi;

public interface IEpicGamesService
{
    Task<List<GameEpic>> GetFreeGamesAsync();
}

public class EpicGamesService : IEpicGamesService
{
    private readonly HttpClient _httpClient;

    public EpicGamesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<GameEpic>> GetFreeGamesAsync()
    {
        var games = new List<GameEpic>();

        try
        {
            // Используем правильный REST API эндпоинт
            var url = "https://store-site-backend-static-ipv4.ak.epicgames.com/freeGamesPromotions?locale=ru-RU&country=RU&allowCountries=RU";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var epicResponse = JsonSerializer.Deserialize<EpicFreeGamesResponse>(json);

            if (epicResponse?.Data?.Catalog?.SearchStore?.Elements == null)
                return games;

            foreach (var element in epicResponse.Data.Catalog.SearchStore.Elements)
            {
                // Проверяем, что игра бесплатная
                if (element.Price?.TotalPrice?.DiscountPrice == 0 && element.Price.TotalPrice.OriginalPrice > 0)
                {
                    var promotions = element.Promotions?.PromotionalOffers?
                        .SelectMany(p => p.PromotionalOffers)
                        .ToList();

                    if (promotions != null && promotions.Any())
                    {
                        var promo = promotions.First();
                        var imageUrl = element.KeyImages?.FirstOrDefault(img => img.Type == "Thumbnail")?.Url;

                        games.Add(new GameEpic
                        {
                            Title = element.Title,
                            Description = element.Description,
                            ImageUrl = imageUrl,
                            StoreUrl = element.Url,
                            IsActive = true,
                            StartDate = promo.StartDate,
                            EndDate = promo.EndDate
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        return games;
    }
}