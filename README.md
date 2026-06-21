# FuelWallet API

Backend service for fuel wallet authorization and transaction processing, built with
Clean Architecture and Domain-Driven Design.

## Tech Stack

- **.NET 8**, ASP.NET Core Minimal APIs
- **Entity Framework Core 8** + SQL Server (code-first migrations, `rowversion` concurrency)
- **MediatR** (CQRS) + **FluentValidation** (request validation pipeline behaviour)
- **JWT** bearer authentication (stateless; tokens expire naturally)
- **Serilog** structured logging (config-driven sinks/levels + per-request HTTP logging)
- BCrypt password hashing, fixed-window rate limiting, hosted background expiry job
- `TimeProvider` clock abstraction for deterministic, testable time

## Architecture

```
src/
├── Domain          — entities, aggregates, domain rules (no dependencies)
│                     Wallet owns the authorization invariant; rejections are results, not exceptions
├── Application     — CQRS handlers, DTOs, validators, interfaces (depends on Domain)
├── Infrastructure  — EF Core, persistence, JWT, BCrypt, background jobs (depends on Application)
└── Web             — Minimal API endpoints, middleware, DI composition root (depends on all)

tests/
└── Application.Tests — xUnit + FluentAssertions, in-memory EF, deterministic clock
```

## Run with Docker (recommended — one command)

```bash
docker-compose up --build
```

That's it. The stack starts SQL Server, waits for it to become healthy, then starts the API,
which **applies migrations and seeds data automatically on startup** (no manual EF step).

- Swagger UI: **http://localhost:5000/swagger**
- Health check: **http://localhost:5000/health**
- SQL Server: `localhost:1433` (`sa` / `P@ssw0rd@123`)

Stop and wipe the database volume with `docker-compose down -v`.

## Run Locally

**Prerequisites:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and SQL Server
(or just the DB from Docker: `docker-compose up sqlserver`).

```bash
dotnet run --project src/Web
```

Migrations apply automatically on startup. Then open **http://localhost:5123/swagger**.

The connection string lives in [src/Web/appsettings.json](src/Web/appsettings.json); override it
with the `ConnectionStrings__DefaultConnection` environment variable if needed.

## Authentication

A default user is **seeded automatically**, so you can log in immediately:

```jsonc
POST /api/auth/token
{ "username": "station-api", "password": "P@ssw0rd@123!" }
```

Use the returned token as `Authorization: Bearer <token>` on all business endpoints.
In Swagger, click **Authorize** and enter `Bearer <token>`.

- `POST /api/auth/register` — create another user (`{ "username", "password" }`)

Tokens are stateless and expire on their own (`JwtSettings:ExpiryMinutes`, default 15) — there is no logout/revocation step.

## API Endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET  | `/health` | — | Liveness probe |
| POST | `/api/auth/register` | — | Register a new user |
| POST | `/api/auth/token` | — | Log in, receive a JWT |
| POST | `/api/fuel-authorizations` | ✓ | Authorize a fuel request |
| GET  | `/api/transactions/{id}` | ✓ | Get a transaction by ID |
| GET  | `/api/wallets/{walletId}/transactions` | ✓ | List a wallet's transactions |
| GET  | `/api/wallets/{walletId}/balance` | ✓ | Get a wallet's balance |
| POST | `/api/dev/reseed-wallets` | ✓ | Reset wallet balances to seed values (Development only) |

## Error Responses

Every error flows through `ExceptionHandlingMiddleware` and returns a single, consistent JSON
shape (camelCase; `errors` is omitted when there's nothing to report). Internal details — entity
class names, stack traces — are never leaked; unhandled exceptions are logged server-side and
returned as a generic 500.

```json
{ "statusCode": 404, "error": "Not Found", "message": "Wallet 'WLT-9999' not found." }
```

Validation failures add a field-keyed `errors` object:

```json
{
  "statusCode": 400,
  "error": "Validation Failed",
  "message": "One or more validation errors occurred.",
  "errors": { "RequestedAmount": ["requested amount must be greater than zero."] }
}
```

| Status | `error` | When |
|--------|---------|------|
| 400 | Bad Request | Domain rule violation |
| 400 | Validation Failed | Request fails validation (includes `errors`) |
| 401 | Unauthorized | Invalid credentials |
| 404 | Not Found | Unknown wallet or transaction |
| 409 | Conflict | Duplicate `requestReference` race / conflicting state |
| 500 | Internal Server Error | Unhandled exception (logged, not leaked) |

## Postman

A ready-to-run collection lives in **[postman/](postman/)**:

- `postman/FuelWallet.postman_collection.json`
- `postman/FuelWallet.postman_environment.json`

Import both into Postman, select the **FuelWallet (Local)** environment, then run
**Auth → Get Token** — it captures the JWT into the `token` variable automatically, so every
other request is authenticated. The environment defaults to `http://localhost:5000` (Docker);
change `baseUrl` to `http://localhost:5123` if you run the API locally.

## Seed Data

Four wallets are seeded on first migration:

| WalletId | Customer | Balance | DailyLimit | Active |
|----------|----------|---------|-----------|--------|
| WLT-1001 | Ahmed Hassan | 500.00 | 300.00 | Yes |
| WLT-1002 | Sara Mostafa | 50.00 | 200.00 | Yes |
| WLT-1003 | Omar Khalil | 1000.00 | 100.00 | Yes |
| WLT-1004 | Layla Ibrahim | 500.00 | 300.00 | No (inactive) |

Plus five transactions (for exercising the GET endpoints) and one user
(`station-api` / `P@ssw0rd@123!`).

## Tests

```bash
dotnet test
```

36 tests across the domain (Wallet rules), the authorization handler, the optimistic-concurrency
retry policy, validators, auth handlers, and the expiry background job. Time is driven by a fixed
`TimeProvider`, so date-sensitive logic (daily-limit reset, expiry) is deterministic.

## Notable Behaviours & Assumptions

1. **WalletId is business-defined** — IDs like `WLT-1001` come from an upstream system; this service does not generate them.
2. **Daily limit resets at UTC midnight** — `CreatedAt >= today`, where `today` is read from the injected `TimeProvider`.
3. **Balance is deducted on authorization** — a request transitions Pending → Authorized/Rejected atomically in one save; the wallet balance drops at authorization time.
4. **Idempotency via `requestReference`** — a repeated reference returns the original result instead of double-charging (app-level fast path + unique-index guard for races).
5. **Optimistic concurrency via `rowversion`** — retry-once-on-conflict lives in `OptimisticConcurrencyExecutor` (Infrastructure); its logic is unit-tested. The SQL-level conflict itself needs an integration test (in-memory EF does not enforce `rowversion`).
6. **Stale Pending transactions expire** — a background job marks `Pending` transactions older than 2 minutes as `Expired` (no balance change).
7. **Rejections are persisted** — every rejected authorization is saved with its reason, giving a full audit trail.

