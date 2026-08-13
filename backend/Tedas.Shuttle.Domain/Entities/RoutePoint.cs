namespace Tedas.Shuttle.Domain.Entities;

public sealed class RoutePoint
{
    public Guid Id { get; private set; }

    public Guid ShuttleShiftId { get; private set; }

    public int Order { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Address { get; private set; }

    public decimal Latitude { get; private set; }

    public decimal Longitude { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public ShuttleShift? ShuttleShift { get; private set; }

    private RoutePoint()
    {
    }

    public RoutePoint(
        Guid shuttleShiftId,
        int order,
        string name,
        string? address,
        decimal latitude,
        decimal longitude,
        DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        ShuttleShiftId = shuttleShiftId;
        Order = order;
        Name = name;
        Address = address;
        Latitude = latitude;
        Longitude = longitude;
        IsActive = true;
        CreatedAt = createdAt;
    }

    public void Update(
        string name,
        string? address,
        decimal latitude,
        decimal longitude,
        DateTimeOffset updatedAt)
    {
        Name = name;
        Address = address;
        Latitude = latitude;
        Longitude = longitude;
        UpdatedAt = updatedAt;
    }

    public void SetOrder(int order, DateTimeOffset updatedAt)
    {
        Order = order;
        UpdatedAt = updatedAt;
    }

    public void SetActiveStatus(bool isActive, DateTimeOffset updatedAt)
    {
        IsActive = isActive;
        UpdatedAt = updatedAt;
    }
}

