namespace Tedas.Shuttle.Domain.Entities;

public sealed class Personnel
{
    public Guid Id { get; private set; }

    public string RegistrationNumber { get; private set; } = string.Empty;

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string? Department { get; private set; }

    public string? Title { get; private set; }

    public string? Phone { get; private set; }

    public string? Email { get; private set; }

    public string? Address { get; private set; }

    public decimal? Latitude { get; private set; }

    public decimal? Longitude { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    private Personnel()
    {
    }

    public Personnel(
        string registrationNumber,
        string firstName,
        string lastName,
        string? department,
        string? title,
        string? phone,
        string? email,
        string? address,
        decimal? latitude,
        decimal? longitude,
        DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        RegistrationNumber = registrationNumber;
        FirstName = firstName;
        LastName = lastName;
        Department = department;
        Title = title;
        Phone = phone;
        Email = email;
        Address = address;
        Latitude = latitude;
        Longitude = longitude;
        IsActive = true;
        CreatedAt = createdAt;
    }

    public void Update(
        string firstName,
        string lastName,
        string? department,
        string? title,
        string? phone,
        string? email,
        string? address,
        decimal? latitude,
        decimal? longitude,
        DateTimeOffset updatedAt)
    {
        FirstName = firstName;
        LastName = lastName;
        Department = department;
        Title = title;
        Phone = phone;
        Email = email;
        Address = address;
        Latitude = latitude;
        Longitude = longitude;
        UpdatedAt = updatedAt;
    }

    public void SetActiveStatus(bool isActive, DateTimeOffset updatedAt)
    {
        IsActive = isActive;
        UpdatedAt = updatedAt;
    }
}
