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

    public static readonly IReadOnlyList<ExcelColumnMappingProfile> ShuttleCapacity =
    [
        new("PhysicalShuttleCode", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Servis",
            "Servis Kodu",
            "Servis No",
            "Shuttle Code"
        }),
        new("ShiftName", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Vardiya",
            "Vardiya Adi",
            "Shift",
            "Shift Name"
        }),
        new("Capacity", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Kapasite",
            "Kontenjan",
            "Capacity"
        }),
        new("ShiftType", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Vardiya Tipi",
            "Tip",
            "Shift Type"
        }),
        new("StartTime", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Baslangic",
            "Baslangic Saati",
            "Start Time"
        }),
        new("EndTime", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Bitis",
            "Bitis Saati",
            "End Time"
        })
    ];
}
