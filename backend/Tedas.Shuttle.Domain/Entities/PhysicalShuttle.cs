namespace Tedas.Shuttle.Domain.Entities;

public sealed class PhysicalShuttle
{
    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string PlateNumber { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public IReadOnlyCollection<ShuttleShift> Shifts => _shifts.AsReadOnly();

    private readonly List<ShuttleShift> _shifts = [];

    private PhysicalShuttle()
    {
    }

    public PhysicalShuttle(
        string code,
        string plateNumber,
        string? description,
        DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        Code = code;
        PlateNumber = plateNumber;
        Description = description;
        IsActive = true;
        CreatedAt = createdAt;
    }

    public void Update(
        string plateNumber,
        string? description,
        DateTimeOffset updatedAt)
    {
        PlateNumber = plateNumber;
        Description = description;
        UpdatedAt = updatedAt;
    }

    public void SetActiveStatus(bool isActive, DateTimeOffset updatedAt)
    {
        IsActive = isActive;
        UpdatedAt = updatedAt;
    }
}
