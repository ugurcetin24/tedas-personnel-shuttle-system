# TEDAS Personel Servisi Atama Sistemi Proje Dokumani

## 1. Dokuman Amaci

Bu dokuman, mevcut `PersonnelShuttleSystem` projesinin ne yaptigini, nasil gelistirildigini, hangi teknolojileri kullandigini, backend/frontend mimarisini, veri modelini, API yuzeyini, calistirma adimlarini ve test stratejisini aciklamak icin hazirlanmistir.

Proje, TEDAS personel servislerinin yonetimi icin gelistirilmis bir web uygulamasidir. Ilk surum local calisan, greenfield baslatilmis, gercek TEDAS ic sistemlerine baglanmayan bir uygulamadir. Gercek personel verisi, plaka verisi, SAP/Active Directory entegrasyonu veya uretim veritabani baglantisi icermez.

## 2. Proje Ozeti

TEDAS Personel Servisi Atama Sistemi; personellerin, servis araclarinin, servis vardiyalarinin, soforlerin, guzergah noktalarinin, servis atamalarinin ve Excel aktarim sureclerinin yonetilmesi icin tasarlanmistir.

Uygulamanin temel hedefleri sunlardir:

- Personel kayitlarini yonetmek
- Fiziksel servis araclarini ve vardiyalarini tanimlamak
- Soforleri vardiyalara atamak
- Personelleri uygun servis vardiyalarina yerlestirmek
- Servis guzergah noktalarini harita uzerinden yonetmek
- Adres arama ve rota hesaplama entegrasyonlari saglamak
- Excel dosyalariyla toplu personel, kapasite ve guzergah aktarimi yapmak
- Dashboard uzerinden genel kapasite, doluluk ve operasyon durumunu izlemek

## 3. Gelistirme Yaklasimi

Proje asamali ve vertical slice yaklasimiyla gelistirilmistir. Her fazda bir islevsel alan backend, frontend, veri modeli ve testleriyle birlikte tamamlanmistir.

Tamamlanan fazlar:

- Phase 0: Ortam ve proje iskeleti
- Phase 1: Backend temeli
- Phase 2: Frontend temeli
- Phase 3: Personel modulu
- Phase 4: Servis araci modulu
- Phase 5: Servis vardiyalari
- Phase 6: Sofor modulu
- Phase 7: Servis atamalari
- Phase 8: Guzergah noktalari
- Phase 9: Harita ve geocoding
- Phase 10: OSRM rota hesaplama
- Phase 11: Excel import/export cekirdegi
- Phase 12: Personel Excel import commit akisi
- Phase 13: Servis/vardiya kapasite import akisi
- Phase 14: Guzergah Excel import akisi
- Phase 15: Dashboard ve UX
- Phase 16: Final dogrulama

Her fazda once domain ve application katmani kurallari olusturulmus, sonra infrastructure implementasyonlari ve API endpointleri eklenmis, ardindan frontend ekrani ve testler tamamlanmistir.

## 4. Teknoloji Stack'i

### 4.1 Backend Teknolojileri

Backend .NET tabanli, katmanli mimariye sahip bir ASP.NET Core Web API projesidir.

Kullanilan baslica teknolojiler:

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core 10.0.11
- SQLite
- FluentValidation 12.1.1
- Serilog.AspNetCore 10.0.0
- Serilog.Sinks.File 7.0.0
- Swashbuckle.AspNetCore 10.2.3
- ClosedXML 0.105.1
- Microsoft.Extensions.Http 10.0.11
- xUnit 2.9.3
- xUnit Visual Studio Runner 3.1.4
- coverlet.collector 6.0.4

### 4.2 Frontend Teknolojileri

Frontend React ve TypeScript ile gelistirilmis Vite tabanli bir SPA uygulamasidir.

Kullanilan baslica teknolojiler:

- React 19.2.8
- React DOM 19.2.8
- TypeScript 6.0.2
- Vite 8.2.x
- React Router 7.18.2
- TanStack Query 5.101.4
- Material UI 9.3.1
- MUI Icons 9.3.1
- Emotion React / Styled
- Axios 1.19.0
- React Hook Form 7.85.0
- Zod 4.4.3
- Leaflet 1.9.4
- React Leaflet 5.0.0

### 4.3 Harici Servisler

Proje iki harici HTTP servisi icin altyapi icerir:

- Nominatim: Adres arama ve koordinat bulma
- OSRM: Guzergah noktalarindan rota hesaplama

Bu servisler `appsettings.json` icindeki `ExternalServices` bolumunden ayarlanir. Varsayilan adresler:

```json
{
  "Nominatim": {
    "BaseUrl": "https://nominatim.openstreetmap.org"
  },
  "Osrm": {
    "BaseUrl": "https://router.project-osrm.org"
  }
}
```

## 5. Repository ve Dosya Yapisi

Proje kok dizininde backend, frontend, solution dosyasi ve dokumantasyon bulunur.

```text
PersonnelShuttleSystem/
  TedasPersonnelShuttleSystem.sln
  README.md
  PROJECT_DOCUMENTATION.md
  backend/
    Tedas.Shuttle.Domain/
    Tedas.Shuttle.Application/
    Tedas.Shuttle.Infrastructure/
    Tedas.Shuttle.Api/
    Tedas.Shuttle.Tests/
  frontend/
    tedas-shuttle-web/
```

Backend katmanlari:

```text
backend/
  Tedas.Shuttle.Domain
    Entities
    Enums
  Tedas.Shuttle.Application
    Common
    DTOs
    Imports
    Interfaces
    Services
    Validators
  Tedas.Shuttle.Infrastructure
    Excel
    Geocoding
    Persistence
    Repositories
    Routing
  Tedas.Shuttle.Api
    Controllers
    Extensions
    Middleware
    Properties
  Tedas.Shuttle.Tests
```

Frontend yapisi:

```text
frontend/tedas-shuttle-web/src/
  api
  components
  features
  hooks
  layouts
  pages
  router
  styles
```

## 6. Mimari

### 6.1 Backend Mimari Yaklasimi

Backend pragmatik katmanli mimari ile kurulmustur. Amac, domain kurallarini HTTP, EF Core ve dis servis detaylarindan ayirmaktir.

Katmanlar:

- `Tedas.Shuttle.Domain`: Entity ve enum gibi teknoloji bagimsiz domain nesnelerini icerir.
- `Tedas.Shuttle.Application`: Use case servisleri, DTO'lar, repository interface'leri, validator'lar ve is kurallarini icerir.
- `Tedas.Shuttle.Infrastructure`: EF Core, SQLite, repository implementasyonlari, Excel okuyucu, Nominatim ve OSRM servis implementasyonlarini icerir.
- `Tedas.Shuttle.Api`: HTTP controller'lari, Swagger, CORS, global exception middleware, health endpoint ve uygulama baslangic konfigurasyonunu icerir.
- `Tedas.Shuttle.Tests`: xUnit testlerini icerir.

Bagimlilik yonu su sekildedir:

```text
API -> Application -> Domain
API -> Infrastructure -> Application -> Domain
Tests -> API / Application / Infrastructure
```

Domain katmani herhangi bir framework'e bagli degildir. Infrastructure katmani Application tarafindaki interface'leri uygular. API katmani dependency injection ile application ve infrastructure servislerini baglar.

### 6.2 Frontend Mimari Yaklasimi

Frontend feature-based React yapisiyla gelistirilmistir. Her islevsel alan kendi `features` klasoru altinda API fonksiyonlarini, tiplerini, hook'larini ve form semalarini barindirir.

Frontend mimarisindeki ana bolumler:

- `src/api`: Merkezi Axios client ve ortak API fonksiyonlari
- `src/features`: Personel, servis, vardiya, atama, sofor, guzergah, import gibi alan bazli kodlar
- `src/pages`: Route seviyesinde sayfa componentleri
- `src/layouts`: Sol menu, ust bar ve ana sayfa yerlesimi
- `src/router`: React Router route tanimlari
- `src/components`: Ortak UI componentleri
- `src/styles`: Global CSS

Frontend veri cekme, cache invalidation ve mutation yonetimi icin TanStack Query kullanir. Formlarda React Hook Form ve Zod birlikte kullanilir. UI katmani Material UI ile olusturulmustur.

## 7. Backend Detaylari

### 7.1 API Baslangici

API giris noktasi `backend/Tedas.Shuttle.Api/Program.cs` dosyasidir.

Baslangicta yapilan ana islemler:

- Serilog konfigurasyonu
- Application servislerinin eklenmesi
- Infrastructure servislerinin eklenmesi
- Controller, Swagger, CORS ve ProblemDetails konfigurasyonu
- Global exception middleware'in eklenmesi
- Development ortaminda Swagger UI acilmasi
- CORS politikasinin uygulanmasi
- Controller endpointlerinin map edilmesi
- Health endpointlerinin map edilmesi
- Uygulama acilirken EF Core migration'larin uygulanmasi

Baslangic akisi ozet olarak:

```text
CreateBuilder
  -> UseSerilog
  -> AddApplication
  -> AddInfrastructure
  -> AddApiServices
Build
  -> GlobalExceptionHandlingMiddleware
  -> Swagger
  -> HTTPS Redirection
  -> CORS
  -> Controllers
  -> Health
  -> Database.Migrate()
Run
```

### 7.2 Dependency Injection

Application katmaninda su servisler DI'a eklenir:

- `IPersonnelService` -> `PersonnelService`
- `IShuttleService` -> `ShuttleService`
- `IShiftService` -> `ShiftService`
- `IDriverService` -> `DriverService`
- `IAssignmentService` -> `AssignmentService`
- `IRoutePointService` -> `RoutePointService`
- `IRouteCalculationService` -> `RouteCalculationService`
- `IExcelImportPreviewService` -> `ExcelImportPreviewService`
- `IDashboardService` -> `DashboardService`

Infrastructure katmaninda su repository ve dis servis implementasyonlari DI'a eklenir:

- `IPersonnelRepository` -> `PersonnelRepository`
- `IShuttleRepository` -> `ShuttleRepository`
- `IShiftRepository` -> `ShiftRepository`
- `IDriverRepository` -> `DriverRepository`
- `IAssignmentRepository` -> `AssignmentRepository`
- `IRoutePointRepository` -> `RoutePointRepository`
- `ISavedRouteRepository` -> `SavedRouteRepository`
- `IDashboardRepository` -> `DashboardRepository`
- `IExcelWorkbookReader` -> `ClosedXmlWorkbookReader`
- `IGeocodingService` -> `NominatimGeocodingService`
- `IRoutingService` -> `OsrmRoutingService`

### 7.3 Veritabani

Veritabani olarak SQLite kullanilir. EF Core DbContext sinifi:

```text
Tedas.Shuttle.Infrastructure/Persistence/AppDbContext.cs
```

DbSet'ler:

- `Personnel`
- `PhysicalShuttles`
- `ShuttleShifts`
- `Drivers`
- `PersonnelAssignments`
- `RoutePoints`
- `SavedRoutes`

API baslarken `app.MigrateDatabase()` ile bekleyen migration'lar uygulanir.

Varsayilan local veri dizini:

```text
%LOCALAPPDATA%\TedasPersonnelShuttleSystem\
```

Varsayilan SQLite veritabani:

```text
%LOCALAPPDATA%\TedasPersonnelShuttleSystem\tedas-personnel-shuttle.db
```

Connection string verilirse API onu kullanir:

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=..."
  }
}
```

Development ortaminda `appsettings.Development.json` uzerinden varsayilan connection string su sekildedir:

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=tedas-shuttle-dev.db"
  }
}
```

Bu ayar, gelistirme ve sunum sirasinda proje calisma dizininde `.gitignore` kapsaminda kalan bir SQLite dosyasi kullanilmasini saglar. Boylece AppData izin/path problemleri sunum akisini bozmaz.

### 7.3.1 Demo Data Seed

Sunum ve demo senaryolari icin Development ortaminda otomatik demo veri seed mekanizmasi eklenmistir.

Konfigurasyon:

```json
{
  "DemoData": {
    "SeedOnStartup": true
  }
}
```

API startup sirasinda once EF Core migration'lari uygular. Ardindan uygulama Development ortamindaysa ve `DemoData:SeedOnStartup` degeri `true` ise demo data seeder calisir.

Seeder idempotent tasarlanmistir. Veritabaninda personel, servis, vardiya, sofor, atama, guzergah noktasi veya kayitli rota verisi varsa seed islemi atlanir. Bu sayede uygulama her baslatildiginda duplicate demo veri olusmaz.

Varsayilan demo veri kapsami:

- 24 personel
- 5 servis araci
- 9 servis vardiyasi
- 6 sofor
- 18 servis atamasi
- 20 guzergah noktasi
- 5 kayitli rota

Demo seed implementasyonu:

```text
backend/Tedas.Shuttle.Infrastructure/Persistence/DemoDataSeeder.cs
```

Startup entegrasyonu:

```text
backend/Tedas.Shuttle.Api/Extensions/DatabaseApplicationBuilderExtensions.cs
```

### 7.4 Migration'lar

Projede su migration'lar bulunur:

- `20260813104609_InitialPersonnel`
- `20260813105923_AddPhysicalShuttles`
- `20260813111522_AddShuttleShifts`
- `20260813114342_AddDrivers`
- `20260813120310_AddPersonnelAssignments`
- `20260813121308_AddRoutePoints`
- `20260814105326_AddSavedRoutes`

### 7.5 Loglama

Loglama icin Serilog kullanilir.

Log hedefleri:

- Console
- Gunluk rolling file log

Dosya loglari varsayilan olarak local app data altina yazilir:

```text
%LOCALAPPDATA%\TedasPersonnelShuttleSystem\logs\
```

Log dosyasi format olarak su sekildedir:

```text
tedas-shuttle-YYYYMMDD.log
```

### 7.6 Hata Yonetimi

API'de merkezi hata yonetimi `GlobalExceptionHandlingMiddleware` ile yapilir.

Hata donusleri:

- FluentValidation hatalari: `400 Bad Request`
- Business conflict hatalari: `409 Conflict`
- Beklenmeyen hatalar: `500 Internal Server Error`

Yanıt tipi `application/problem+json` formatindadir. ProblemDetails cevabina ayrica `code` extension alani eklenir.

### 7.7 CORS

Development icin izin verilen frontend origin'leri:

```text
http://localhost:5173
http://127.0.0.1:5173
```

Bu liste `appsettings.json` icindeki `Cors:AllowedOrigins` bolumunden degistirilebilir.

### 7.8 Health Endpoint

Health endpoint:

```text
GET /health
```

Bu endpoint uygulama durumunu ve veritabani baglantisini kontrol eder.

## 8. Veri Modeli

### 8.1 Personnel

Personel kaydini temsil eder.

Baslica alanlar:

- `Id`
- `RegistrationNumber`
- `FirstName`
- `LastName`
- `Department`
- `Title`
- `Phone`
- `Email`
- `Address`
- `Latitude`
- `Longitude`
- `IsActive`
- `CreatedAt`
- `UpdatedAt`

`RegistrationNumber` unique olacak sekilde modellenmistir.

### 8.2 PhysicalShuttle

Fiziksel servis aracini temsil eder.

Baslica alanlar:

- `Id`
- `Code`
- `PlateNumber`
- `Description`
- `IsActive`
- `CreatedAt`
- `UpdatedAt`

`PhysicalShuttle` birden fazla `ShuttleShift` kaydina sahip olabilir.

### 8.3 ShuttleShift

Servis aracinin belirli bir vardiyasini temsil eder.

Baslica alanlar:

- `Id`
- `PhysicalShuttleId`
- `Name`
- `ShiftType`
- `Capacity`
- `StartTime`
- `EndTime`
- `IsActive`
- `CreatedAt`
- `UpdatedAt`

`ShiftType` degerleri:

- `Morning`
- `Evening`
- `Custom`

### 8.4 Driver

Sofor kaydini temsil eder.

Baslica alanlar:

- `Id`
- `FirstName`
- `LastName`
- `Phone`
- `LicenseNumber`
- `ShuttleShiftId`
- `IsActive`
- `CreatedAt`
- `UpdatedAt`

Soforler fiziksel araca degil, operasyonel vardiyaya atanir. Bu sayede ayni servis aracinin sabah ve aksam vardiyalari icin farkli sofor atamasi yapilabilir.

### 8.5 PersonnelAssignment

Personelin servis vardiyasina atanmasini temsil eder.

Baslica alanlar:

- `Id`
- `PersonnelId`
- `ShuttleShiftId`
- `BoardingRoutePointId`
- `AssignedAt`
- `IsActive`
- `DeactivatedAt`

Aktif bir personelin ayni anda yalnizca bir aktif servis atamasi olabilir.

### 8.6 RoutePoint

Bir servis vardiyasina ait guzergah noktasini temsil eder.

Baslica alanlar:

- `Id`
- `ShuttleShiftId`
- `Order`
- `Name`
- `Address`
- `Latitude`
- `Longitude`
- `IsActive`
- `CreatedAt`
- `UpdatedAt`

Ayni vardiya icinde `Order` degeri siralama icin kullanilir.

### 8.7 SavedRoute

OSRM tarafindan hesaplanmis ve kaydedilmis rotayi temsil eder.

Baslica alanlar:

- `Id`
- `ShuttleShiftId`
- `Name`
- `DistanceMeters`
- `DurationSeconds`
- `Geometry`
- `CreatedAt`
- `UpdatedAt`

`Geometry` alaninda hesaplanan rota geometrisi saklanir.

## 9. Backend Modulleri

### 9.1 Personel Modulu

Personel modulu personel kayitlarinin CRUD ve aktif/pasif durum yonetimini saglar.

Ozellikler:

- Personel listeleme
- Arama ve filtreleme
- Personel ekleme
- Personel guncelleme
- Aktif/pasif durum degistirme
- Sicil numarasi unique kontrolu
- Koordinat alanlariyla adres bazli personel konumu tutma

### 9.2 Servis Modulu

Fiziksel servis araclarini yonetir.

Ozellikler:

- Servis araci listeleme
- Servis kodu ve plaka ile arama
- Servis ekleme
- Servis guncelleme
- Aktif/pasif yapma
- Servis kodu unique kontrolu

### 9.3 Vardiya Modulu

Servis araclarina ait vardiyalari yonetir.

Ozellikler:

- Servise ait vardiyalari listeleme
- Tum aktif vardiyalari listeleme
- Vardiya ekleme
- Vardiya guncelleme
- Aktif/pasif yapma
- Kapasite ve doluluk hesaplari
- Kapasiteyi mevcut dolulugun altina dusurmeme kuralı

### 9.4 Sofor Modulu

Sofor kayitlarini ve vardiya iliskilerini yonetir.

Ozellikler:

- Sofor listeleme
- Sofor ekleme/guncelleme
- Aktif/pasif yapma
- Vardiyaya sofor atama
- Vardiya atamasini kaldirma
- Ehliyet numarasi unique kontrolu
- Bir vardiyaya ayni anda tek sofor atanmasi

### 9.5 Servis Atama Modulu

Personellerin servis vardiyalarina atanmasini saglar.

Is kurallari:

- Pasif personel atanamaz
- Pasif servis atanamaz
- Pasif vardiya atanamaz
- Ayni personel icin duplicate aktif atama olusturulamaz
- Kapasitesi dolu vardiyaya yeni atama yapilamaz
- Boarding route point verilirse secilen vardiyaya ait aktif nokta olmalidir

### 9.6 Guzergah Noktalari Modulu

Vardiyalara ait durak/guzergah noktalarini yonetir.

Ozellikler:

- Vardiya bazli guzergah noktasi listeleme
- Nokta ekleme/guncelleme
- Aktif/pasif yapma
- Sira degistirme
- Latitude/longitude validasyonu
- Ayni vardiya icinde sirali rota noktasi yapisi

### 9.7 Harita ve Geocoding Modulu

Adres arama ve harita uzerinde koordinat secimi icin kullanilir.

Backend:

- `IGeocodingService`
- `NominatimGeocodingService`
- HTTP failure ve malformed JSON durumlarinda kontrollu bos sonuc

Frontend:

- Leaflet harita
- OpenStreetMap tile layer
- Marker ile durak gosterimi
- Noktalari polyline ile baglama
- Adres arama sonucunu form alanlarina aktarma

### 9.8 Rota Hesaplama Modulu

OSRM ile guzergah noktalarindan rota hesaplar.

Ozellikler:

- Vardiya icin aktif route point'leri alir
- En az iki aktif nokta kuralini uygular
- OSRM'den mesafe, sure ve geometri bilgisi alir
- Hesaplanan rotayi kaydedebilir
- Kayitli rotalari vardiya bazinda listeler

### 9.9 Excel Import Modulu

Excel aktarim modulu ClosedXML ile `.xlsx` ve `.xlsm` dosyalarini okur.

Desteklenen import tipleri:

- Personel import
- Servis/vardiya kapasite import
- Guzergah import

Genel import yaklasimi:

- Dosya once preview edilir
- Kolonlar normalize edilir
- Hatalar, uyarilar ve conflict durumlari hesaplanir
- Preview hatasizsa commit endpointi cagrilir
- Commit islemleri transaction icinde yapilir
- Tekrar importlarda duplicate kayit olusturmamaya dikkat edilir

### 9.10 Dashboard Modulu

Dashboard backend'den gercek summary verisi alir.

Gosterilen metrikler:

- Toplam personel
- Aktif personel
- Toplam servis
- Aktif servis
- Toplam vardiya
- Aktif vardiya
- Atanmis personel
- Atanmamis personel
- Guzergah noktasi sayisi
- Kayitli rota sayisi
- Vardiya doluluklari
- Bos koltuk sayilari
- Doluluk yuzdeleri

## 10. API Endpoint Ozeti

### 10.1 Sistem Endpointleri

```text
GET /health
GET /swagger
GET /swagger/index.html
```

### 10.2 Dashboard

```text
GET /api/dashboard/summary
```

### 10.3 Personel

```text
GET   /api/personnel
GET   /api/personnel/{id}
POST  /api/personnel
PUT   /api/personnel/{id}
PATCH /api/personnel/{id}/status
```

### 10.4 Servisler

```text
GET   /api/shuttles
GET   /api/shuttles/{id}
POST  /api/shuttles
PUT   /api/shuttles/{id}
PATCH /api/shuttles/{id}/status
```

### 10.5 Vardiyalar

```text
GET   /api/shifts
GET   /api/shuttles/{shuttleId}/shifts
POST  /api/shuttles/{shuttleId}/shifts
GET   /api/shifts/{id}
PUT   /api/shifts/{id}
PATCH /api/shifts/{id}/status
```

### 10.6 Soforler

```text
GET   /api/drivers
GET   /api/drivers/{id}
POST  /api/drivers
PUT   /api/drivers/{id}
PATCH /api/drivers/{id}/status
PATCH /api/drivers/{id}/shift-assignment
```

### 10.7 Atamalar

```text
GET    /api/assignments
GET    /api/assignments/{id}
POST   /api/assignments
DELETE /api/assignments/{id}
```

### 10.8 Guzergah Noktalari

```text
GET   /api/shifts/{shiftId}/route-points
POST  /api/shifts/{shiftId}/route-points
PATCH /api/shifts/{shiftId}/route-points/order
GET   /api/route-points/{id}
PUT   /api/route-points/{id}
PATCH /api/route-points/{id}/status
```

### 10.9 Geocoding

```text
GET /api/geocoding/search?query=Kizilay%20Ankara&limit=5
```

### 10.10 Rotalar

```text
POST /api/shifts/{shiftId}/routes/calculate
GET  /api/shifts/{shiftId}/routes
POST /api/shifts/{shiftId}/routes
```

### 10.11 Excel Import

```text
POST /api/imports/personnel/preview
POST /api/imports/personnel/commit
POST /api/imports/capacity/preview
POST /api/imports/capacity/commit
POST /api/imports/routes/preview
POST /api/imports/routes/commit
```

## 11. Frontend Detaylari

### 11.1 Routing

Frontend route yapisi:

```text
/                 Dashboard
/personnel        Personeller
/shuttles         Servisler
/shuttles/:id     Servis Detay ve Vardiyalar
/drivers          Soforler
/assignments      Servis Atamalari
/routes           Guzergahlar
/imports          Excel Aktarim
```

Bilinmeyen route'lar ana sayfaya yonlendirilir.

### 11.2 Layout

Uygulama layout'u sol sabit menu ve ust durum barindan olusur.

Sol menude bulunan ekranlar:

- Dashboard
- Personeller
- Servisler
- Soforler
- Servis Atamalari
- Guzergahlar
- Excel Aktarim

Ust barda health badge bulunur. Bu badge backend `/health` endpointini sorgulayarak API ve veritabani durumunu gosterir.

### 11.3 API Client

Frontend API istekleri merkezi Axios client uzerinden gider.

Varsayilan backend adresi:

```text
http://localhost:5284
```

Environment variable ile degistirilebilir:

```text
VITE_API_BASE_URL=http://localhost:5284
```

Axios timeout degeri 10 saniyedir.

### 11.4 State ve Server Cache

TanStack Query su amaclarla kullanilir:

- Liste verilerini cekmek
- Loading/error state yonetmek
- Mutation sonrasi ilgili query'leri invalidate etmek
- Dashboard ve health gibi verileri merkezi hook'larla almak

### 11.5 Formlar ve Validasyon

Frontend formlarinda:

- React Hook Form form state icin
- Zod client-side schema validasyonu icin
- Material UI form componentleri gorsel katman icin kullanilir

Backend tarafinda ayrica FluentValidation ile server-side validation uygulanir.

## 12. Kullanici Akisi

Bos veritabaniyla baslayan tipik kullanim sirasi:

1. Servisler sayfasinda fiziksel servis araci eklenir.
2. Servis detay sayfasinda sabah/aksam/custom vardiya tanimlanir.
3. Personeller sayfasinda personel kayitlari eklenir.
4. Soforler sayfasinda soforler eklenir ve vardiyalara atanir.
5. Guzergahlar sayfasinda vardiya secilip route point'ler eklenir.
6. Harita uzerinden duraklar kontrol edilir.
7. Gerekirse Nominatim ile adres aramasi yapilir.
8. OSRM ile rota hesaplanir ve kaydedilir.
9. Servis Atamalari sayfasinda personeller vardiyalara atanir.
10. Dashboard uzerinden kapasite ve doluluk izlenir.
11. Excel Aktarim sayfasindan toplu personel, kapasite veya guzergah import edilir.

## 13. Excel Import Davranisi

### 13.1 Personel Import

Personel import akisi:

- Excel dosyasi yuklenir
- Header ve satirlar okunur
- Kolon eslestirme onerileri olusturulur
- Sicil numarasi normalize edilir
- Excel icindeki duplicate siciller conflict olarak isaretlenir
- Sistemde olmayan kayitlar `Create`
- Sistemde olan ve farkli veri tasiyan kayitlar `Update`
- Sistemde olan ve ayni veri tasiyan kayitlar `NoChange`
- Hata yoksa commit ile transaction icinde yazilir

### 13.2 Kapasite Import

Kapasite import business key:

```text
PhysicalShuttle.Code + ShuttleShift.Name
```

Davranis:

- Servis kodu yoksa conflict
- Vardiya varsa kapasite guncelleme kontrolu
- Vardiya yoksa gerekli alanlar varsa yeni vardiya olusturma
- Yeni kapasite mevcut dolulugun altina dusuyorsa conflict
- Commit transaction icinde yapilir

### 13.3 Guzergah Import

Guzergah import business key:

```text
PhysicalShuttle.Code + ShuttleShift.Name + RoutePoint.Order
```

Davranis:

- Servis/vardiya yoksa conflict
- Ayni servis/vardiya/sira tekrar ederse conflict
- Sira, durak adi, enlem ve boylam zorunludur
- Koordinatlar parse edilemiyorsa hata doner
- Var olan nokta farkliysa update
- Var olan nokta ayniysa no-change
- Yeni sira ise create

## 14. Konfigurasyon

### 14.1 Backend Konfigurasyonu

Ana konfigurasyon dosyasi:

```text
backend/Tedas.Shuttle.Api/appsettings.json
```

Onemli bolumler:

- `ConnectionStrings:Default`
- `Cors:AllowedOrigins`
- `DemoData:SeedOnStartup`
- `ExternalServices:Nominatim:BaseUrl`
- `ExternalServices:Osrm:BaseUrl`
- `Serilog`
- `Logging`

### 14.2 Frontend Konfigurasyonu

Frontend icin ornek environment dosyasi:

```text
frontend/tedas-shuttle-web/.env.example
```

Icerik:

```text
VITE_API_BASE_URL=http://localhost:5284
```

## 15. Calistirma

### 15.1 Gereksinimler

Gelistirme ortaminda gerekli araclar:

- .NET 10 SDK
- Node.js
- npm
- Git

### 15.2 Backend Calistirma

Kok dizinde:

```bash
dotnet restore TedasPersonnelShuttleSystem.sln
dotnet build TedasPersonnelShuttleSystem.sln --no-restore
dotnet run --project backend/Tedas.Shuttle.Api/Tedas.Shuttle.Api.csproj
```

Varsayilan backend adresi:

```text
http://localhost:5284
```

Swagger:

```text
http://localhost:5284/swagger
```

Health:

```text
http://localhost:5284/health
```

### 15.3 Frontend Calistirma

Frontend dizininde:

```bash
cd frontend/tedas-shuttle-web
npm install
npm run dev
```

Varsayilan frontend adresi:

```text
http://localhost:5173
```

### 15.4 Production Build

Backend build:

```bash
dotnet build TedasPersonnelShuttleSystem.sln --no-restore
```

Frontend build:

```bash
cd frontend/tedas-shuttle-web
npm run build
```

Not: Frontend production build sirasinda Vite buyuk chunk uyarisi uretebilir. Bu uyari build basarisizligi degildir.

## 16. Test Stratejisi

Test projesi:

```text
backend/Tedas.Shuttle.Tests
```

Kullanilan test teknolojileri:

- xUnit
- Microsoft.NET.Test.Sdk
- coverlet.collector

Test kapsaminda bulunan alanlar:

- DbContext ve SQLite baglantisi
- Application data path provider
- Personnel service
- Shuttle service
- Shift service
- Driver service
- Assignment service
- Route point service
- Route calculation service
- Nominatim geocoding service
- OSRM routing service
- Excel import core
- Dashboard repository

Final dogrulamada dogrudan test projesi uzerinden 67 testin tamamı basariyla calistirilmistir:

```bash
dotnet test backend/Tedas.Shuttle.Tests/Tedas.Shuttle.Tests.csproj --no-build
```

## 17. Kalite ve Dogrulama

Phase 16 final dogrulamada yapilan kontroller:

- `dotnet clean`
- `dotnet restore`
- `dotnet build --no-restore`
- `dotnet test backend/Tedas.Shuttle.Tests/Tedas.Shuttle.Tests.csproj --no-build`
- `npm ci`
- `npm run build`
- Temiz SQLite veritabani uzerinde EF Core migration dogrulamasi
- API smoke testleri:
  - `/health`
  - `/api/dashboard/summary`
  - `/swagger/index.html`
- Yari kalmis implementasyon belirteclerine karsi kaynak kod taramasi

## 18. Guvenlik ve Sinirlar

Mevcut surum gelistirme/local kullanim odaklidir.

Mevcut sinirlar:

- Authentication/authorization yoktur
- Rol bazli yetkilendirme yoktur
- Uretim veritabani konfigurasyonu yoktur
- Gercek TEDAS ic sistem entegrasyonu yoktur
- SAP, Active Directory veya kurumsal API baglantisi yoktur
- Audit trail sinirlidir
- Deployment altyapisi tanimli degildir

Bu nedenle uygulama dogrudan production ortamina alinmadan once kimlik dogrulama, yetkilendirme, uretim veritabani, merkezi loglama, hata izleme ve deployment konfigurasyonu eklenmelidir.

## 19. Onerilen Sonraki Adimlar

Production seviyesine yaklasmak icin dusunulebilecek gelistirmeler:

- Authentication ve role-based authorization
- Kullanici/rol yonetimi
- Audit log
- Production database secimi ve migration stratejisi
- Dockerfile ve deployment pipeline
- API versiyonlama
- OpenAPI schema zenginlestirme
- Frontend code splitting
- Dashboard filtreleri
- Excel export akislari
- Servis optimizasyon algoritmalari
- Kurumsal harita/rota servisi konfigurasyonu
- Gercek kurumsal sistem entegrasyonlari

## 20. Kisa Teknik Ozet

Bu proje, .NET 10 ve React 19 ile gelistirilmis, SQLite uzerinde calisan, katmanli mimariye sahip bir personel servis yonetim sistemidir. Backend tarafinda domain, application, infrastructure ve API katmanlari ayrilmistir. Frontend tarafinda feature-based React yapisi kullanilmistir. Veri erisimi EF Core ile, UI Material UI ile, server-state yonetimi TanStack Query ile, formlar React Hook Form ve Zod ile, Excel okuma ClosedXML ile, harita Leaflet ile, adres arama Nominatim ile, rota hesaplama OSRM ile yapilmaktadir.

Mevcut haliyle proje local gelistirme ve demo senaryolari icin calisir durumdadir. Personel, servis, vardiya, sofor, atama, guzergah, rota, Excel import ve dashboard akislari tamamlanmistir.
