# FrenZoAPI — Day-wise Changes

This file tracks day-wise development progress, commands executed, and packages installed.

---

## 2026-10-10 — Day 1: Database-First Scaffolding with EF Core

### What was done
- Scaffolded EF Core `DbContext` + entity models from existing PostgreSQL database `frenzodb` (Database-First approach).
- Generated context as `AppDbContext` in `FrenZoAPI.DataContext` namespace (`DataContext/` folder).
- Generated 13 entity models in `FrenZoAPI.Models` namespace (`Models/` folder).
- Used `--no-onconfiguring` so connection string is NOT hardcoded in context — it must be supplied via DI / `appsettings.json`.
- Used `--force` to overwrite existing files in `Models/` and `DataContext/`.

### Command executed
> Password masked for security. Replace `****` with the real password locally.

```powershell
dotnet ef dbcontext scaffold "Host=localhost;Port=3311;Database=frenzodb;Username=postgres;Password=****" Npgsql.EntityFrameworkCore.PostgreSQL --project "D:\DotNetCodes\FrenZoAPI\FrenZoAPI\FrenZoAPI.csproj" --startup-project "D:\DotNetCodes\FrenZoAPI\FrenZoAPI\FrenZoAPI.csproj" --output-dir Models --context-dir DataContext --context AppDbContext --namespace FrenZoAPI.Models --context-namespace FrenZoAPI.DataContext --no-onconfiguring --force
```

#### Flag reference
| Flag | Meaning |
|------|---------|
| `scaffold` | Reverse-engineers DB tables into models + DbContext |
| `--output-dir Models` | Puts entities in `Models/` |
| `--context-dir DataContext` | Puts context in `DataContext/` |
| `--context AppDbContext` | Context class name |
| `--namespace FrenZoAPI.Models` | Namespace for entities |
| `--context-namespace FrenZoAPI.DataContext` | Namespace for context |
| `--no-onconfiguring` | Skips `OnConfiguring()` with hardcoded connection string |
| `--force` | Overwrites existing files |

### Packages installed / verified (`FrenzoAPI.csproj`)
| Package | Version | Purpose |
|---------|---------|---------|
| `Npgsql.EntityFrameworkCore.PostgreSQL` | `9.0.4` | PostgreSQL provider for EF Core, required for scaffold + runtime |
| `Microsoft.EntityFrameworkCore.Design` | `9.0.4` | Design-time tooling for `dotnet ef` scaffolding / migrations |
| `Microsoft.EntityFrameworkCore.Tools` | `9.0.4` | EF Core PMC / CLI tools |
| `Microsoft.AspNetCore.OpenApi` | `9.0.20` | Default template package for OpenAPI (pre-existing, not added today) |

Install commands (for reference, if setting up fresh):
```powershell
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 9.0.4
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.4
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 9.0.4
dotnet tool install --global dotnet-ef
```

### Files generated
- `FrenzoAPI/DataContext/AppDbContext.cs` — 13x `DbSet<>` + `OnModelCreating`
- `FrenzoAPI/Models/`:
  - `TblUser.cs`, `UserProfile.cs`, `TblUserDevice.cs`
  - `TblConversation.cs`, `TblConversationParticipant.cs`, `TblGroupProfile.cs`
  - `TblMessage.cs`, `TblMessageMedium.cs`, `TblMedium.cs`
  - `MstConversationType.cs`, `MstMessageType.cs`, `MstMessageStatus.cs`, `MstUserRole.cs`

### Pending / Next steps
- [ ] Add connection string to `appsettings.json` / `appsettings.Development.json`
- [ ] Register `AppDbContext` in `Program.cs` via `AddDbContext<AppDbContext>(opt => opt.UseNpgsql(...))`
- [ ] Decide on migrations vs. continued Database-First (`--force` re-scaffold) workflow
- [ ] Add custom controllers / endpoints

---

## Template for next days

```md
## YYYY-MM-DD — Day N: Title

### What was done
-

### Commands executed
```powershell
-
```

### Packages installed
| Package | Version | Purpose |
|---------|---------|---------|
| | | |

### Endpoints added
| Method | Route | Description |
|--------|-------|-------------|
| | | |

### Notes
-
```
