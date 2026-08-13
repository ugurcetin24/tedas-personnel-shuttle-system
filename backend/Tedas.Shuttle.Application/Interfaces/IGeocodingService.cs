using Tedas.Shuttle.Application.DTOs.Geocoding;

namespace Tedas.Shuttle.Application.Interfaces;

public interface IGeocodingService
{
    Task<IReadOnlyList<GeocodingResultDto>> SearchAsync(
        string query,
        int limit,
        CancellationToken cancellationToken);
}

