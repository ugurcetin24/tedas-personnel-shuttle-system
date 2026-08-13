namespace Tedas.Shuttle.Domain.Entities;

public sealed class Driver
{
    public Guid Id { get; private set; }

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string? Phone { get; private set; }

    public string LicenseNumber { get; private set; } = string.Empty;

    public Guid? ShuttleShiftId { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public ShuttleShift? ShuttleShift { get; private set; }

    private Driver()
    {
    }

    public Driver(
        string firstName,
        string lastName,
        string? phone,
        string licenseNumber,
        DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        LicenseNumber = licenseNumber;
        IsActive = true;
        CreatedAt = createdAt;
    }

    public void Update(
        string firstName,
        string lastName,
        string? phone,
        string licenseNumber,
        DateTimeOffset updatedAt)
    {
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        LicenseNumber = licenseNumber;
        UpdatedAt = updatedAt;
    }

    public void SetActiveStatus(bool isActive, DateTimeOffset updatedAt)
    {
        IsActive = isActive;
        UpdatedAt = updatedAt;
    }

    public void AssignToShift(Guid shuttleShiftId, DateTimeOffset updatedAt)
    {
        ShuttleShiftId = shuttleShiftId;
        UpdatedAt = updatedAt;
    }

    public void ClearShiftAssignment(DateTimeOffset updatedAt)
    {
        ShuttleShiftId = null;
        UpdatedAt = updatedAt;
    }
}

