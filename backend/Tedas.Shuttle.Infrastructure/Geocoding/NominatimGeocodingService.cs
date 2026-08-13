using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Tedas.Shuttle.Application.DTOs.Geocoding;
using Tedas.Shuttle.Application.Interfaces;

namespace Tedas.Shuttle.Infrastructure.Geocoding;

public sealed class NominatimGeocodingService(
    HttpClient httpClient,
    ILogger<NominatimGeocodingService> logger)
    : IGeocodingService
{
    public async Task<IReadOnlyList<GeocodingResultDto>> SearchAsync(
        string query,
        int limit,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var safeLimit = Math.Clamp(limit, 1, 10);
        var requestUri = $"search?format=jsonv2&addressdetails=0&q={Uri.EscapeDataString(query.Trim())}&limit={safeLimit}";

        try
        {
            var results = await httpClient.GetFromJsonAsync<NominatimSearchResult[]>(
                requestUri,
                cancellationToken);

            if (results is null)
            {
                return [];
            }

            return results
                .Select(Map)
                .Where(result => result is not null)
                .Cast<GeocodingResultDto>()
                .ToArray();
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Nominatim request failed for query {Query}.", query);
            return [];
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Nominatim request timed out for query {Query}.", query);
            return [];
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Nominatim returned malformed JSON for query {Query}.", query);
            return [];
        }
    }

    private static GeocodingResultDto? Map(NominatimSearchResult result)
    {
        if (string.IsNullOrWhiteSpace(result.DisplayName)
            || !decimal.TryParse(result.Latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude)
            || !decimal.TryParse(result.Longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
        {
            return null;
        }

        return new GeocodingResultDto(result.DisplayName, latitude, longitude);
    }

    private sealed record NominatimSearchResult(
        [property: JsonPropertyName("display_name")] string? DisplayName,
        [property: JsonPropertyName("lat")] string? Latitude,
        [property: JsonPropertyName("lon")] string? Longitude);
}
