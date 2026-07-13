using System.Text.Json.Serialization;

namespace EasyDropInfrastructure.ExternalApi;

public class EpicFreeGamesResponse
{
    [JsonPropertyName("data")]
    public EpicData Data { get; set; } = new();
}

public class EpicData
{
    [JsonPropertyName("Catalog")]
    public Catalog Catalog { get; set; } = new();
}

public class Catalog
{
    [JsonPropertyName("searchStore")]
    public SearchStore SearchStore { get; set; } = new();
}

public class SearchStore
{
    [JsonPropertyName("elements")]
    public List<EpicGameElement> Elements { get; set; } = new();
}

public class EpicGameElement
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("keyImages")]
    public List<KeyImage> KeyImages { get; set; } = new();

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("promotions")]
    public Promotions? Promotions { get; set; }

    [JsonPropertyName("price")]
    public Price? Price { get; set; }
}

public class KeyImage
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

public class Price
{
    [JsonPropertyName("totalPrice")]
    public TotalPrice TotalPrice { get; set; } = new();
}

public class TotalPrice
{
    [JsonPropertyName("discountPrice")]
    public int DiscountPrice { get; set; }

    [JsonPropertyName("originalPrice")]
    public int OriginalPrice { get; set; }
}

public class Promotions
{
    [JsonPropertyName("promotionalOffers")]
    public List<PromotionalOffer> PromotionalOffers { get; set; } = new();
}

public class PromotionalOffer
{
    [JsonPropertyName("promotionalOffers")]
    public List<Discount> PromotionalOffers { get; set; } = new();
}

public class Discount
{
    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }
}