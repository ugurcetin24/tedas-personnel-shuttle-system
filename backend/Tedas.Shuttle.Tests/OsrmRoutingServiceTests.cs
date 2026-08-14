using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Tedas.Shuttle.Application.DTOs.Routing;
using Tedas.Shuttle.Infrastructure.Routing;

namespace Tedas.Shuttle.Tests;

public sealed class OsrmRoutingServiceTests
{
    [Fact]
    public async Task CalculateAsync_WithValidResponse_ReturnsRoute()
    {
        using var httpClient = CreateHttpClient(
            HttpStatusCode.OK,
            """
            {
              "routes": [
                {
                  "distance": 1234.5,
                  "duration": 678.9,
                  "geometry": {
                    "type": "LineString",
                    "coordinates": [[32.85, 39.92], [32.86, 39.93]]
                  }
                }
              ]
            }
            """);
        var service = new OsrmRoutingService(httpClient, NullLogger<OsrmRoutingService>.Instance);

        var route = await service.CalculateAsync(
            [
                new RouteCoordinateDto(39.92m, 32.85m),
                new RouteCoordinateDto(39.93m, 32.86m)
            ],
            CancellationToken.None);

        Assert.NotNull(route);
        Assert.Equal(1234.5, route.DistanceMeters);
        Assert.Equal(678.9, route.DurationSeconds);
        Assert.Equal(39.92m, route.Coordinates[0].Latitude);
        Assert.Equal(32.85m, route.Coordinates[0].Longitude);
        Assert.Contains("LineString", route.Geometry);
    }

    [Fact]
    public async Task CalculateAsync_WithHttpFailure_ReturnsNull()
    {
        using var httpClient = CreateHttpClient(HttpStatusCode.InternalServerError, "");
        var service = new OsrmRoutingService(httpClient, NullLogger<OsrmRoutingService>.Instance);

        var route = await service.CalculateAsync(
            [
                new RouteCoordinateDto(39.92m, 32.85m),
                new RouteCoordinateDto(39.93m, 32.86m)
            ],
            CancellationToken.None);

        Assert.Null(route);
    }

    [Fact]
    public async Task CalculateAsync_WithMalformedJson_ReturnsNull()
    {
        using var httpClient = CreateHttpClient(HttpStatusCode.OK, "{");
        var service = new OsrmRoutingService(httpClient, NullLogger<OsrmRoutingService>.Instance);

        var route = await service.CalculateAsync(
            [
                new RouteCoordinateDto(39.92m, 32.85m),
                new RouteCoordinateDto(39.93m, 32.86m)
            ],
            CancellationToken.None);

        Assert.Null(route);
    }

    private static HttpClient CreateHttpClient(HttpStatusCode statusCode, string content)
    {
        return new HttpClient(new StubHttpMessageHandler(statusCode, content))
        {
            BaseAddress = new Uri("https://osrm.test/")
        };
    }

    private sealed class StubHttpMessageHandler(HttpStatusCode statusCode, string content) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content)
            });
        }
    }
}
