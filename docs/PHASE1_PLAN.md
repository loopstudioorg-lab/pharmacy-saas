---
name: Pharmacy ERP Foundation
overview: "Scaffold the offline-first Pharmacy Management System solution and ship Phase 1 (Foundation): solution structure, foundation DB tables, first-time setup with PHM-DEMO/123456, login, machine binding, license foundation, modern WPF shell, and dashboard placeholders."
todos:
  - id: scaffold
    content: "Scaffold solution: PharmacySaas.sln with PMS.Desktop (WPF), PMS.Core, PMS.Data, PMS.Services, PMS.Security, PMS.Sync, PMS.Reporting, PMS.Database, PMS.Api, plus tests/ and docs/. Add .editorconfig, .gitignore, README, copy blueprint into docs/."
    status: completed
  - id: db_foundation
    content: Write PMS.Database/001_init_foundation.sql with all foundation tables (incl. tbl_Branches and BranchId on relevant tables), RecordGuid + audit on every table. Wire tbl_SchemaVersion runner. Add EF Core DbContext + Dapper connection factory side-by-side.
    status: completed
  - id: security_services
    content: "Implement PMS.Security: BCrypt password hasher, AES encryption helper, device fingerprint service (combined signal: motherboard/CPU/disk/MachineGuid/MAC), encrypted license file with signature + monotonic clock counter."
    status: completed
  - id: core_services
    content: "Implement PMS.Services: IAuthService, IUserService, IPharmacyService, IBranchService, IMachineService, ILicenseService, IPermissionService, IAuditService. Tax engine interface stubbed (off by default)."
    status: completed
  - id: wpf_shell
    content: "Build PMS.Desktop WPF shell: theme resources (indigo/green/orange/red), reusable styles (cards, buttons, grids), MVVM base classes, message helpers, confirmation dialog, loading overlay, empty-state, top bar + sidebar + status bar."
    status: completed
  - id: setup_login_dashboard
    content: "Build screens: First-Time Setup (PHM-DEMO/123456 mock activation -> create pharmacy + default branch + machine + owner + 30-day trial), Login (machine + license validated), Dashboard with 6 KPI cards (placeholder data), Settings + Coming Next placeholders."
    status: completed
  - id: api_placeholders
    content: Scaffold PMS.Api with placeholder controllers (Auth, License, Machine, Sync, Backup, Reports) - no implementation, returns 501. Confirms cloud structure exists for later phases.
    status: completed
  - id: tests_smoke
    content: Add xUnit projects with smoke tests for password hashing, license signing, fingerprint service, and a setup-flow integration test against LocalDB.
    status: completed
  - id: ci_installer
    content: "Add GitHub Actions workflow (.github/workflows/build-windows.yml) running on windows-latest: dotnet publish PMS.Desktop self-contained win-x64, install Inno Setup via choco, compile installer/PMS.Setup.iss, upload installer .exe as workflow artifact and attach to GitHub Release on tag push."
    status: completed
  - id: inno_script
    content: "Author installer/PMS.Setup.iss (Inno Setup script): app metadata, install dir under Program Files, Start Menu shortcut, desktop shortcut option, uninstaller, LocalDB prerequisite check note, version pulled from .csproj."
    status: completed
  - id: phase1_dod
    content: "Run Phase 1 DoD checklist on Windows machine: download installer artifact from GitHub Actions, run installer, complete setup, close+reopen, login on registered machine, verify license countdown, verify permission hides menu, verify audit log entries, run uninstaller cleanly."
    status: pending
isProject: false
---

# Pharmacy ERP - Foundation Plan (Phase 1)

## Confirmed decisions

- **Stack:** .NET 8, **WPF** desktop, **SQL Server LocalDB** (dev) / **Express** (prod)
- **Data access:** Hybrid - **EF Core 8** for admin/setup/CRUD, **Dapper** for hot paths (POS, reports) - both wired from day 1
- **Workflow:** Code on macOS in Cursor at `/Users/farhan/Desktop/Pharmacy Saas`, push to Git, pull and build/run/test on Windows machine
- **Multi-branch:** `BranchId` column included on all relevant tables from day 1 (no UI for it in Phase 1, schema-only)
- **Geographic scope:** Pakistan-first. **FBR integration is OUT OF SCOPE** for the foreseeable future. **Tax engine is optional + configurable** (admin can enable/disable, define rates per pharmacy)
- **Deferred to later phases:** OCR engine choice, customer mobile app, reseller program, E2E backup default, pricing tiers

## Solution structure

Will scaffold at `/Users/farhan/Desktop/Pharmacy Saas`:

```
PharmacySaas/
  PharmacySaas.sln
  src/
    PMS.Desktop/        WPF app (entry point)
    PMS.Core/           Entities, DTOs, enums, interfaces, constants
    PMS.Data/           DbContext (EF), Dapper repos, UoW
    PMS.Services/       Auth, License, Users, Setup, Modules
    PMS.Security/       Hashing, encryption, fingerprint, license signing, permissions
    PMS.Sync/           Sync queue + API client (foundation only)
    PMS.Reporting/      Report models, export helpers (foundation only)
    PMS.Database/       Versioned SQL scripts (001_init.sql, ...)
    PMS.Api/            ASP.NET Core Web API (placeholder controllers only)
  tests/
    PMS.Services.Tests/
    PMS.Security.Tests/
  installer/
    PMS.Setup.iss       Inno Setup installer script
  .github/
    workflows/
      build-windows.yml CI: build + publish + Inno Setup installer artifact
  docs/
    Pharmacy_Management_System_Complete_Blueprint_v2.0.md   (copy of source blueprint)
  .editorconfig
  .gitignore
  README.md
```

## Phase 1 - Foundation (the only thing we build now)

### Database (foundation tables, all with `RecordGuid`, audit fields, `BranchId` where relevant)

- `tbl_Pharmacies`
- `tbl_Branches` (NEW - schema only, default branch auto-created)
- `tbl_PharmacyMachines`
- `tbl_Users`, `tbl_Roles`, `tbl_Permissions`, `tbl_RolePermissions`
- `tbl_LicenseInfo`
- `tbl_ModuleSettings` (with all module flags from Section 9 stubbed off)
- `tbl_SyncQueue`, `tbl_SyncLogs`
- `tbl_AuditLogs`
- `tbl_FeatureFlags`, `tbl_SchemaVersion`, `tbl_RecycleBin`

Schema applied via numbered SQL scripts (`001_init_foundation.sql`) tracked by `tbl_SchemaVersion`. EF Core migrations layered on top for code-first changes after that.

### Services (all in `PMS.Services` / `PMS.Security`)

- `IAuthService`, `IUserService`, `IPharmacyService`, `IBranchService`
- `IMachineService`, `ILicenseService`
- `IPermissionService`, `IAuditService`
- `IPasswordHasher` (BCrypt), `IDeviceFingerprintService`, `IEncryptionService`

### Screens (WPF, MVVM, reusable styles)

1. **First-Time Setup** - prompt for Pharmacy Code (`PHM-DEMO`) + Setup Code (`123456`) -> mock activation -> create local pharmacy + default branch -> register machine -> create owner user -> 30-day trial license
2. **Login** - validates user, machine binding, license status
3. **Shell** - top bar (pharmacy/user/online-offline/license badge), sidebar, content area, status bar
4. **Dashboard** - KPI cards: Today Sales, Today Profit, Stock Value, Low Stock, Near Expiry, License Days Left (all wired with placeholder zero-state data)
5. **Settings** - placeholder
6. **Coming Next** - placeholder for any sidebar item not yet built

### Reusable UX (built once, used everywhere)

- Message helpers: `ShowSuccess`, `ShowError`, `ShowWarning`
- Confirmation dialog
- Loading overlay
- Card/panel/button/grid styles (theme: indigo primary, green/orange/red status)
- Empty-state component
- Global search component (foundation)

### Hard rules baked in from day 1

- UI never touches DB (UI -> Service -> Repo -> DB)
- Every mutation calls `IAuditService.Log(...)`
- Every screen behind `IPermissionService.CanAccess(module, permission)`
- Every table has `RecordGuid UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID()`
- Soft-delete (`IsActive = 0`) - no hard deletes; recycle bin restores
- Tax engine OFF by default, configurable per pharmacy (no FBR)

### Cross-platform dev workflow

- Solution targets `.NET 8` and is `dotnet build`-able on macOS for non-WPF projects (Core, Services, Security, Data, Api, tests)
- `PMS.Desktop` is `<TargetFramework>net8.0-windows</TargetFramework>` + `<UseWPF>true</UseWPF>` - only builds on Windows. `.gitignore` and `Directory.Build.props` set up so Mac IDE doesn't error
- Git workflow: commit on Mac, push to GitHub, GitHub Actions builds installer on Windows runner, you download installer to test

### CI + Installer (new in plan)

- **`.github/workflows/build-windows.yml`** runs on every push to `main` and on tag push:
  1. Checkout
  2. Setup .NET 8
  3. `dotnet restore` and `dotnet build -c Release`
  4. `dotnet test` (tests projects only)
  5. `dotnet publish src/PMS.Desktop -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true`
  6. Install Inno Setup via Chocolatey (`choco install innosetup -y`)
  7. Run `iscc.exe installer/PMS.Setup.iss` to produce `PharmacySaas-Setup-x.y.z.exe`
  8. Upload installer as workflow artifact (always)
  9. On tag push (e.g. `v0.1.0`), attach installer to GitHub Release
- **`installer/PMS.Setup.iss`** (Inno Setup):
  - App name: Pharmacy Saas
  - Default install dir: `{autopf}\PharmacySaas`
  - Start Menu group + optional desktop shortcut
  - Uninstaller (with option to keep local DB on uninstall)
  - Version metadata read from `PMS.Desktop.csproj`
  - Note in installer: "Requires SQL Server LocalDB or Express - install separately if not present" (we won't auto-install LocalDB in Phase 1; that's a Phase 6 polish item)
- **Testing flow for you:** Push to `main` -> wait ~3-5 min -> download `PharmacySaas-Setup-x.y.z.exe` from the Actions run artifacts on GitHub -> run on Windows -> test as a real user. No Visual Studio required to test.
- **Prerequisite for this to work:** code must live in a GitHub repository (public or private). If the repo doesn't exist yet, I'll initialize it locally and you'll create the GitHub remote and push.

### Definition of Done (Phase 1)

- GitHub Actions workflow produces a working `PharmacySaas-Setup-x.y.z.exe` installer artifact on every push
- Installer installs cleanly on a fresh Windows machine, creates Start Menu shortcut
- Setup with `PHM-DEMO` / `123456` works on first launch
- 30-day trial license is created and visible in top bar
- Closing and reopening preserves login session correctly
- Login only works on the registered machine
- Permissions can hide a sidebar menu item
- Audit log records setup, login, license creation
- Dashboard shows all 6 KPI cards with license days countdown live
- Uninstaller removes the app (option to keep local DB)

### What is NOT in Phase 1

Per blueprint Section 27: no medicines, no sales, no purchases, no real reports, no real sync, no AI invoice import, no hardware integration, no compliance pack, no cloud API beyond placeholder controllers. Each of those is a dedicated future phase.

## After Phase 1

We follow the blueprint's phase order strictly (DoD must pass before next phase starts):

`Phase 2` Medicines + Stock -> `Phase 3` Purchase -> `Phase 3.5` Smart Invoice Import (mock) -> `Phase 4` Sales POS -> `Phase 4.5` Hardware -> `Phase 5` Reports + Accounts -> `Phase 5.5` skipped/deferred (FBR out of scope) -> `Phase 6` Cloud + Mobile + Engagement -> `Phase 6.5` Loyalty/Delivery -> `Phase 7` AI / Real OCR.

Each phase will get its own focused plan when we get there.

