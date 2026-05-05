# AGENTS.md - Pharmacy Saas

> Read this file first. It encodes every product/architecture decision made for this codebase.
> If you're about to make a choice that contradicts something here, stop and ask the user.

## Product

Offline-first Pharmacy ERP for Pakistani pharmacies. Primary platform is a **Windows desktop app** (WPF on .NET 8). Data lives in **SQL Server Express** locally; cloud API + mobile reporting + AI invoice import arrive in later phases. Source-of-truth requirements doc: [`docs/Pharmacy_Management_System_Complete_Blueprint_v2.0.md`](docs/Pharmacy_Management_System_Complete_Blueprint_v2.0.md).

## Locked-in technical decisions

| Decision | Choice | Why |
|---|---|---|
| UI framework | **WPF** (.NET 8) | Premium look, great data binding for POS grids, full ecosystem of hardware/printer libs |
| Data access | **Hybrid: EF Core + Dapper** | EF Core for admin/setup/CRUD; Dapper for POS hot paths and reports |
| Database (local) | **SQL Server Express** (default `.\SQLEXPRESS`) | Production target; LocalDB also supported via appsettings override |
| Multi-branch | **`BranchId` baked into schema from day 1** | Cheap now; painful migration later |
| Geographic scope | **Pakistan-first**, FBR **OUT OF SCOPE** | Keeps scope tight; tax engine optional/configurable per pharmacy |
| Tax engine | Optional, off by default | Pharmacy admin enables + configures rates |
| Schema migration | Numbered SQL scripts (`001_*.sql`) under `PMS.Database/Scripts/`, applied via `SqlScriptRunner` on app start, tracked in `tbl_SchemaVersion` | Mirrors blueprint Section 28 |
| Audit | Every mutation calls `IAuditService.LogAsync(...)` | Hard rule - no exceptions |
| Permissions | Every screen behind `IPermissionService.CanAccessAsync(module, permission)` | Hard rule |
| Soft delete | `IsActive = 0`; admin recycle bin restores | No hard deletes ever |
| `RecordGuid` | Every operational table has `UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID()` | Sync-ready from day 1 |
| Workflow | Code anywhere; CI builds installer on `windows-latest`; Windows PC is the only place to run/test the WPF UI | See "Build & test" below |

## Deferred decisions (do not finalize without user approval)

| Topic | Decide before |
|---|---|
| OCR engine for Phase 7 (Tesseract / Cloud AI / both) | Phase 6 starts |
| Customer mobile app scope | Phase 6.5 starts |
| Reseller/agent program | Phase 6 starts |
| E2E encrypted backup default (ON or opt-in) | Phase 6 starts |
| Pricing for Basic / Standard / Premium tiers | Before first paying customer |
| Auto-update mechanism details | Phase 6 |

## Solution layout (do not move projects without asking)

```
PharmacySaas/
  PharmacySaas.sln
  global.json                     - pins .NET SDK selection (8.0+, rollForward latestMajor)
  Directory.Build.props           - shared C# settings, version (currently 0.1.0)
  src/
    PMS.Core/                     - entities, DTOs, abstractions, constants. NO infrastructure deps
    PMS.Security/                 - BCrypt, AES-GCM, fingerprint, license signer (BCrypt + System.Management)
    PMS.Database/                 - versioned .sql scripts as embedded resources
    PMS.Data/                     - PharmacyDbContext (EF Core) + SqlConnectionFactory (Dapper) + SqlScriptRunner
    PMS.Services/                 - business logic. Depends on Core/Data/Security
    PMS.Sync/                     - foundation only; full background drain is Phase 6
    PMS.Reporting/                - foundation only; real reports are Phase 5
    PMS.Api/                      - ASP.NET Core 8. Phase 1 has placeholder controllers returning 501
    PMS.Desktop/                  - WPF app (net8.0-windows). MVVM via CommunityToolkit.Mvvm
  tests/
    PMS.Security.Tests/           - 14 tests (BCrypt, AES, signer, fingerprint)
    PMS.Services.Tests/           - 4 tests (setup flow against EF InMemory)
  installer/PMS.Setup.iss         - Inno Setup script
  .github/workflows/build-windows.yml - CI on windows-latest
  docs/
  .cursor/rules/conventions.mdc   - hard architectural rules, auto-loaded by Cursor
```

## Architecture rules (NEVER violate)

1. **Layering:** UI -> Service -> Repository / DbContext -> SQL Server. UI **never** touches DbContext or Dapper directly.
2. **Audit-on-mutation:** every state-changing service method calls `IAuditService.LogAsync(...)`.
3. **Permission gating:** every UI screen and every cloud API call checks `IPermissionService.CanAccessAsync(module, permission)`.
4. **Module gating:** premium features check `IModuleSettingsService.IsEnabledAsync(pharmacyId, moduleKey)` before doing work.
5. **No business logic in code-behind / event handlers.** Code-behind only bridges things WPF cannot bind (e.g. `PasswordBox.Password`).
6. **Soft delete:** flip `IsActive = 0`. Recycle bin restore is the only "delete reversal" path.
7. **Constants over strings:** module keys, role keys, permission keys live in `PMS.Core/Constants`. Never hardcode them in services or UI.
8. **DTOs in PMS.Core only,** so the same DTO round-trips between desktop and the future API.
9. **Pure async services.** All public service methods are `Task` / `Task<T>`. Names end in `Async`.
10. **Use `IClock`** for `DateTime.UtcNow` so tests can fake time.

## Phase 1 - what currently ships

| Capability | Status |
|---|---|
| Solution scaffold + project graph | done |
| Foundation tables (`tbl_Pharmacies`, `tbl_Branches`, `tbl_PharmacyMachines`, `tbl_Users`, `tbl_Roles`, `tbl_Permissions`, `tbl_RolePermissions`, `tbl_LicenseInfo`, `tbl_ModuleSettings`, `tbl_FeatureFlags`, `tbl_AuditLogs`, `tbl_SyncQueue`, `tbl_SyncLogs`, `tbl_RecycleBin`, `tbl_SchemaVersion`) | done |
| First-Time Setup wizard with mock activation `PHM-DEMO` / `123456` | done |
| Login (validates user + machine binding + license) | done |
| 30-day trial license + signed encrypted local file + monotonic clock counter | done |
| Modern WPF shell: top bar, sidebar, status bar, status pills | done |
| Dashboard with 6 KPI cards (live license countdown) | done |
| Settings + "Coming Next" placeholder views | done |
| Reusable theme (indigo primary), buttons, inputs, cards, layout | done |
| `IDialogService`, `INavigationService`, `IClock`, etc. | done |
| 18 tests passing on macOS, Windows CI green | done |
| Inno Setup installer auto-built on every push | done |

## Phase 1 Definition of Done (still open)

- [ ] Run installer on a fresh Windows PC with SQL Express installed.
- [ ] Setup with `PHM-DEMO` / `123456` succeeds.
- [ ] DB `PharmacySaas` is auto-created on `.\SQLEXPRESS`; `tbl_SchemaVersion` shows scripts 001 + 002 applied.
- [ ] Login on the registered machine works; license badge shows "Basic - 30 days left".
- [ ] Dashboard renders 6 KPI cards with live "License Days Left".
- [ ] Audit log records `SetupCompleted`, `LoginSuccess`, `Logout`, `LoginFailure` (try wrong password).
- [ ] Closing + reopening the app retains login session and license.
- [ ] Uninstaller removes the app while preserving `%LOCALAPPDATA%\PharmacySaas` (manual delete needed for full reset).

## Phase plan (blueprint Section 27)

| Phase | Theme | Status |
|---|---|---|
| 1 | Foundation | shipping, awaiting Windows install verification |
| 2 | Medicines + Stock foundation | next |
| 3 | Purchase module + supplier ledger | after 2 |
| 3.5 | Smart Invoice Import (mock OCR) | after 3 |
| 4 | Sales POS | after 3.5 |
| 4.5 | Hardware integration | after 4 |
| 5 | Reports + Accounts | after 4.5 |
| 5.5 | Compliance pack [PK] | **deferred** (FBR out of scope) |
| 6 | Cloud API + Admin Panel + Mobile reporting | after 5 |
| 6.5 | Loyalty / Refill reminders / Home delivery | after 6 |
| 7 | Real OCR + AI / forecasting / anomaly detection | after 6.5 |

Each phase has its own focused plan written **at the start of that phase** - do not pre-write Phase 2 plans now.

## Demo credentials (Phase 1 only)

| Field | Value |
|---|---|
| Pharmacy Code | `PHM-DEMO` |
| Setup Code | `123456` |
| Trial duration | 30 days |
| Connection (default) | `Server=.\SQLEXPRESS;Database=PharmacySaas;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;` |

These come from `PMS.Core/Constants/AppConstants.cs`. Real cloud activation lands in Phase 6.

## Build & test

| Project | macOS / Linux | Windows |
|---|:---:|:---:|
| `PMS.Core`, `PMS.Data`, `PMS.Services`, `PMS.Security`, `PMS.Sync`, `PMS.Reporting`, `PMS.Database`, `PMS.Api`, both test projects | builds + tests run | builds + tests run |
| `PMS.Desktop` (WPF) | does not build (WPF is Windows-only) | builds + runs |

```bash
# macOS / Linux
dotnet build src/PMS.Core/PMS.Core.csproj
dotnet test tests/PMS.Security.Tests
dotnet test tests/PMS.Services.Tests

# Windows
dotnet build PharmacySaas.sln
dotnet run --project src/PMS.Desktop
dotnet publish src/PMS.Desktop -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## CI / Release pipeline

`.github/workflows/build-windows.yml` runs on every push to `main`, every PR, every `v*.*.*` tag:

1. `dotnet restore` + `build` + `test` on `windows-latest`
2. `dotnet publish PMS.Desktop` self-contained single-file `win-x64`
3. Install Inno Setup via Chocolatey
4. Compile `installer/PMS.Setup.iss` -> `PharmacySaas-Setup-x.y.z.exe`
5. Upload installer + portable build + test results as artifacts
6. On tag push, attach installer to GitHub Release

Repository: https://github.com/loopstudioorg-lab/pharmacy-saas (private)

To get a fresh installer:
```bash
git push origin main          # triggers CI
gh run list --repo loopstudioorg-lab/pharmacy-saas --limit 1   # check status
# Download artifact "PharmacySaas-Setup-0.1.0" from the run page
```

## Common gotchas

- **`MSSQL$SQLEXPRESS` service must be running.** If install fails on first save, check `Get-Service MSSQL*` in PowerShell.
- **Workflow scope.** The `gh` token must include the `workflow` scope to push `.github/workflows/*.yml`. If a push is rejected with "refusing to allow an OAuth App to create or update workflow", run `gh auth refresh -h github.com -s workflow`.
- **EF Core InMemory transactions.** Tests configure `ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))` because `SetupService` opens a transaction for atomicity but InMemory cannot honor it.
- **DPI / fonts.** WPF uses Segoe UI, falls back to Inter then Arial. The app manifest sets PerMonitorV2 DPI awareness.

## Working with this codebase as an AI agent

- When the user says "Phase X", consult the table above and stay in scope. Do not pre-build later phases.
- When the user asks for a new feature/table, **always** add `RecordGuid` + audit fields + `BranchId` (where relevant) and create a new numbered SQL script.
- When you add a new screen/module, add a new entry in `ModuleKeys` + `tbl_ModuleSettings` seed defaults, plus permission entries in `002_seed_permissions.sql` (or a new numbered script).
- Never put `MessageBox.Show` calls in view models; route through `IDialogService`.
- Never put SQL strings in view models or view code-behind. Services own SQL.
- When stuck on Cursor chat continuity (e.g. moving machines), tell the user the chat history is per-machine and that **this file is the persistent context**.
