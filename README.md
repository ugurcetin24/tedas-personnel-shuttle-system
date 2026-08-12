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

Frontend klasörü Phase 2 için ayrılmıştır; bu aşamada React uygulaması henüz oluşturulmamaktadır.

## Technologies

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core 10
- SQLite
- Serilog
- FluentValidation
- Swagger / OpenAPI
- xUnit

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

## Development Commands

```bash
dotnet restore TedasPersonnelShuttleSystem.sln
dotnet build TedasPersonnelShuttleSystem.sln --no-restore
dotnet test TedasPersonnelShuttleSystem.sln --no-build
dotnet run --project backend/Tedas.Shuttle.Api/Tedas.Shuttle.Api.csproj
```

Development ortamında Swagger UI:

```text
/swagger
```

Health endpoint:

```text
/health
```

## Database Migration Strategy

Phase 1'de `AppDbContext` ve SQLite bağlantısı hazırdır. API başlangıcında `Database.Migrate()` çalıştırılır. İlk domain entity'leri Phase 3 ile eklendiğinde EF Core migration dosyaları bu altyapı üzerinden üretilecektir.

## Project Phases

- Phase 0: Environment ve scaffolding. Tamamlandı.
- Phase 1: Backend foundation. Tamamlandı.
- Phase 2: Frontend foundation. Kullanıcı onayı olmadan başlatılmayacak.
- Phase 3: Personel modülünün ilk vertical slice'ı.
