namespace Tedas.Shuttle.Application.DTOs.RoutePoints;

public sealed record ReorderRoutePointsRequest(IReadOnlyList<Guid> RoutePointIds);

