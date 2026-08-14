using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Tedas.Shuttle.Application.DTOs.Routing;
using Tedas.Shuttle.Application.Interfaces;

namespace Tedas.Shuttle.Infrastructure.Routing;

public sealed class OsrmRoutingService(
    HttpClient httpClient,
    ILogger<OsrmRoutingService> logger)
    : IRoutingService
{
    public async Task<CalculatedRouteDto?> CalculateAsync(
        IReadOnlyList<RouteCoordinateDto> coordinates,
        CancellationToken cancellationToken)
    {
        if (coordinates.Count < 2)
        {
            return null;
        }

        var path = string.Join(
            ';',
            coordinates.Select(coordinate =>
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"{coordinate.Longitude},{coordinate.Latitude}")));
        var requestUri = $"route/v1/driving/{path}?overview=full&geometries=geojson";

        try
        {
            var response = await httpClient.GetFromJsonAsync<OsrmRouteResponse>(
                requestUri,
                cancellationToken);
            var route = response?.Routes?.FirstOrDefault();
            var geometry = route?.Geometry;

            if (route is null || geometry?.Coordinates is null || geometry.Coordinates.Length == 0)
            {
                return null;
            }

            var mappedCoordinates = geometry.Coordinates
                .Where(item => item.Length >= 2)
                .Select(item => new RouteCoordinateDto(
                    Convert.ToDecimal(item[1], CultureInfo.InvariantCulture),
                    Convert.ToDecimal(item[0], CultureInfo.InvariantCulture)))
                .ToArray();

            if (mappedCoordinates.Length == 0)
            {
                return null;
            }

            return new CalculatedRouteDto(
                route.Distance,
                route.Duration,
                JsonSerializer.Serialize(geometry),
                mappedCoordinates);
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "OSRM route calculation failed.");
            return null;
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "OSRM route calculation timed out.");
            return null;
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "OSRM returned malformed JSON.");
            return null;
        }
    }

    private sealed record OsrmRouteResponse(
        [property: JsonPropertyName("routes")] OsrmRoute[]? Routes);

    private sealed record OsrmRoute(
        [property: JsonPropertyName("distance")] double Distance,
        [property: JsonPropertyName("duration")] double Duration,
        [property: JsonPropertyName("geometry")] OsrmGeometry? Geometry);

    private sealed record OsrmGeometry(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("coordinates")] double[][] Coordinates);
}

