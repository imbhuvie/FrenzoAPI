# FrenZoAPI

FrenZoAPI is the backend API for **FrenZo** — a chat / messaging application.
Built with **ASP.NET Core (.NET 9)** + **Entity Framework Core** + **PostgreSQL (Npgsql)**, using a **Database-First** workflow (`dotnet ef dbcontext scaffold`).

> Template `WeatherForecast` files were removed from the project; only custom FrenZo endpoints are documented here.

## Tech Stack

- .NET 9 / ASP.NET Core Web API
- Entity Framework Core 9 + `Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4`
- PostgreSQL (`frenzodb` on `localhost:3311`)
- OpenAPI (via `Microsoft.AspNetCore.OpenApi`)

## Project Structure

```text
FrenzoAPI/
├── FrenzoAPI/
│   ├── Common/Responses/    # ApiResponse<T> envelope
│   ├── Controllers/       # Custom API controllers (template WeatherForecast removed)
│   ├── DataContext/
│   │   └── AppDbContext.cs # Scaffolded DbContext (FrenZoAPI.DataContext)
│   ├── DTOs/Auth/         # SignupRequest, LoginRequest, UserResponse
│   ├── Services/          # IAuthService + AuthService (BCrypt + EF)
│   ├── Models/            # Scaffolded entities (FrenZoAPI.Models) — 13 tables
│   ├── Program.cs
│   ├── appsettings.json
│   └── FrenzoAPI.csproj
├── docs/
│   ├── CHANGELOG.md             # Day-wise changes, commands, packages
│   ├── IMPLEMENTATION_PLAN_Auth.md # Detailed Auth plan + rationale
│   └── features/Auth.md         # Per-feature Auth spec (routes, payloads)
└── README.md
```

## Database Model (scaffolded)

Context: `AppDbContext` (`FrenZoAPI/DataContext/AppDbContext.cs`)

| Entity | Table | Description |
|--------|-------|-------------|
| `TblUser` | `tbl_users` | Core user + credentials (username / email / phone unique) |
| `UserProfile` | `user_profiles` | 1-1 profile for user (display name, bio, photo, online status) |
| `TblUserDevice` | `tbl_user_devices` | User devices / push tokens |
| `TblConversation` | `tbl_conversations` | 1-1 or group conversation |
| `TblConversationParticipant` | `tbl_conversation_participants` | Join table: users in conversations |
| `TblGroupProfile` | `tbl_group_profiles` | 1-1 group metadata for a conversation |
| `TblMessage` | `tbl_messages` | Chat messages |
| `TblMedium` | `tbl_media` | Media attachments (url, thumbnail, size, duration) |
| `TblMessageMedium` | `tbl_message_media` | Join table: messages ↔ media |
| `MstConversationType` | `mst_conversation_type` | Master: conversation types |
| `MstMessageType` | `mst_message_type` | Master: message types |
| `MstMessageStatus` | `mst_message_status` | Master: message statuses |
| `MstUserRole` | `mst_user_role` | Master: user roles |

## Getting Started

### Prerequisites

- .NET 9 SDK
- PostgreSQL running with `frenzodb` (default local: `Host=localhost;Port=3311`)
- EF tools: `dotnet tool install --global dotnet-ef`

### 1. Clone / open

```powershell
dotnet restore
```

### 2. Configure connection string

`AppDbContext` was scaffolded with `--no-onconfiguring`, so no connection string is hardcoded. Add it yourself:

`FrenzoAPI/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=3311;Database=frenzodb;Username=postgres;Password=****"
  }
}
```

Register in `FrenzoAPI/Program.cs`:
```csharp
using FrenZoAPI.DataContext;
using Microsoft.EntityFrameworkCore;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### 3. Re-scaffold (Database-First, when DB changes)

See `docs/CHANGELOG.md` for full history. Latest command (password masked):

```powershell
dotnet ef dbcontext scaffold "Host=localhost;Port=3311;Database=frenzodb;Username=postgres;Password=****" Npgsql.EntityFrameworkCore.PostgreSQL --project "D:\DotNetCodes\FrenZoAPI\FrenZoAPI\FrenZoAPI.csproj" --startup-project "D:\DotNetCodes\FrenZoAPI\FrenZoAPI\FrenZoAPI.csproj" --output-dir Models --context-dir DataContext --context AppDbContext --namespace FrenZoAPI.Models --context-namespace FrenZoAPI.DataContext --no-onconfiguring --force
```

### 4. Run

```powershell
dotnet run --project "D:\DotNetCodes\FrenZoAPI\FrenZoAPI\FrenZoAPI.csproj"
```

OpenAPI (Development only): `/openapi/v1.json`

### Testing from browser (Scalar)
```powershell
dotnet run --project "D:\DotNetCodes\FrenZoAPI\FrenZoAPI\FrenZoAPI.csproj"
```
Open `https://localhost:<port>/scalar/v1` (port shown in console) → try `POST api/auth/signup` and `POST api/auth/login` directly from the browser.

## Endpoints

Only custom FrenZo endpoints are listed here (template `WeatherForecast` removed 2026-10-10).

| Method | Route | Description | Added |
|--------|-------|-------------|-------|
| POST | `/api/auth/signup` | Register user (username + email + phone + password, BCrypt hash; tokenless) | 2026-10-10 |
| POST | `/api/auth/login` | Login with username / email / phone + password → `{ user, token, expiresAt }` (60-min JWT) | 2026-10-10 (JWT added) |

When adding an endpoint, also append it to `docs/CHANGELOG.md` under that day's `Endpoints added` section.

## Docs

- `docs/CHANGELOG.md` — day-wise log: commands executed, packages installed, files generated, endpoints added.
- `docs/IMPLEMENTATION_PLAN_Auth.md` — detailed Auth implementation plan + rationale.
- `docs/IMPLEMENTATION_PLAN_Jwt.md` — JWT step-by-step: what/why/how + implementation flow (future reference).
- `docs/features/Auth.md` — per-feature Auth spec (routes, request/response examples).
- `docs/database/01-schema.sql` — full pgAdmin schema export (source of truth).
- `docs/database/SCHEMA.md` — schema reference + timestamp-type note (UTC fix).
