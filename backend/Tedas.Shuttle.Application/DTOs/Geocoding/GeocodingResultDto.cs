namespace Tedas.Shuttle.Application.DTOs.Geocoding;

public sealed record GeocodingResultDto(
    string DisplayName,
    decimal Latitude,
    decimal Longitude);

