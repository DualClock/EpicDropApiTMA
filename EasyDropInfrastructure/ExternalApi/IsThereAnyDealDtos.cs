using System.Text.Json.Serialization;

namespace EasyDropInfrastructure.ExternalApi;

// Корневой ответ от API
public class IsThereAnyDealFreeDealsResponse
{
    [JsonPropertyName("data")]
    public List<FreeDealData> Data { get; set; } = new();
}

// Данные об одной раздаче
public class FreeDealData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("shop")]
    public DealShop Shop { get; set; } = new();

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("drm")]
    public List<DrmInfo> Drm { get; set; } = new();

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}

// Информация о магазине
public class DealShop
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

// Информация о DRM
public class DrmInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}