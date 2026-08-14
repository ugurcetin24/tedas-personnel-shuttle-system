# TEDAŞ Personel Servisi Atama Sistemi

**TEDAŞ Personel Servisi Atama Sistemi**, TEDAŞ bünyesindeki personel servislerinin, servis vardiyalarının, şoförlerin, güzergâhların ve personel-servis atamalarının yönetilmesi amacıyla geliştirilen ASP.NET Core ve React tabanlı bir uygulamadır.

Bu depo greenfield olarak başlatılmıştır. İlk sürüm TEDAŞ'ın gerçek iç sistemlerine bağlanmaz; gerçek personel, plaka, Active Directory, SAP, API veya üretim veritabanı bilgisi içermez.

## Architecture

Backend pragmatik katmanlı mimari ile kurulmuştur:

- `Tedas.Shuttle.Domain`: teknoloji bağımsız domain alanı.
- `Tedas.Shuttle.Application`: use case, DTO, interface ve validator alanı.
- `Tedas.Shuttle.Infrastructure`: EF Core, SQLite ve dış servis implementasyonları.
- `Tedas.Shuttle.Api`: ASP.NET Core Web API, middleware, DI ve HTTP endpointleri.
- `Tedas.Shuttle.Tests`: xUnit testleri.

Frontend feature-based React yapısıyla kurulmuştur:

- `src/api`: merkezi Axios client ve API fonksiyonları.
- `src/components`: ortak UI parçaları.
- `src/hooks`: reusable React hooks.
- `src/layouts`: ana uygulama layout'u.
- `src/pages`: route sayfaları.
- `src/router`: React Router yapılandırması.
- `src/styles`: global CSS.

## Technologies

Backend:

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core 10
- SQLite
- Serilog
- FluentValidation
- Swagger / OpenAPI
- xUnit

Frontend:

- React 19
- TypeScript
- Vite
- React Router
- TanStack Query
- Material UI
- Axios
- React Hook Form
- Zod
- Leaflet / React Leaflet

## Data And Logging

Varsayılan local veri dizini:

```text
%LOCALAPPDATA%\TedasPersonnelShuttleSystem\
```

Varsayılan SQLite veritabanı:

```text
%LOCALAPPDATA%\TedasPersonnelShuttleSystem\tedas-personnel-shuttle.db
```

Loglar:

```text
%LOCALAPPDATA%\TedasPersonnelShuttleSystem\logs\
```

Path çözümleme `IApplicationDataPathProvider` üzerinden tek noktadan yapılır. `ConnectionStrings:Default` verilirse API bu connection string'i kullanır; verilmezse local app data altındaki SQLite dosyasına gider.

## Backend Commands

```bash
dotnet restore TedasPersonnelShuttleSystem.sln
dotnet build TedasPersonnelShuttleSystem.sln --no-restore
dotnet test TedasPersonnelShuttleSystem.sln --no-build
dotnet run --project backend/Tedas.Shuttle.Api/Tedas.Shuttle.Api.csproj
```

EF Core migration komutları:

```bash
dotnet ef migrations add InitialPersonnel --project backend/Tedas.Shuttle.Infrastructure/Tedas.Shuttle.Infrastructure.csproj --startup-project backend/Tedas.Shuttle.Api/Tedas.Shuttle.Api.csproj --output-dir Persistence/Migrations
dotnet ef database update --project backend/Tedas.Shuttle.Infrastructure/Tedas.Shuttle.Infrastructure.csproj --startup-project backend/Tedas.Shuttle.Api/Tedas.Shuttle.Api.csproj
```

Development ortamında Swagger UI:

```text
/swagger
```

Health endpoint:

```text
/health
```

Geocoding endpoint:

```text
GET /api/geocoding/search?query=Kizilay%20Ankara&limit=5
```

Routing endpointleri:

```text
POST /api/shifts/{shiftId}/routes/calculate
GET /api/shifts/{shiftId}/routes
POST /api/shifts/{shiftId}/routes
```

## Frontend Commands

```bash
cd frontend/tedas-shuttle-web
npm install
npm run build
npm run dev
```

Frontend backend adresini `VITE_API_BASE_URL` ile okur. Örnek değer:

```text
VITE_API_BASE_URL=http://localhost:5284
```

## CORS

API varsayılan olarak Vite development originleri için CORS izni içerir:

```text
http://localhost:5173
http://127.0.0.1:5173
```

Bu liste `Cors:AllowedOrigins` üzerinden değiştirilebilir.

## Database Migration Strategy

`AppDbContext` ve SQLite bağlantısı hazırdır. API başlangıcında `Database.Migrate()` çalıştırılır. Phase 3 ile `InitialPersonnel`, Phase 4 ile `AddPhysicalShuttles`, Phase 5 ile `AddShuttleShifts`, Phase 6 ile `AddDrivers`, Phase 7 ile `AddPersonnelAssignments`, Phase 8 ile `AddRoutePoints`, Phase 10 ile `AddSavedRoutes` migration dosyaları eklenmiştir.

## Personnel Module

Phase 3 ile Personel modülünün ilk vertical slice'ı tamamlanmıştır:

- `Personnel` domain entity'si.
- EF Core tablo konfigürasyonu, unique `RegistrationNumber` index'i ve sık kullanılan alan index'leri.
- `InitialPersonnel` migration.
- DTO, validation, repository interface ve application service.
- REST endpointleri:
  - `GET /api/personnel`
  - `GET /api/personnel/{id}`
  - `POST /api/personnel`
  - `PUT /api/personnel/{id}`
  - `PATCH /api/personnel/{id}/status`
- Frontend Personeller sayfası:
  - listeleme
  - pagination
  - arama
  - aktif/pasif filtresi
  - ekleme
  - düzenleme
  - aktif/pasif yapma

## Shuttle Module

Phase 4 ile fiziksel servis araçları modülünün vertical slice'ı tamamlanmıştır:

- `PhysicalShuttle` domain entity'si.
- EF Core tablo konfigürasyonu, unique `Code` index'i ve `PlateNumber` / `IsActive` index'leri.
- `AddPhysicalShuttles` migration.
- DTO, validation, repository interface ve application service.
- REST endpointleri:
  - `GET /api/shuttles`
  - `GET /api/shuttles/{id}`
  - `POST /api/shuttles`
  - `PUT /api/shuttles/{id}`
  - `PATCH /api/shuttles/{id}/status`
- Frontend Servisler sayfası:
  - listeleme
  - servis kodu ile arama
  - plaka ile arama
  - aktif/pasif filtresi
  - servis ekleme
  - servis düzenleme
  - aktif/pasif yapma

## Shuttle Shift Module

Phase 5 ile servis vardiyaları modülünün vertical slice'ı tamamlanmıştır:

- `ShuttleShift` domain entity'si ve `ShiftType` enum'u.
- `PhysicalShuttle` ile bire çok vardiya ilişkisi.
- EF Core tablo konfigürasyonu, `PhysicalShuttleId` / `IsActive` index'leri ve `AddShuttleShifts` migration.
- DTO, validation, repository interface ve application service.
- Kapasite güncellemede mevcut doluluğun altına düşmeyi engelleyen business rule.
- REST endpointleri:
  - `GET /api/shuttles/{shuttleId}/shifts`
  - `POST /api/shuttles/{shuttleId}/shifts`
  - `GET /api/shifts/{id}`
  - `PUT /api/shifts/{id}`
  - `PATCH /api/shifts/{id}/status`
- Frontend Servis Detay sayfası:
  - servis özeti
  - vardiya listeleme
  - vardiya ekleme
  - vardiya düzenleme
  - aktif/pasif yapma
  - kapasite, doluluk ve boş koltuk gösterimi

Not: Personel-servis atama modülü henüz uygulanmadığı için repository tarafındaki aktif doluluk sayımı Phase 7'ye hazır bir genişleme noktası olarak şimdilik `0` döner.

## Driver Module

Phase 6 ile şoför modülünün vertical slice'ı tamamlanmıştır:

- `Driver` domain entity'si.
- Şoför ile servis vardiyası arasında optional bire bir ilişki.
- İlişki kararı: Şoför ataması fiziksel servis yerine `ShuttleShift` üzerinden tutulur; böylece aynı servis aracının sabah/akşam vardiyaları ayrı operasyonel atama olarak yönetilebilir.
- EF Core tablo konfigürasyonu, unique `LicenseNumber` index'i, nullable unique `ShuttleShiftId` index'i ve `AddDrivers` migration.
- DTO, validation, repository interface ve application service.
- REST endpointleri:
  - `GET /api/drivers`
  - `GET /api/drivers/{id}`
  - `POST /api/drivers`
  - `PUT /api/drivers/{id}`
  - `PATCH /api/drivers/{id}/status`
  - `PATCH /api/drivers/{id}/shift-assignment`
  - `GET /api/shifts`
- Frontend Şoförler sayfası:
  - listeleme
  - arama
  - aktif/pasif filtresi
  - şoför ekleme
  - şoför düzenleme
  - aktif/pasif yapma
  - vardiya ilişkilendirme
  - vardiya ilişkisini kaldırma

## Assignment Module

Phase 7 ile servis atamaları modülünün vertical slice'ı tamamlanmıştır:

- `PersonnelAssignment` domain entity'si.
- Aktif personel için aynı anda tek aktif servis ataması kuralı.
- EF Core tablo konfigürasyonu, `PersonnelId` için aktif kayıtlarda unique index ve `ShuttleShiftId` / `IsActive` index'leri.
- `AddPersonnelAssignments` migration.
- DTO, validation, repository interface ve application service.
- Backend business rule kontrolleri:
  - pasif personel atanamaz
  - pasif servis atanamaz
  - pasif vardiya atanamaz
  - aynı personel için duplicate aktif atama oluşturulamaz
  - kapasitesi dolu vardiyaya yeni atama yapılamaz
- REST endpointleri:
  - `GET /api/assignments`
  - `GET /api/assignments/{id}`
  - `POST /api/assignments`
  - `DELETE /api/assignments/{id}`
- Frontend Servis Atamaları sayfası:
  - listeleme
  - arama
  - aktif/pasif filtresi
  - aktif personel ve aktif vardiya ile atama başlatma
  - atamayı pasife alma
  - vardiya doluluk bilgisini gösterme

Not: `BoardingRoutePointId` alanı Phase 8 RoutePoint modülü için nullable olarak hazırlandı.

## Route Point Module

Phase 8 ile güzergah noktaları modülünün vertical slice'ı tamamlanmıştır:

- `RoutePoint` domain entity'si.
- Her `ShuttleShift` için sıralı güzergah noktaları.
- EF Core tablo konfigürasyonu, unique `(ShuttleShiftId, Order)` index'i ve `AddRoutePoints` migration.
- DTO, validation, repository interface ve application service.
- Backend business rule kontrolleri:
  - route point yalnızca mevcut vardiya altında oluşturulur
  - koordinatlar geçerli latitude/longitude aralığında olmalıdır
  - sıralama isteği vardiyaya ait tüm noktaları eksiksiz ve tekrarsız içermelidir
  - personel atamasında `BoardingRoutePointId` verilirse seçilen vardiyaya ait aktif nokta olmalıdır
- REST endpointleri:
  - `GET /api/shifts/{shiftId}/route-points`
  - `POST /api/shifts/{shiftId}/route-points`
  - `PATCH /api/shifts/{shiftId}/route-points/order`
  - `GET /api/route-points/{id}`
  - `PUT /api/route-points/{id}`
  - `PATCH /api/route-points/{id}/status`
- Frontend Güzergahlar sayfası:
  - aktif vardiya seçimi
  - güzergah noktası listeleme
  - nokta ekleme
  - nokta düzenleme
  - aktif/pasif yapma
  - yukarı/aşağı taşıyarak sıralama

## Map And Geocoding Module

Phase 9 ile harita ve adres arama entegrasyonunun ilk vertical slice'ı tamamlanmıştır:

- Backend geocoding akışı:
  - `IGeocodingService` application interface'i
  - `NominatimGeocodingService` infrastructure implementation'ı
  - `HttpClientFactory` kullanımı
  - timeout, HTTP failure ve malformed JSON durumlarında kontrollü boş sonuç
  - `ExternalServices:Nominatim:BaseUrl` configuration kullanımı
- REST endpoint:
  - `GET /api/geocoding/search`
- Frontend Güzergahlar sayfası:
  - Leaflet / OpenStreetMap harita paneli
  - aktif güzergah noktalarını marker olarak gösterme
  - aktif noktaları çizgi ile bağlama
  - adres arama
  - seçilen geocoding sonucunu route point formuna koordinat/adres olarak aktarma

## Routing Module

Phase 10 ile OSRM rota hesaplama vertical slice'ı tamamlanmıştır:

- `SavedRoute` domain entity'si.
- EF Core tablo konfigürasyonu, `ShuttleShiftId` index'i ve `AddSavedRoutes` migration.
- Backend routing akışı:
  - `IRoutingService` application interface'i
  - `OsrmRoutingService` infrastructure implementation'ı
  - `HttpClientFactory` kullanımı
  - timeout, HTTP failure ve malformed JSON durumlarında kontrollü `null` sonuç
  - `ExternalServices:Osrm:BaseUrl` configuration kullanımı
- Rota hesaplama business rule'u:
  - en az iki aktif güzergah noktası olmadan rota hesaplanamaz
  - OSRM sonucu alınamazsa kayıt işlemi kontrollü conflict hatası verir
- REST endpointleri:
  - `POST /api/shifts/{shiftId}/routes/calculate`
  - `GET /api/shifts/{shiftId}/routes`
  - `POST /api/shifts/{shiftId}/routes`
- Frontend Güzergahlar sayfası:
  - manuel nokta sırasını kesikli çizgiyle gösterme
  - OSRM hesaplanan yol geometrisini ayrı çizgiyle gösterme
  - mesafe ve süre özeti
  - hesaplanan rotayı isimle kaydetme
  - kayıtlı rotaları vardiya bazında listeleme

## Project Phases

- Phase 0: Environment ve scaffolding. Tamamlandı.
- Phase 1: Backend foundation. Tamamlandı.
- Phase 2: Frontend foundation. Tamamlandı.
- Phase 3: Personel modülünün ilk vertical slice'ı. Tamamlandı.
- Phase 4: Servisler modülü. Tamamlandı.
- Phase 5: Servis vardiyaları. Tamamlandı.
- Phase 6: Şoförler. Tamamlandı.
- Phase 7: Servis atamaları. Tamamlandı.
- Phase 8: Güzergah noktaları. Tamamlandı.
- Phase 9: Harita ve geocoding. Tamamlandı.
- Phase 10: OSRM routing. Tamamlandı.
- Phase 11: Excel import/export core. Sıradaki faz.
