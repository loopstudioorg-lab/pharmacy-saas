# Pharmacy Saas

Offline-first Pharmacy ERP. C# desktop app (WPF, .NET 8) backed by SQL Server LocalDB / Express, with a future ASP.NET Core cloud API and mobile reporting app.

> Phase 1 (Foundation) is implemented. Medicines, sales, purchases, AI invoice import, hardware integration, reports, and cloud sync arrive in subsequent phases. See [`docs/Pharmacy_Management_System_Complete_Blueprint_v2.0.md`](docs/Pharmacy_Management_System_Complete_Blueprint_v2.0.md).

## Solution layout

| Project | Purpose |
|---|---|
| [`src/PMS.Desktop`](src/PMS.Desktop) | WPF desktop app (entry point) |
| [`src/PMS.Core`](src/PMS.Core) | Entities, DTOs, enums, interfaces, constants |
| [`src/PMS.Data`](src/PMS.Data) | EF Core DbContext, Dapper connection factory, schema runner |
| [`src/PMS.Services`](src/PMS.Services) | Setup, Auth, Users, Pharmacy, Branch, Machine, License, Modules, Permissions, Audit, Dashboard |
| [`src/PMS.Security`](src/PMS.Security) | BCrypt hashing, AES-GCM, device fingerprint, license signer |
| [`src/PMS.Sync`](src/PMS.Sync) | Sync queue writer (foundation only - background drain in Phase 6) |
| [`src/PMS.Reporting`](src/PMS.Reporting) | Report base types (foundation only) |
| [`src/PMS.Database`](src/PMS.Database) | Versioned `.sql` scripts shipped as embedded resources |
| [`src/PMS.Api`](src/PMS.Api) | ASP.NET Core placeholder controllers - real cloud lands in Phase 6 |
| [`tests/PMS.Security.Tests`](tests/PMS.Security.Tests) | xUnit smoke tests for hashing, AES, license signing, fingerprint |
| [`tests/PMS.Services.Tests`](tests/PMS.Services.Tests) | xUnit integration tests for the setup flow |

## Phase 1 - what works

- Solution scaffold with hard architectural rules baked in (UI never touches DB; every mutation audited; permission-gated screens; soft-delete; `RecordGuid` everywhere; `BranchId` reserved across the schema).
- Foundation DB tables (`tbl_Pharmacies`, `tbl_Branches`, `tbl_Users`, `tbl_Roles`, `tbl_Permissions`, `tbl_RolePermissions`, `tbl_PharmacyMachines`, `tbl_LicenseInfo`, `tbl_ModuleSettings`, `tbl_FeatureFlags`, `tbl_AuditLogs`, `tbl_SyncQueue`, `tbl_SyncLogs`, `tbl_RecycleBin`, `tbl_SchemaVersion`) applied via numbered SQL scripts.
- First-Time Setup with mock activation (`PHM-DEMO` / `123456`).
- Login that validates user, machine binding, and license status.
- License foundation: trial, signed local license file (encrypted), grace period, monotonic clock counter.
- Modern WPF shell: top bar, sidebar, dashboard with 6 KPI cards, settings, "Coming Next" placeholder.
- Reusable theme + components (cards, buttons, inputs, dialogs).
- Placeholder REST API.

## Workflow: code on macOS, run on Windows

The desktop app uses WPF, which **only builds and runs on Windows**. Code on macOS in Cursor; push to Git; build/run/test on a Windows machine.

| Project | Builds on macOS? | Builds on Windows? |
|---|:---:|:---:|
| `PMS.Core`, `PMS.Data`, `PMS.Services`, `PMS.Security`, `PMS.Sync`, `PMS.Reporting`, `PMS.Database`, `PMS.Api`, both test projects | Yes | Yes |
| `PMS.Desktop` (WPF) | No (WPF requires Windows) | Yes |

## Prerequisites

| Tool | Version |
|---|---|
| .NET SDK | 8.0 (LTS recommended) - [download](https://dotnet.microsoft.com/download/dotnet/8.0) |
| Visual Studio 2022 | Community+ with `.NET desktop development` workload (Windows machine) |
| SQL Server LocalDB or Express | LocalDB ships with VS; standalone download [here](https://aka.ms/get-localdb) (Windows machine) |
| Inno Setup 6 | only required for local installer build, CI installs it automatically |

## Local commands

```bash
# Build everything except the WPF project on macOS / Linux
dotnet build src/PMS.Core/PMS.Core.csproj
dotnet build src/PMS.Services/PMS.Services.csproj
dotnet build src/PMS.Api/PMS.Api.csproj

# Run tests on macOS / Linux (uses .NET InMemory provider, no SQL needed)
dotnet test tests/PMS.Security.Tests
dotnet test tests/PMS.Services.Tests

# Build the entire solution including PMS.Desktop (Windows only)
dotnet build PharmacySaas.sln

# Run the desktop app from Visual Studio (F5) or:
dotnet run --project src/PMS.Desktop

# Produce a self-contained .exe (Windows only)
dotnet publish src/PMS.Desktop -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## CI: GitHub Actions builds the installer

Every push to `main` (and every PR, and every `v*.*.*` tag) triggers [.github/workflows/build-windows.yml](.github/workflows/build-windows.yml). The workflow:

1. Restores + builds `PharmacySaas.sln` on a `windows-latest` runner.
2. Runs the test suite.
3. `dotnet publish` of `PMS.Desktop` as self-contained, single-file `win-x64`.
4. Installs Inno Setup via Chocolatey.
5. Compiles [`installer/PMS.Setup.iss`](installer/PMS.Setup.iss) into `PharmacySaas-Setup-x.y.z.exe`.
6. Uploads the installer + portable build as workflow artifacts.
7. On `v*.*.*` tag push, attaches the installer to a GitHub Release.

To test a build:
1. Open the latest run on the Actions tab in GitHub.
2. Download the `PharmacySaas-Setup-x.y.z` artifact.
3. Run the installer on your Windows test machine.
4. Launch from Start Menu - the First-Time Setup screen appears with `PHM-DEMO` / `123456` pre-filled.

## First-Time Setup (Phase 1 demo flow)

| Field | Value |
|---|---|
| Pharmacy Code | `PHM-DEMO` |
| Setup Code | `123456` |
| Pharmacy Name | (your choice) |
| Owner Username | (your choice) |
| Owner Password | (at least 6 chars) |

After setup, a 30-day trial is created. Login on the same machine; license countdown shows in the top bar.

## Phase 1 Definition of Done

- [ ] `windows-latest` CI run produces `PharmacySaas-Setup-x.y.z.exe` on every push.
- [ ] Installer installs cleanly on a fresh Windows machine and adds a Start Menu shortcut.
- [ ] `PHM-DEMO` / `123456` setup flow creates pharmacy + default branch + owner user + 30-day trial.
- [ ] Login restricted to the registered machine.
- [ ] Closing and reopening preserves the saved DB and license.
- [ ] Top bar shows live "X days left" countdown.
- [ ] Audit log records setup, license activation, login success, login failure, logout.
- [ ] Uninstaller removes the app while preserving the local DB folder under `%LOCALAPPDATA%\PharmacySaas`.

## Conventions

- Layered architecture: UI -> Service -> Repository -> DB. UI never touches DB.
- Every table prefixed `tbl_`, with `RecordGuid UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID()` and standard audit fields.
- Every mutation calls `IAuditService.LogAsync(...)`.
- Every screen behind `IPermissionService.CanAccessAsync(module, permission)`.
- Soft-delete by default (`IsActive = 0`); admin recycle bin restores.
- `BranchId` reserved on relevant tables from day 1 (multi-branch decision).
- Tax engine off by default; configurable per pharmacy. **No FBR integration in scope** for the foreseeable future.
- DTOs in `PMS.Core` are shared between desktop and the future API.

## License

Proprietary. All rights reserved.
