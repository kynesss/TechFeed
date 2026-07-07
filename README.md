# TechFeed

Agregator artykułów technicznych. Pobiera najnowsze wpisy z **Dev.to** i **Hacker News**,
zapisuje je w MongoDB, cache'uje odczyty w Redis (Cache Aside Pattern) i udostępnia przez
REST API dla frontendu w Blazor WebAssembly.

## Tech Stack

- **.NET 10** — ASP.NET Core Minimal API + Blazor WebAssembly
- **MongoDB 7** (`MongoDB.Driver` 3.9.0) — trwałe przechowywanie artykułów
- **Redis 7** (`StackExchange.Redis` 3.0.11) — cache warstwy odczytu (Cache Aside)
- **Refit** 13.1.0 — typowani klienci HTTP do Dev.to i Hacker News
- **Serilog** 10.0.0 — logowanie strukturalne (konsola + rolling file)
- **Scalar** 2.16.10 — interaktywne UI do OpenAPI
- **Blazor WebAssembly** 10.0.3 — frontend (SPA)
- **xUnit** — testy

## Uruchomienie

Wymagania: .NET 10 SDK, Docker.

```bash
# 1. Infrastruktura (MongoDB + mongo-express + Redis)
docker compose up -d

# 2. API (http://localhost:5016, Scalar UI pod /scalar/v1)
dotnet run --project src/TechFeed.API

# 3. Frontend Blazor WASM (http://localhost:5164)
dotnet run --project src/TechFeed.Client
```

Podglądy infrastruktury:
- **mongo-express** — http://localhost:8081 (login `admin` / `admin`)
- **Redis** — `docker exec -it techfeed-redis redis-cli`

Najpierw napełnij bazę danymi (wymaga API key — patrz niżej):

```bash
curl -X POST "http://localhost:5016/api/feed/refresh?tag=csharp&limit=10" \
  -H "X-Api-Key: dev-secret-key-change-me"
```

## Endpointy

| Metoda | Ścieżka                 | Opis                                                        | API key |
|--------|-------------------------|------------------------------------------------------------|:-------:|
| GET    | `/api/articles`         | Lista artykułów (query: `tag`, `source`, `limit` 1–100)    | nie     |
| GET    | `/api/articles/{id}`    | Pojedynczy artykuł po id (404 gdy brak)                    | nie     |
| GET    | `/api/sources`          | Lista wspieranych źródeł: `["devto","hackernews"]`         | nie     |
| POST   | `/api/feed/refresh`     | Pobiera i zapisuje artykuły ze źródeł; invaliduje cache    | **tak** |

Endpoint `POST /api/feed/refresh` wymaga nagłówka `X-Api-Key` zgodnego z
`ApiSettings:RefreshApiKey`. Brak lub błędny klucz → `401 Unauthorized`
(`{"error":"Invalid or missing API key"}`). Endpointy GET są publiczne.

## Czego się nauczyłem

- **Cache Aside Pattern** — odczyt najpierw z Redis (HIT), przy braku (MISS) z MongoDB
  + zapis do cache z TTL 10 min; invalidacja kluczy `articles:*` / `article:*` po refreshu.
- **Repository Pattern** — `IArticleRepository` (kontrakt w Core) odcina logikę biznesową
  od szczegółów MongoDB; ta sama zasada dla `ICacheService`.
- **Clean Architecture** — zależności skierowane do środka: `Core` nie zna infrastruktury,
  a `Infrastructure`/`API` implementują jego kontrakty (dependency inversion).
- **Refit** — deklaratywni klienci HTTP jako interfejsy z atrybutami; ujednolicenie
  różnych API (Dev.to, Hacker News) pod wspólny kontrakt `IArticleSourceClient`.
- **Odporność na częściowe błędy** — awaria jednego źródła nie przerywa odświeżania reszty.
- **Serilog** — logowanie strukturalne do konsoli i pliku; obserwacja cache HIT/MISS.

## Struktura projektu

```
TechFeed.sln
├── src/
│   ├── TechFeed.Core            # modele domenowe + interfejsy (bez zależności infra)
│   ├── TechFeed.Infrastructure  # MongoDB, Redis, Refit clients, serwisy
│   ├── TechFeed.API             # Minimal API, endpointy, middleware, DI
│   ├── TechFeed.Client          # Blazor WebAssembly (frontend)
│   └── TechFeed.Shared          # DTO współdzielone API ↔ Client
└── tests/
    └── TechFeed.Tests           # xUnit
```

Warstwy zależą tylko „do środka": `API → Infrastructure → Core`, `Client → Shared`,
`Core` bez referencji. Dzięki temu domena pozostaje niezależna od baz danych i bibliotek.
