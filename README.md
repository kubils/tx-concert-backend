# DotnetTemplate

A production-ready .NET 8 Web API template built with Clean Architecture, CQRS, and the Transactional Outbox pattern. Designed as the .NET counterpart to an existing NestJS template, preserving the same architectural decisions and feature set.

## Tech Stack

| Category | Technology |
|----------|-----------|
| Framework | ASP.NET Core 8 Web API |
| Language | C# 12 (latest) |
| ORM | Entity Framework Core 8 |
| Database | PostgreSQL 16 (Npgsql) |
| Messaging | MassTransit + RabbitMQ |
| Cache | StackExchange.Redis |
| CQRS | MediatR 12 |
| Validation | FluentValidation 12 |
| Logging | Serilog (Console, Seq) |
| Auth | JWT Bearer + API Key |
| Rate Limiting | Built-in .NET Rate Limiting |
| Scheduling | Quartz.NET |
| Testing | xUnit, Testcontainers, FluentAssertions, Moq |
| Date/Time | NodaTime |
| Storage | Google Cloud Storage, AWS S3 |
| API Docs | Swashbuckle (Swagger) |

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                      API Layer                          │
│   Controllers  │  Middleware  │  Auth  │  Rate Limiting │
├─────────────────────────────────────────────────────────┤
│                  Application Layer                      │
│   MediatR Handlers  │  Pipeline Behaviors  │  Validators│
├─────────────────────────────────────────────────────────┤
│                    Domain Layer                         │
│   Entities  │  Errors  │  Interfaces  │  Constants      │
├─────────────────────────────────────────────────────────┤
│                Infrastructure Layer                     │
│   EF Core  │  Repositories  │  Redis  │  MassTransit    │
└─────────────────────────────────────────────────────────┘
```

**Dependency flow:**

```
Api ──────→ Application ──→ Domain
Worker ──→ Application ──→ Domain
               ↑
         Infrastructure (implements Domain interfaces)
```

- **Domain** has zero external dependencies (only MediatR.Contracts and NodaTime)
- **Application** depends only on Domain
- **Infrastructure** implements Domain and Application interfaces
- **Api / Worker** are composition roots — they wire everything together

## Project Structure

```
DotnetTemplate.sln
├── src/
│   ├── DotnetTemplate.Api/              # ASP.NET Core Web API entry point
│   │   ├── Auth/                        # JWT Bearer + API Key authentication
│   │   ├── Controllers/                 # REST controllers (ExternalAudits, Health)
│   │   ├── Middleware/                  # GlobalExceptionHandler, CorrelationId
│   │   ├── RateLimit/                   # Rate limiting policies (Default, Auth, Public)
│   │   ├── Program.cs
│   │   ├── Dockerfile
│   │   └── appsettings.json
│   │
│   ├── DotnetTemplate.Application/      # Use cases, CQRS handlers
│   │   ├── Common/
│   │   │   ├── Behaviors/               # MediatR pipeline (Logging → Validation → Transaction → DomainEvents)
│   │   │   ├── Interfaces/              # IDbContext, ICurrentUserService, ITransactional
│   │   │   ├── Exceptions/              # ValidationException (422)
│   │   │   └── Responses/               # ApiResponse<T>, PaginatedResponse<T>
│   │   ├── Features/
│   │   │   └── ExternalAudits/
│   │   │       ├── Commands/            # CreateRequestAudit, RetryAudit
│   │   │       └── Queries/             # GetExternalAudit, GetExternalAudits
│   │   └── DependencyInjection.cs
│   │
│   ├── DotnetTemplate.Domain/           # Pure domain layer
│   │   ├── Common/
│   │   │   ├── Base/                    # BaseEntity (UUID, soft delete, audit, domain events)
│   │   │   ├── Cqrs/                    # ICommand, IQuery, IDomainEvent markers
│   │   │   ├── Enums/                   # RunMode, AppEnvironment
│   │   │   ├── Errors/                  # Exception hierarchy (400–503)
│   │   │   └── Settings/               # Typed configuration classes
│   │   ├── Commands/                    # ICommandBus, CommandOutboxEntity
│   │   ├── Events/                      # IEventBus, EventOutboxEntity
│   │   ├── Cache/                       # ICacheService, IDistributedLock
│   │   ├── Storage/                     # IStorageService
│   │   └── Features/
│   │       └── ExternalAudit/           # ExternalAuditEntity, IExternalAuditRepository
│   │
│   ├── DotnetTemplate.Infrastructure/   # Interface implementations
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs  # EF Core DbContext with global soft-delete filter
│   │   │   ├── Configurations/          # EF entity configurations (Fluent API)
│   │   │   ├── Interceptors/            # AuditInterceptor (auto CreatedAt/UpdatedAt/audit fields)
│   │   │   └── Repositories/            # BaseRepository<T>, outbox repos, feature repos
│   │   ├── BackgroundJobs/              # Quartz.NET jobs (outbox pollers, retention, stale checker)
│   │   ├── Cache/                       # RedisCacheService
│   │   ├── Messaging/                   # MassTransit command/event bus + consumers
│   │   ├── Services/                    # CurrentUserService
│   │   ├── Storage/                     # GoogleCloudStorageService
│   │   └── DependencyInjection.cs
│   │
│   └── DotnetTemplate.Worker/           # Background job host
│       ├── Program.cs                   # Quartz scheduler with 4 recurring jobs
│       └── Dockerfile
│
├── test/
│   ├── DotnetTemplate.UnitTests/        # Domain + Application unit tests
│   └── DotnetTemplate.IntegrationTests/ # WebApplicationFactory + Testcontainers
│
├── docker-compose.yml                   # API + Worker + PostgreSQL + Redis + RabbitMQ
├── Directory.Build.props                # Global build settings (TreatWarningsAsErrors, analyzers)
└── .editorconfig                        # Code style rules
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) (for infrastructure services)
- PostgreSQL 16
- Redis 7
- RabbitMQ 3 (needed for Worker)

## Getting Started

### 1. Start Infrastructure Services

```bash
docker compose up -d postgres redis rabbitmq
```

This starts:
- **PostgreSQL** on port `5432` (user: `postgres`, password: `postgres`, db: `dotnet_template`)
- **Redis** on port `6379`
- **RabbitMQ** on port `5672` (management UI: `http://localhost:15672`, user: `guest`)

### 2. Apply Database Migrations

```bash
dotnet ef database update --project src/DotnetTemplate.Infrastructure --startup-project src/DotnetTemplate.Api
```

### 3. Run the API

```bash
dotnet run --project src/DotnetTemplate.Api
```

The API starts on `http://localhost:5104` (Development profile). Swagger UI is available at `/swagger`.

### 4. Run the Worker (Optional)

```bash
dotnet run --project src/DotnetTemplate.Worker
```

The Worker runs 4 scheduled background jobs:
- **Command Outbox Poller** — every 5 seconds, publishes pending commands to RabbitMQ
- **Event Outbox Poller** — every 5 seconds, publishes pending events to RabbitMQ
- **Stale Command Checker** — every 1 minute, retries commands stuck for >5 minutes
- **Retention Cleanup** — every 1 hour, deletes successful outbox entries older than 7 days

### 5. Run with Docker Compose

```bash
docker compose up --build
```

Runs the full stack: API (port 5000), Worker, PostgreSQL, Redis, and RabbitMQ.

## Configuration

Configuration is managed through `appsettings.json` with environment-specific overrides. All settings are strongly typed via `IOptions<T>`.

### Key Configuration Sections

| Section | Settings Class | Description |
|---------|---------------|-------------|
| `App` | `AppSettings` | App name, environment, port |
| `ConnectionStrings` | — | PostgreSQL connection string |
| `Redis` | `RedisSettings` | Host, port, password, db, key prefix |
| `RabbitMq` | `RabbitMqSettings` | Host, port, vhost, credentials |
| `Jwt` | `JwtSettings` | Authority, audience, issuer, local secret |
| `RateLimit` | `RateLimitSettings` | Enable/disable rate limiting |
| `Storage` | `StorageSettings` | Provider (gcs/s3), bucket name |
| `CommandQueue` | `CommandQueueSettings` | Retry attempts, backoff, polling interval |
| `EventQueue` | `EventQueueSettings` | Retry attempts, polling interval |

### Environment Variable Overrides

Use double underscores for nested keys:

```bash
ConnectionStrings__DefaultConnection="Host=prod-db;Port=5432;..."
Redis__Host=redis.prod.internal
Jwt__Authority=https://tenant.auth0.com/
RateLimit__Enabled=true
```

## API Endpoints

### Health Check

```
GET /api/health-check          # Returns { status: "ok", timestamp: ... }
GET /health                    # Detailed health (PostgreSQL + Redis checks)
```

### External Audits (Requires Authentication)

```
GET    /api/external-audits              # List audits (paginated, filterable by type/targetId)
GET    /api/external-audits/{id}         # Get single audit
POST   /api/external-audits              # Create audit record
POST   /api/external-audits/{id}/retry   # Retry failed audit
```

## Authentication

The API supports two authentication schemes:

### JWT Bearer (Default)

Configure via the `Jwt` section in `appsettings.json`. For local development, a `LocalSecret` is used as the signing key. In production, set `Authority` to your identity provider (e.g., Auth0).

### API Key

Send `x-admin-api-key` header with the value matching `Security:AdminApiKey` in configuration. Grants the `Admin` role.

## Rate Limiting

Rate limiting is opt-in per endpoint via `[EnableRateLimiting("policyName")]` and globally disabled by default (`RateLimit.Enabled = false`).

| Policy | Limit | Window |
|--------|-------|--------|
| `default` | 100 requests | 60 seconds |
| `auth` | 5 requests | 15 minutes |
| `public` | 60 requests | 60 seconds |

## Key Patterns

### CQRS with MediatR

Commands and queries are separated using marker interfaces (`ICommand<T>`, `IQuery<T>`) and handled by MediatR. The pipeline processes every request through 4 behaviors in order:

1. **LoggingBehavior** — logs entry/exit and exceptions
2. **ValidationBehavior** — runs FluentValidation rules, throws 422 on failure
3. **TransactionBehavior** — wraps `ITransactional` requests in a database transaction
4. **DomainEventDispatcherBehavior** — publishes domain events after the handler completes

### Transactional Outbox

Commands and events are not sent directly to RabbitMQ. Instead:

1. The API writes them to outbox tables (`command_outbox`, `event_outbox`) within the same database transaction
2. The Worker polls the outbox tables and publishes messages to RabbitMQ
3. MassTransit consumers process the messages

This guarantees at-least-once delivery and avoids distributed transaction issues.

### Soft Delete

All entities extend `BaseEntity` which supports soft delete via a `DeletedAt` field. A global EF Core query filter automatically excludes soft-deleted records from all queries. Hard delete is available when permanent removal is needed.

### Audit Fields

The `AuditInterceptor` automatically sets `CreatedAt`, `UpdatedAt`, `CreatedById`, `CreatedByName`, `ModifiedById`, and `ModifiedByName` on every `SaveChanges` call, using the current authenticated user from `ICurrentUserService`.

### Prefixed UUIDs

Entity IDs are generated as `{prefix}_{guid}` (e.g., `exaud_550e8400...`). The prefix is limited to 5 characters and set in each entity's constructor for easy identification in logs and databases.

### Domain Error Hierarchy

All domain exceptions extend `DomainException` and map directly to HTTP status codes:

| Exception | Status Code |
|-----------|-------------|
| `BadRequestException` | 400 |
| `UnauthorizedException` | 401 |
| `ForbiddenException` / `PermissionDeniedException` | 403 |
| `NotFoundException` | 404 |
| `ConflictException` | 409 |
| `UnprocessableEntityException` / `ValidationException` | 422 |
| `TooManyRequestsException` | 429 |
| `OperationException` / `ConfigurationException` | 500 |
| `ServiceUnavailableException` | 503 |

The `GlobalExceptionHandler` catches these and returns structured JSON error responses with optional i18n keys.

## Testing

### Run Unit Tests

```bash
dotnet test test/DotnetTemplate.UnitTests
```

Unit tests cover domain entities, domain events, outbox entities, and MediatR pipeline behaviors using xUnit, FluentAssertions, and Moq with EF Core InMemory provider.

### Run Integration Tests

```bash
dotnet test test/DotnetTemplate.IntegrationTests
```

Integration tests use `WebApplicationFactory<Program>` with real PostgreSQL (via Testcontainers or a local instance) and test the full HTTP request pipeline.

**Requirements:** Docker must be running for Testcontainers, or set the connection string via:

```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=dotnet_template_test;Username=postgres;Password=postgres"
```

### Run All Tests

```bash
dotnet test
```

## CI/CD

The project includes a GitHub Actions workflow (`.github/workflows/ci.yml`) that:

1. Starts PostgreSQL 16 and Redis 7 as service containers
2. Restores, builds, and runs unit tests
3. Runs integration tests against the service containers

## Docker

### Multi-Stage Dockerfiles

Both the API and Worker use multi-stage builds:

- **Build stage:** `mcr.microsoft.com/dotnet/sdk:8.0` — restore, build, publish
- **Runtime stage:** `mcr.microsoft.com/dotnet/aspnet:8.0` — minimal image

### Docker Compose Services

| Service | Port | Description |
|---------|------|-------------|
| `api` | 5000 | ASP.NET Core Web API |
| `worker` | — | Background job processor |
| `postgres` | 5432 | PostgreSQL 16 |
| `redis` | 6379 | Redis 7 |
| `rabbitmq` | 5672, 15672 | RabbitMQ 3 (+ management UI) |

## Code Quality

- **TreatWarningsAsErrors** enabled globally via `Directory.Build.props`
- **Microsoft.CodeAnalysis.NetAnalyzers** for static analysis
- **EditorConfig** with rules for naming conventions (`_camelCase` private fields, `I`-prefixed interfaces), var usage, null checks, and braces
- **Nullable reference types** enabled project-wide

## NestJS Mapping Reference

This template is the .NET equivalent of a NestJS production template. Key mappings:

| NestJS | .NET |
|--------|------|
| `@nestjs/cqrs` | MediatR |
| `BullMQ` | MassTransit + RabbitMQ |
| `TypeORM` | Entity Framework Core |
| `@nestjs/throttler` | `Microsoft.AspNetCore.RateLimiting` |
| `nestjs-pino` | Serilog |
| `@nestjs/terminus` | `Microsoft.Extensions.Diagnostics.HealthChecks` |
| `passport-jwt` | `Microsoft.AspNetCore.Authentication.JwtBearer` |
| `class-validator` | FluentValidation |
| `ioredis` | StackExchange.Redis |
| `@nestjs/schedule` | Quartz.NET |
| NestJS Guards | ASP.NET Core Authorization Policies |
| NestJS Exception Filters | `IExceptionHandler` middleware |
| NestJS Interceptors | MediatR Pipeline Behaviors |
| Module System | `IServiceCollection` DI extensions |

## License

Private — All rights reserved.
