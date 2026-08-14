namespace Tedas.Shuttle.Domain.Entities;

public sealed class SavedRoute
{
    public Guid Id { get; private set; }

    public Guid ShuttleShiftId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public double DistanceMeters { get; private set; }

    public double DurationSeconds { get; private set; }

    public string Geometry { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public ShuttleShift? ShuttleShift { get; private set; }

    private SavedRoute()
    {
    }

    public SavedRoute(
        Guid shuttleShiftId,
        string name,
        double distanceMeters,
        double durationSeconds,
        string geometry,
        DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        ShuttleShiftId = shuttleShiftId;
        Name = name;
        DistanceMeters = distanceMeters;
        DurationSeconds = durationSeconds;
        Geometry = geometry;
        CreatedAt = createdAt;
    }
}

