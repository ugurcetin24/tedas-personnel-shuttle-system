using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Tedas.Shuttle.Infrastructure.Geocoding;

namespace Tedas.Shuttle.Tests;

public sealed class NominatimGeocodingServiceTests
{
    [Fact]
    public async Task SearchAsync_WithValidResponse_ReturnsResults()
    {
        using var httpClient = CreateHttpClient(
            HttpStatusCode.OK,
            """
            [
              {
                "display_name": "Kizilay, Ankara, Turkiye",
                "lat": "39.920770",
                "lon": "32.854110"
              }
            ]
            """);
        var service = new NominatimGeocodingService(
            httpClient,
            NullLogger<NominatimGeocodingService>.Instance);

        var results = await service.SearchAsync("Kizilay Ankara", 5, CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Kizilay, Ankara, Turkiye", results[0].DisplayName);
        Assert.Equal(39.920770m, results[0].Latitude);
        Assert.Equal(32.854110m, results[0].Longitude);
    }

    [Fact]
    public async Task SearchAsync_WithEmptyQuery_ReturnsEmptyList()
    {
        using var httpClient = CreateHttpClient(HttpStatusCode.OK, "[]");
        var service = new NominatimGeocodingService(
            httpClient,
            NullLogger<NominatimGeocodingService>.Instance);

        var results = await service.SearchAsync(" ", 5, CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_WithHttpFailure_ReturnsEmptyList()
    {
        using var httpClient = CreateHttpClient(HttpStatusCode.InternalServerError, "");
        var service = new NominatimGeocodingService(
            httpClient,
            NullLogger<NominatimGeocodingService>.Instance);

        var results = await service.SearchAsync("Kizilay Ankara", 5, CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_WithMalformedJson_ReturnsEmptyList()
    {
        using var httpClient = CreateHttpClient(HttpStatusCode.OK, "{");
        var service = new NominatimGeocodingService(
            httpClient,
            NullLogger<NominatimGeocodingService>.Instance);

        var results = await service.SearchAsync("Kizilay Ankara", 5, CancellationToken.None);

        Assert.Empty(results);
    }

    private static HttpClient CreateHttpClient(HttpStatusCode statusCode, string content)
    {
        return new HttpClient(new StubHttpMessageHandler(statusCode, content))
        {
            BaseAddress = new Uri("https://nominatim.test/")
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
