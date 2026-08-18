using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tedas.Shuttle.Domain.Entities;
using Tedas.Shuttle.Domain.Enums;

namespace Tedas.Shuttle.Infrastructure.Persistence;

public static class DemoDataSeeder
{
    public static void Seed(AppDbContext dbContext, ILogger logger)
    {
        if (HasOperationalData(dbContext))
        {
            logger.LogInformation("Demo data seed skipped because operational data already exists.");
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var createdAt = now.AddDays(-21);

        var personnel = CreatePersonnel(createdAt);
        var shuttles = CreateShuttles(createdAt);
        var shifts = CreateShifts(shuttles, createdAt);
        var routePoints = CreateRoutePoints(shifts, createdAt);
        var drivers = CreateDrivers(shifts, createdAt);
        var assignments = CreateAssignments(personnel, shifts, routePoints, createdAt);
        var savedRoutes = CreateSavedRoutes(shifts, routePoints, createdAt);

        dbContext.Personnel.AddRange(personnel);
        dbContext.PhysicalShuttles.AddRange(shuttles);
        dbContext.ShuttleShifts.AddRange(shifts);
        dbContext.RoutePoints.AddRange(routePoints);
        dbContext.Drivers.AddRange(drivers);
        dbContext.PersonnelAssignments.AddRange(assignments);
        dbContext.SavedRoutes.AddRange(savedRoutes);
        dbContext.SaveChanges();

        logger.LogInformation(
            "Demo data seed completed. Personnel: {PersonnelCount}, Shuttles: {ShuttleCount}, Shifts: {ShiftCount}, Drivers: {DriverCount}, Assignments: {AssignmentCount}, RoutePoints: {RoutePointCount}, SavedRoutes: {SavedRouteCount}.",
            personnel.Length,
            shuttles.Length,
            shifts.Length,
            drivers.Length,
            assignments.Length,
            routePoints.Length,
            savedRoutes.Length);
    }

    private static bool HasOperationalData(AppDbContext dbContext)
    {
        return dbContext.Personnel.AsNoTracking().Any()
            || dbContext.PhysicalShuttles.AsNoTracking().Any()
            || dbContext.ShuttleShifts.AsNoTracking().Any()
            || dbContext.Drivers.AsNoTracking().Any()
            || dbContext.PersonnelAssignments.AsNoTracking().Any()
            || dbContext.RoutePoints.AsNoTracking().Any()
            || dbContext.SavedRoutes.AsNoTracking().Any();
    }

    private static Personnel[] CreatePersonnel(DateTimeOffset createdAt)
    {
        return
        [
            new("P-1001", "Ayse", "Demir", "Insan Kaynaklari", "Uzman", "0532 100 10 01", "ayse.demir@example.local", "Kizilay, Cankaya/Ankara", 39.920770m, 32.854110m, createdAt),
            new("P-1002", "Mehmet", "Kaya", "Bilgi Teknolojileri", "Kidemli Uzman", "0532 100 10 02", "mehmet.kaya@example.local", "Sogutozu, Cankaya/Ankara", 39.913000m, 32.785000m, createdAt),
            new("P-1003", "Elif", "Sahin", "Finans", "Analist", "0532 100 10 03", "elif.sahin@example.local", "Bahcelievler, Ankara", 39.926500m, 32.831900m, createdAt),
            new("P-1004", "Can", "Yildiz", "Operasyon", "Koordinator", "0532 100 10 04", "can.yildiz@example.local", "Emek, Ankara", 39.920300m, 32.811100m, createdAt),
            new("P-1005", "Zeynep", "Acar", "Satinalma", "Uzman", "0532 100 10 05", "zeynep.acar@example.local", "Ayranci, Ankara", 39.901800m, 32.857600m, createdAt),
            new("P-1006", "Burak", "Arslan", "Hukuk", "Avukat", "0532 100 10 06", "burak.arslan@example.local", "Cebeci, Ankara", 39.932800m, 32.878600m, createdAt),
            new("P-1007", "Derya", "Koc", "Planlama", "Uzman", "0532 100 10 07", "derya.koc@example.local", "Balgat, Ankara", 39.905800m, 32.810100m, createdAt),
            new("P-1008", "Okan", "Eren", "Bakim", "Tekniker", "0532 100 10 08", "okan.eren@example.local", "Kecioren, Ankara", 39.977100m, 32.866300m, createdAt),
            new("P-1009", "Selin", "Tas", "Musteri Hizmetleri", "Temsilci", "0532 100 10 09", "selin.tas@example.local", "Mamak, Ankara", 39.941900m, 32.916500m, createdAt),
            new("P-1010", "Murat", "Aksoy", "Saha Operasyon", "Teknisyen", "0532 100 10 10", "murat.aksoy@example.local", "Etimesgut, Ankara", 39.948600m, 32.669700m, createdAt),
            new("P-1011", "Nazli", "Polat", "Kalite", "Uzman", "0532 100 10 11", "nazli.polat@example.local", "Umitkoy, Ankara", 39.889900m, 32.684200m, createdAt),
            new("P-1012", "Emre", "Celik", "Depo", "Sorumlu", "0532 100 10 12", "emre.celik@example.local", "Sincan, Ankara", 39.966700m, 32.584400m, createdAt),
            new("P-1013", "Gizem", "Kurt", "Egitim", "Uzman", "0532 100 10 13", "gizem.kurt@example.local", "Batkent, Ankara", 39.971400m, 32.731100m, createdAt),
            new("P-1014", "Hakan", "Gunes", "Proje", "Muhendis", "0532 100 10 14", "hakan.gunes@example.local", "Dikmen, Ankara", 39.880600m, 32.846900m, createdAt),
            new("P-1015", "Irem", "Ozturk", "Muhasebe", "Uzman Yardimcisi", "0532 100 10 15", "irem.ozturk@example.local", "Oran, Ankara", 39.847100m, 32.840700m, createdAt),
            new("P-1016", "Kerem", "Uslu", "Guvenlik", "Vardiya Amiri", "0532 100 10 16", "kerem.uslu@example.local", "Koru, Ankara", 39.884700m, 32.664900m, createdAt),
            new("P-1017", "Melis", "Kara", "Arsiv", "Memur", "0532 100 10 17", "melis.kara@example.local", "Yenimahalle, Ankara", 39.971900m, 32.810600m, createdAt),
            new("P-1018", "Onur", "Sari", "Cagri Merkezi", "Temsilci", "0532 100 10 18", "onur.sari@example.local", "Pursaklar, Ankara", 40.039700m, 32.895700m, createdAt),
            new("P-1019", "Pinar", "Kaplan", "Insan Kaynaklari", "Uzman Yardimcisi", "0532 100 10 19", "pinar.kaplan@example.local", "Gazi Mahallesi, Ankara", 39.940300m, 32.802300m, createdAt),
            new("P-1020", "Serkan", "Yalcin", "Bilgi Teknolojileri", "Sistem Uzmani", "0532 100 10 20", "serkan.yalcin@example.local", "Cayyolu, Ankara", 39.868900m, 32.651900m, createdAt),
            new("P-1021", "Asli", "Tuna", "Finans", "Uzman", "0532 100 10 21", "asli.tuna@example.local", "Cankaya, Ankara", 39.902000m, 32.862400m, createdAt),
            new("P-1022", "Ege", "Bozkurt", "Bakim", "Tekniker", "0532 100 10 22", "ege.bozkurt@example.local", "Siteler, Ankara", 39.969900m, 32.889000m, createdAt),
            new("P-1023", "Merve", "Aydin", "Planlama", "Analist", "0532 100 10 23", "merve.aydin@example.local", "Incek, Ankara", 39.817900m, 32.735300m, createdAt),
            new("P-1024", "Tolga", "Keskin", "Operasyon", "Uzman", "0532 100 10 24", "tolga.keskin@example.local", "Golbasi, Ankara", 39.790400m, 32.809000m, createdAt)
        ];
    }

    private static PhysicalShuttle[] CreateShuttles(DateTimeOffset createdAt)
    {
        var shuttles = new[]
        {
            new PhysicalShuttle("SVC-01", "06 ABC 101", "Cankaya merkez hatti", createdAt),
            new PhysicalShuttle("SVC-02", "06 ABC 102", "Sincan ve Etimesgut hatti", createdAt),
            new PhysicalShuttle("SVC-03", "06 ABC 103", "Mamak ve Cebeci hatti", createdAt),
            new PhysicalShuttle("SVC-04", "06 ABC 104", "Kecioren ve Pursaklar hatti", createdAt),
            new PhysicalShuttle("SVC-05", "06 ABC 105", "Yedek servis araci", createdAt)
        };

        shuttles[4].SetActiveStatus(false, createdAt.AddDays(3));

        return shuttles;
    }

    private static ShuttleShift[] CreateShifts(IReadOnlyList<PhysicalShuttle> shuttles, DateTimeOffset createdAt)
    {
        return
        [
            new(shuttles[0].Id, "Cankaya Sabah", ShiftType.Morning, 24, new TimeOnly(7, 15), new TimeOnly(8, 30), createdAt),
            new(shuttles[0].Id, "Cankaya Aksam", ShiftType.Evening, 24, new TimeOnly(17, 30), new TimeOnly(18, 45), createdAt),
            new(shuttles[1].Id, "Sincan Sabah", ShiftType.Morning, 28, new TimeOnly(6, 45), new TimeOnly(8, 20), createdAt),
            new(shuttles[1].Id, "Sincan Aksam", ShiftType.Evening, 28, new TimeOnly(17, 20), new TimeOnly(19, 00), createdAt),
            new(shuttles[2].Id, "Mamak Sabah", ShiftType.Morning, 20, new TimeOnly(7, 00), new TimeOnly(8, 15), createdAt),
            new(shuttles[2].Id, "Mamak Aksam", ShiftType.Evening, 20, new TimeOnly(17, 35), new TimeOnly(18, 50), createdAt),
            new(shuttles[3].Id, "Kecioren Sabah", ShiftType.Morning, 22, new TimeOnly(6, 55), new TimeOnly(8, 25), createdAt),
            new(shuttles[3].Id, "Kecioren Aksam", ShiftType.Evening, 22, new TimeOnly(17, 25), new TimeOnly(18, 55), createdAt),
            new(shuttles[4].Id, "Yedek Hafta Ici", ShiftType.Custom, 18, new TimeOnly(9, 00), new TimeOnly(18, 00), createdAt)
        ];
    }

    private static RoutePoint[] CreateRoutePoints(IReadOnlyList<ShuttleShift> shifts, DateTimeOffset createdAt)
    {
        return
        [
            .. CreateShiftRoute(shifts[0].Id, createdAt,
                (1, "Kizilay Duragi", "Kizilay Meydani", 39.920770m, 32.854110m),
                (2, "Bahcelievler 7. Cadde", "Bahcelievler, Ankara", 39.926500m, 32.831900m),
                (3, "Emek Metro", "Emek, Ankara", 39.920300m, 32.811100m),
                (4, "Sogutozu Kampus", "Sogutozu, Ankara", 39.913000m, 32.785000m)),
            .. CreateShiftRoute(shifts[1].Id, createdAt,
                (1, "Sogutozu Kampus", "Sogutozu, Ankara", 39.913000m, 32.785000m),
                (2, "Ayranci", "Ayranci, Ankara", 39.901800m, 32.857600m),
                (3, "Cankaya", "Cankaya, Ankara", 39.902000m, 32.862400m),
                (4, "Dikmen", "Dikmen, Ankara", 39.880600m, 32.846900m)),
            .. CreateShiftRoute(shifts[2].Id, createdAt,
                (1, "Sincan Merkez", "Sincan, Ankara", 39.966700m, 32.584400m),
                (2, "Etimesgut", "Etimesgut, Ankara", 39.948600m, 32.669700m),
                (3, "Batkent", "Batkent, Ankara", 39.971400m, 32.731100m),
                (4, "Sogutozu Kampus", "Sogutozu, Ankara", 39.913000m, 32.785000m)),
            .. CreateShiftRoute(shifts[4].Id, createdAt,
                (1, "Mamak Merkez", "Mamak, Ankara", 39.941900m, 32.916500m),
                (2, "Cebeci", "Cebeci, Ankara", 39.932800m, 32.878600m),
                (3, "Kurtulus", "Kurtulus, Ankara", 39.930900m, 32.862700m),
                (4, "Sogutozu Kampus", "Sogutozu, Ankara", 39.913000m, 32.785000m)),
            .. CreateShiftRoute(shifts[6].Id, createdAt,
                (1, "Pursaklar", "Pursaklar, Ankara", 40.039700m, 32.895700m),
                (2, "Kecioren", "Kecioren, Ankara", 39.977100m, 32.866300m),
                (3, "Yenimahalle", "Yenimahalle, Ankara", 39.971900m, 32.810600m),
                (4, "Sogutozu Kampus", "Sogutozu, Ankara", 39.913000m, 32.785000m))
        ];
    }

    private static RoutePoint[] CreateShiftRoute(
        Guid shiftId,
        DateTimeOffset createdAt,
        params (int Order, string Name, string Address, decimal Latitude, decimal Longitude)[] points)
    {
        return points
            .Select(point => new RoutePoint(
                shiftId,
                point.Order,
                point.Name,
                point.Address,
                point.Latitude,
                point.Longitude,
                createdAt))
            .ToArray();
    }

    private static Driver[] CreateDrivers(IReadOnlyList<ShuttleShift> shifts, DateTimeOffset createdAt)
    {
        var drivers = new[]
        {
            new Driver("Ali", "Korkmaz", "0533 200 20 01", "E-10001", createdAt),
            new Driver("Fatih", "Bulut", "0533 200 20 02", "E-10002", createdAt),
            new Driver("Yasin", "Dogan", "0533 200 20 03", "E-10003", createdAt),
            new Driver("Deniz", "Erdem", "0533 200 20 04", "E-10004", createdAt),
            new Driver("Koray", "Ates", "0533 200 20 05", "E-10005", createdAt),
            new Driver("Bora", "Yavuz", "0533 200 20 06", "E-10006", createdAt)
        };

        for (var index = 0; index < drivers.Length; index++)
        {
            drivers[index].AssignToShift(shifts[index].Id, createdAt.AddDays(1));
        }

        return drivers;
    }

    private static PersonnelAssignment[] CreateAssignments(
        IReadOnlyList<Personnel> personnel,
        IReadOnlyList<ShuttleShift> shifts,
        IReadOnlyList<RoutePoint> routePoints,
        DateTimeOffset createdAt)
    {
        var routePointLookup = routePoints
            .GroupBy(point => point.ShuttleShiftId)
            .ToDictionary(group => group.Key, group => group.OrderBy(point => point.Order).ToArray());

        return
        [
            CreateAssignment(personnel[0], shifts[0], routePointLookup, 0, createdAt),
            CreateAssignment(personnel[1], shifts[0], routePointLookup, 1, createdAt),
            CreateAssignment(personnel[2], shifts[0], routePointLookup, 2, createdAt),
            CreateAssignment(personnel[3], shifts[0], routePointLookup, 3, createdAt),
            CreateAssignment(personnel[4], shifts[1], routePointLookup, 1, createdAt),
            CreateAssignment(personnel[5], shifts[1], routePointLookup, 2, createdAt),
            CreateAssignment(personnel[6], shifts[2], routePointLookup, 0, createdAt),
            CreateAssignment(personnel[7], shifts[2], routePointLookup, 1, createdAt),
            CreateAssignment(personnel[8], shifts[2], routePointLookup, 2, createdAt),
            CreateAssignment(personnel[9], shifts[3], routePointLookup, 0, createdAt),
            CreateAssignment(personnel[10], shifts[3], routePointLookup, 1, createdAt),
            CreateAssignment(personnel[11], shifts[4], routePointLookup, 0, createdAt),
            CreateAssignment(personnel[12], shifts[4], routePointLookup, 1, createdAt),
            CreateAssignment(personnel[13], shifts[4], routePointLookup, 2, createdAt),
            CreateAssignment(personnel[14], shifts[6], routePointLookup, 0, createdAt),
            CreateAssignment(personnel[15], shifts[6], routePointLookup, 1, createdAt),
            CreateAssignment(personnel[16], shifts[7], routePointLookup, 0, createdAt),
            CreateAssignment(personnel[17], shifts[7], routePointLookup, 0, createdAt)
        ];
    }

    private static PersonnelAssignment CreateAssignment(
        Personnel personnel,
        ShuttleShift shift,
        IReadOnlyDictionary<Guid, RoutePoint[]> routePointLookup,
        int routePointIndex,
        DateTimeOffset createdAt)
    {
        var routePointId = routePointLookup.TryGetValue(shift.Id, out var points) && points.Length > 0
            ? points[Math.Min(routePointIndex, points.Length - 1)].Id
            : (Guid?)null;

        return new PersonnelAssignment(personnel.Id, shift.Id, routePointId, createdAt.AddDays(7));
    }

    private static SavedRoute[] CreateSavedRoutes(
        IReadOnlyList<ShuttleShift> shifts,
        IReadOnlyList<RoutePoint> routePoints,
        DateTimeOffset createdAt)
    {
        var routePointLookup = routePoints
            .GroupBy(point => point.ShuttleShiftId)
            .ToDictionary(group => group.Key, group => group.OrderBy(point => point.Order).ToArray());

        return
        [
            CreateSavedRoute(shifts[0], "Cankaya Sabah Demo Rota", 9200, 1680, routePointLookup, createdAt),
            CreateSavedRoute(shifts[1], "Cankaya Aksam Demo Rota", 8700, 1540, routePointLookup, createdAt),
            CreateSavedRoute(shifts[2], "Sincan Sabah Demo Rota", 24400, 2820, routePointLookup, createdAt),
            CreateSavedRoute(shifts[4], "Mamak Sabah Demo Rota", 13700, 2140, routePointLookup, createdAt),
            CreateSavedRoute(shifts[6], "Kecioren Sabah Demo Rota", 18100, 2480, routePointLookup, createdAt)
        ];
    }

    private static SavedRoute CreateSavedRoute(
        ShuttleShift shift,
        string name,
        double distanceMeters,
        double durationSeconds,
        IReadOnlyDictionary<Guid, RoutePoint[]> routePointLookup,
        DateTimeOffset createdAt)
    {
        var points = routePointLookup[shift.Id];
        return new SavedRoute(
            shift.Id,
            name,
            distanceMeters,
            durationSeconds,
            CreateGeometry(points),
            createdAt.AddDays(10));
    }

    private static string CreateGeometry(IReadOnlyList<RoutePoint> points)
    {
        var coordinates = points
            .OrderBy(point => point.Order)
            .Select(point => new[] { point.Longitude, point.Latitude })
            .ToArray();

        return JsonSerializer.Serialize(new
        {
            type = "LineString",
            coordinates
        });
    }
}
