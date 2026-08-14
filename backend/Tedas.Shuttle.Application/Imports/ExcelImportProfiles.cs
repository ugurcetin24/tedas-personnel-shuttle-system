namespace Tedas.Shuttle.Application.Imports;

public static class ExcelImportProfiles
{
    public static readonly IReadOnlyList<ExcelColumnMappingProfile> Personnel =
    [
        new("RegistrationNumber", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Sicil",
            "Sicil No",
            "Sicil Numarasi",
            "Personel No",
            "Registration Number"
        }),
        new("FirstName", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Ad",
            "Adi",
            "First Name"
        }),
        new("LastName", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Soyad",
            "Soyadi",
            "Last Name"
        }),
        new("Department", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Departman",
            "Birim",
            "Unit"
        }),
        new("Title", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Unvan",
            "Gorev",
            "Title"
        }),
        new("Phone", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Telefon",
            "Cep Telefonu",
            "Phone"
        }),
        new("Email", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Eposta",
            "E-posta",
            "Email"
        }),
        new("Address", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Adres",
            "Address"
        }),
        new("Latitude", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Latitude",
            "Enlem",
            "Lat"
        }),
        new("Longitude", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Longitude",
            "Boylam",
            "Lon",
            "Lng"
        })
    ];
}
