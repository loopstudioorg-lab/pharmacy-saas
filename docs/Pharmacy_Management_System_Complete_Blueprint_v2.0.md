# PHARMACY MANAGEMENT SYSTEM
## Complete Technical, UX and Execution Blueprint - Version 2.0

**Document type:** Internal blueprint for developers + stakeholders
**Product:** Offline-first pharmacy ERP with modular cloud, machine binding, AI invoice import
**Stack:** C# Desktop (WinForms or WPF) + SQL Server LocalDB/Express + ASP.NET Core Web API + future Mobile App
**Method:** Foundation first, then module by module, with reusable services and components

---

## TABLE OF CONTENTS

PART I - VISION & ARCHITECTURE
1. Executive Summary
2. System Architecture
3. Product Positioning, Plans & Pricing

PART II - SECURITY, IDENTITY & LICENSING
4. Pharmacy / User / Machine Security Model
5. Signup, Activation & First-Time Setup
6. Machine Binding & Auto Configuration
7. License, Subscription & Offline Renewal
8. Security Implementation Details

PART III - PRODUCT MODULES
9. Module-Based Product Scope
10. Medicine Master
11. Sales POS
12. Smart Invoice Import / AI Invoice Reader (NEW)
13. Hardware Integration
14. Compliance Pack (Pakistan)

PART IV - UX/UI SYSTEM
15. UX/UI Design System
16. Reusable UX Components

PART V - DATABASE & CLOUD
17. Database Blueprint
18. Cloud API & Admin Panel
19. Sync & Backup

PART VI - ROADMAP, AI & ENGAGEMENT
20. Development Roadmap
21. AI / Intelligence Layer
22. Customer Engagement
23. SaaS Business Layer
24. Quality Hardening

PART VII - STEP-BY-STEP EXECUTION GUIDE
25. Prerequisites & Environment Setup
26. Day-1 Bootstrap (Cursor Command)
27. Phase-by-Phase Execution Plan
28. Coding Conventions & Anti-Patterns
29. Testing Strategy
30. Release & Deployment
31. Team Workflow with Cursor
32. Risks & Mitigations

APPENDICES
A. All Database Tables Cheat Sheet
B. Module Flags & License Tier Map
C. Cursor Prompt Library
D. Glossary
E. Open Decisions for Stakeholder Discussion
F. Change Log v1 -> v2

---

# PART I - VISION & ARCHITECTURE

## 1. Executive Summary

This product is **not a simple pharmacy POS**. It is an **offline-first pharmacy ERP** for local pharmacies that need fast sales, purchases, stock, expiry, profit, and secure subscription control. One desktop installer serves every customer; modules (cloud backup, mobile reporting, WhatsApp reports, multi-branch, AI invoice import, advanced reports) are switched on/off per pharmacy via licensing.

**Core idea:** Desktop must work 100% offline for sales and store operations. Cloud API is only for license validation, backup, sync, admin control, mobile reporting, and AI invoice extraction.

**Positioning:**
- Fast offline pharmacy software with cloud backup, expiry alerts, narcotics register, cold-storage tracking, owner mobile reporting, and AI-driven invoice import.
- Built for shopkeepers who need speed, reliability, and low training time.
- SaaS model: Basic (offline) / Standard (cloud backup) / Premium (mobile + AI + WhatsApp + multi-branch).

**Success requirements:**
- Sales, purchase, stock, users, reports, backups all work without internet.
- Each user belongs to exactly one pharmacy. Each machine belongs to exactly one pharmacy. No public pharmacy list anywhere.
- Trial, expiry alerts, online validation, offline grace period, offline renewal codes.
- Premium, clean, fast, shortcut-driven UI for non-technical shopkeepers.
- Cloud API and mobile app planned from day one without forcing every customer to use them.

## 2. System Architecture

```
Desktop App (Offline First)
    |
    v
Local SQL Server Express / LocalDB
    |
    v
Background Sync Queue (optional, license-gated)
    |
    v
ASP.NET Core Web API (Cloud)
    |
    v
Cloud SQL Server Database
    |
    v
Admin Panel  +  Mobile Reporting App  +  AI Invoice OCR Service
```

**Solution projects:**

| Project | Purpose | Phase 1? |
|---|---|---|
| PMS.Desktop | Main offline desktop app | Build now |
| PMS.Core | Entities, DTOs, enums, interfaces, constants | Build now |
| PMS.Data | Repositories, DB helpers, unit-of-work, SQL scripts | Build now |
| PMS.Services | Business logic - auth, license, users, setup, modules | Build now |
| PMS.Security | Hashing, encryption, fingerprint, license signing, permissions | Build now |
| PMS.Sync | Sync queue, API client, retry logic | Foundation now, full sync later |
| PMS.Reporting | Report models, export helpers | Foundation now |
| PMS.Database | Versioned SQL scripts | Build now |
| PMS.Api | Cloud API | Placeholders now, implement after desktop |
| PMS.AdminPanel | Super admin portal | Plan now, build after API |
| Mobile App | Owner reporting + alerts | Later phase |

## 3. Product Positioning, Plans & Pricing

| Plan | Includes |
|---|---|
| Basic | Offline POS, purchases, medicines, stock, basic reports, local backup |
| Standard | Basic + cloud backup, advanced expiry alerts, advanced reports, Smart Invoice Import (offline OCR) |
| Premium | Standard + mobile app reporting, WhatsApp reports, multi-user/multi-branch, Cloud AI Invoice Reading, anomaly detection, sales forecasting |

---

# PART II - SECURITY, IDENTITY & LICENSING

## 4. Pharmacy / User / Machine Security Model

**Most important rule:** users never select a pharmacy from a public list. Pharmacy connection is done through controlled setup codes, admin login, machine registration, and license validation.

```
Pharmacy Account
   |- Owner / Admin User
   |- Staff Users
   |- Licensed Machines
   |- Module Settings
   |- Local Database
   |- Subscription / License
```

**Hard rules:**
- One user belongs to exactly one pharmacy.
- One machine belongs to exactly one pharmacy.
- Staff cannot create a pharmacy account.
- No pharmacy dropdown / visible list anywhere in UI.
- A machine must be registered before login is allowed; otherwise system shows the setup / activation screen.
- Pharmacy admin creates staff users from inside the desktop app.
- Super admin can activate / suspend / renew / change modules from the central admin panel.

**Identity tables:** `tbl_Pharmacies`, `tbl_PharmacyMachines`, `tbl_Users`, `tbl_Roles`, `tbl_Permissions`, `tbl_RolePermissions`, `tbl_AuditLogs`.

## 5. Signup, Activation & First-Time Setup

Open public signup is forbidden. Use **invite/code-based activation**.

**New pharmacy flow:**
1. You create the pharmacy in the Super Admin Panel.
2. System generates Pharmacy Code + Owner Setup Code.
3. Customer installs desktop app.
4. Customer enters Pharmacy Code + Setup Code.
5. System verifies via API (or mock service in Phase 1).
6. Local pharmacy record is created.
7. Current machine is auto-registered to that pharmacy.
8. Customer creates owner/admin username + password.
9. Trial/subscription license is created locally.
10. User is redirected to login -> dashboard.

**Phase 1 dev test values:**
- Pharmacy Code: `PHM-DEMO`
- Setup Code: `123456`
- Trial: 30 days

**Staff user flow:** Admin logs in -> Settings -> Users -> Add User -> assign role/permissions. Staff can only log in on machines registered to the same pharmacy.

## 6. Machine Binding & Auto Configuration

Use a **combined device fingerprint with tolerance** - never rely on a single hardware value.

| Signal | Use |
|---|---|
| Motherboard serial | Strong, may be missing |
| CPU ID | Useful, not always accessible |
| Disk serial | Good, can change if drive replaced |
| Windows Machine GUID | Useful OS-level signal |
| MAC Address | Weak alone, secondary |
| Machine Name + Windows User | Display + support |

**Auto-configure new machine (online):** install -> enter Pharmacy Code -> enter Admin username/password -> system checks subscription + machine limit -> register machine -> download settings + modules -> restore latest cloud backup if enabled -> ready.

**Machine replacement:** if limit reached -> "Replace existing machine" -> admin login -> show old machine names (NOT pharmacy list) -> deactivate old -> activate new.

**Offline new machine activation:** desktop shows New Machine Request Code -> customer sends to support -> super admin generates Offline Activation Code -> customer enters -> app validates signature offline -> machine registered locally.

**Admin fleet view (NEW v2):** super admin sees per-machine last sync, last login, OS version, app version, ability to remote-logout / deactivate.

## 7. License, Subscription & Offline Renewal

**Goals:** 15 or 30 day trial, alerts at 7/3/1 days + expired, controlled offline grace period, offline renewal via secure code, modules controlled from admin.

**License validation flow:**
```
App Start
  -> Load encrypted local license
  -> Validate signature
  -> Check pharmacy + machine match
  -> Check expiry + grace period
  -> If internet, validate with API
  -> Save updated modules locally
  -> Allow / restrict access
```

**Offline renewal:** app shows Machine Code + Pharmacy Code -> customer sends -> super admin generates Renewal Code (PharmacyID + MachineID + NewExpiry + Modules + Signature) -> customer enters -> app validates offline -> license extended.

**NEW v2 additions:**
- Subscription auto-billing hooks (Stripe / local gateway).
- Suspend-on-failed-payment workflow.
- Reseller/agent commission tracking.
- Grace-period UX states clearly shown in top bar.

## 8. Security Implementation Details

**Passwords:**
- Never store plain. Use PBKDF2 / BCrypt / Argon2.
- Store `PasswordHash` + `PasswordSalt`.
- Account lockout after N failed attempts.
- Admin-driven password reset only (no public reset).

**Local license:**
- Encrypted license file + DB mirror for fast access.
- File contains PharmacyID, MachineID, Expiry, Modules, Signature.
- Track `LastValidUtc` + monotonic counter to detect clock rollback.

**Permission service:**
```
CanAccess(moduleKey, permissionKey)
  -> Check user
  -> Check role permissions
  -> Check module enabled by license
  -> Check machine active
  -> Allow / Deny
```

**Audit events to track:** login success/failure, license activation/renewal, machine register/replace, user creation/role change, sale delete/return/discount change, stock adjustment, backup restore, invoice import + conversion.

**NEW v2:**
- 2FA for Owner role via WhatsApp/SMS OTP.
- Soft-delete + recycle bin (admin restore).
- Per-pharmacy password policy config.

---

# PART III - PRODUCT MODULES

## 9. Module-Based Product Scope

One installer, one codebase. Features are toggled by module flags + license + permissions.

| Module | Main Features | License-Controlled |
|---|---|---|
| Dashboard | Sales, profit, stock value, alerts, quick actions | Core |
| Sales POS | Barcode sale, search, invoice, returns, hold, narcotics | Core |
| Medicines | Master, generic, category, company, refrigerator, narcotics, printable | Core |
| Purchase | Supplier invoice, batch, expiry, bonus qty, price, payments | Core |
| Stock | Batch stock, short stock, expiry, adjustment, damaged | Core |
| Accounts | Expenses, cashbook, customer/supplier ledgers | Plan |
| Reports | Sales, purchase, profit, stock, expiry, narcotics, cashier | Plan |
| Backup | Local + auto + cloud | Cloud optional |
| Sync | Background sync queue, retry, logs | Online |
| Mobile Reporting | Owner app reports + alerts | Premium |
| WhatsApp Reports | Daily auto-report | Premium |
| **Smart Invoice Import** | OCR/AI invoice -> auto purchase entry | Standard+ |
| **Cloud AI Invoice Reading** | Online AI extraction (better accuracy) | Premium |
| Prescription Module | Image attach, OCR-to-cart | Premium |
| Drug Interactions | Allergy + interaction warnings | Premium |
| Refill Reminders | WhatsApp/SMS chronic-med reminders | Premium |
| Cold Chain Logging | Daily fridge temperature log | Standard+ |
| Drug Recall | Recall flag + customer-buyer list | Standard+ |
| Insurance / Panels | Panel master, copay, monthly billing | Premium |
| Home Delivery | Order intake + rider assignment | Premium |
| Loyalty Points | Points earn/redeem | Premium |
| Forecasting | Per-item sales forecast -> auto PO | Premium |
| Anomaly Detection | Pilferage detection signals | Premium |
| FBR POS [PK] | Tier-1 retailer FBR integration | Premium PK |
| Barcode Label Printing | Avery + roll printer | Standard+ |
| Shift Management | Open/close shift + cash variance | Standard+ |

## 10. Medicine Master

**Required fields:**

| Group | Fields |
|---|---|
| Basic | MedicineName, GenericName, Formula, Company, Category, Packing, Barcode |
| Pricing | PurchasePrice, SalePrice, RetailPrice, ProfitMargin, **MRP** [PK] |
| Stock rules | ReorderLevel, MinStock, MaxStock |
| Batch rules | BatchNo, ExpiryDate, ManufacturingDate (if needed) |
| Special | IsRefrigeratorItem, IsNarcoticsItem, IsPrintable, **Schedule (G/H/Controlled) [PK]**, **IsChronicMed**, **Synonyms (CSV)**, **InteractionGroupIds (CSV)**, **NameUrdu**, **PhotoPath** |
| Audit | SaveDate, SavedBy, UpdatedDate, UpdatedBy, IsActive, IsSynced, RecordGuid |

**Special-option behaviour:**
- Refrigerator item -> show cold-storage indicator on purchase/sale/stock; appears in refrigerator stock report.
- Narcotics item -> on sale, popup asks Doctor Name + Prescription Remarks; included in Narcotics Sale Report; audit log captures user + machine.
- Printable / Non-printable -> non-printable items save in sale but hidden from printed invoice if configured.
- Synonyms -> drives typo-tolerant search at POS and matching at Smart Invoice Import.
- Chronic med -> drives refill-reminder scheduling.

## 11. Sales POS

POS is the money screen. Fast for counter use, simple enough for a cashier to learn quickly.

**Layout:**
- Top: barcode/search + customer + payment shortcuts.
- Left: search results with stock, batch, expiry, price.
- Center: cart / invoice items.
- Right: summary panel - totals, discount, payment, action buttons.
- Bottom: keyboard shortcut bar.

**Keyboard shortcuts:**

| Key | Action |
|---|---|
| F1 | New Sale |
| F2 | Add / Select Customer |
| F4 | Hold Bill |
| F6 | Payment |
| F8 | Return Sale |
| F9 | Print Invoice |
| ESC | Cancel / Close popup |
| Ctrl+B | Focus barcode |

**POS rules:**
- Search by name, barcode, formula, generic, company, batch, **synonym**.
- Expired products in red; sale blocked by default.
- Near-expiry in orange with confirmation.
- Auto-select batch via FEFO (First Expiry First Out).
- Warn on stock shortage before saving.
- Allow hold + recall (multiple holds with names - NEW v2).
- Print invoice only after successful local save.

**NEW v2 additions:**
- Prescription image attach to sale.
- Drug interaction & customer allergy warnings.
- Generic substitute suggestion panel.
- Multi-payment split (Cash + JazzCash + Card on one bill) [PK].
- Customer-display screen support (secondary monitor).
- Discount approval workflow (cashier requests >X% -> manager PIN approves).
- E-receipt via WhatsApp / Email with QR.
- Lost-sale register (record asked-but-not-stocked items).

## 12. Smart Invoice Import / AI Invoice Reader (NEW MODULE)

### Goal
User uploads a supplier invoice image or PDF on the Purchase screen. System OCR/AI-extracts line items (product name, quantity, batch, expiry, purchase price, sale price, total). System matches each item against the medicine master and shows a colour-coded review grid:
- **Green** = exact match
- **Yellow** = partial match, needs user confirmation
- **Red** = no match

User confirms / re-links / creates new medicines from the screen. On final confirm, the system auto-creates a Purchase Invoice and updates batch stock - eliminating the biggest manual data-entry pain in pharmacy operations.

### Phase split
- **Phase 1 (Phase 3.5 in roadmap)**: build module structure, DB tables, service interfaces, DTOs, matching engine foundation, UI placeholder. Ship a `MockInvoiceExtractionService` that returns canned JSON so the rest of the pipeline (matching + review + conversion to purchase) works end-to-end without real OCR.
- **Phase 7**: plug in real extractors (`TesseractInvoiceExtractionService` offline, `CloudAIInvoiceExtractionService` online) without touching matching / review / conversion code.

### Module flags (in `tbl_ModuleSettings`)
- `EnableSmartInvoiceImport` - master on/off.
- `EnableCloudAIInvoiceReading` - online cloud AI vs offline-only OCR.
- `EnableAutoCreateMedicineFromImport` - admin-controlled gate on creating new medicines from the import screen (prevents staff from polluting master).

### License gating
- `SmartInvoiceImport` -> Standard / Premium.
- `CloudAIInvoiceReading` -> Premium only (consumes cloud AI credits).

### Database tables

`tbl_InvoiceImports` (header):

| Column | Type | Notes |
|---|---|---|
| Id | INT IDENTITY PK | |
| RecordGuid | UNIQUEIDENTIFIER | Sync key |
| PharmacyId | INT FK | |
| SupplierId | INT NULL FK | Resolved after extraction |
| SourceFileName | NVARCHAR(255) | |
| SourceFileType | NVARCHAR(20) | image / pdf |
| SourceFilePath | NVARCHAR(500) | Encrypted local store |
| SourceFileHash | NVARCHAR(64) | Dedupe |
| ExtractionEngine | NVARCHAR(30) | Mock / Tesseract / CloudAI |
| ExtractionStatus | NVARCHAR(20) | Pending / Extracting / Extracted / Reviewing / Confirmed / Converted / Failed / Cancelled |
| ExtractedAtUtc | DATETIME NULL | |
| RawExtractedJson | NVARCHAR(MAX) | Full AI response for audit |
| InvoiceNumberExtracted | NVARCHAR(50) | |
| InvoiceDateExtracted | DATE NULL | |
| SupplierNameExtracted | NVARCHAR(200) | |
| TotalAmountExtracted | DECIMAL(18,2) | |
| ConfidenceScore | INT | 0-100 |
| ConvertedToPurchaseId | INT NULL FK | tbl_Purchases |
| ReviewedBy | INT NULL FK | tbl_Users |
| ReviewedAtUtc | DATETIME NULL | |
| Notes | NVARCHAR(500) | |
| IsActive, SaveDate, SavedBy, UpdatedDate, UpdatedBy, IsSynced | std audit | |

`tbl_InvoiceImportItems` (lines):

| Column | Type | Notes |
|---|---|---|
| Id | INT IDENTITY PK | |
| RecordGuid | UNIQUEIDENTIFIER | |
| InvoiceImportId | INT FK | |
| LineNumber | INT | |
| ExtractedProductName | NVARCHAR(200) | |
| ExtractedPackSize | NVARCHAR(50) | |
| ExtractedBatchNo | NVARCHAR(50) | |
| ExtractedExpiryDate | DATE NULL | |
| ExtractedQuantity | DECIMAL(18,3) | |
| ExtractedBonusQuantity | DECIMAL(18,3) | |
| ExtractedPurchasePrice | DECIMAL(18,2) | |
| ExtractedSalePrice | DECIMAL(18,2) | |
| ExtractedDiscountPercent | DECIMAL(5,2) | |
| ExtractedTaxPercent | DECIMAL(5,2) | |
| ExtractedLineTotal | DECIMAL(18,2) | |
| MatchStatus | NVARCHAR(10) | Green / Yellow / Red |
| MatchScore | INT | 0-100 |
| MatchedMedicineId | INT NULL FK | After user confirm |
| SuggestedMedicineIdsJson | NVARCHAR(MAX) | Top-N candidates with scores |
| UserAction | NVARCHAR(30) | Confirmed / ManuallyLinked / NewMedicineCreated / Skipped |
| NewMedicinePayloadJson | NVARCHAR(MAX) | If user creates new |
| ReviewNotes | NVARCHAR(300) | |
| std audit fields | | |

### Service interfaces (PMS.Core / PMS.Services)

```csharp
public interface IInvoiceExtractionService {
    Task<InvoiceExtractionResult> ExtractAsync(InvoiceFileRef file, CancellationToken ct);
}
// Implementations:
//   MockInvoiceExtractionService          (Phase 1 - returns canned JSON)
//   TesseractInvoiceExtractionService     (Phase 7 - offline OCR)
//   CloudAIInvoiceExtractionService       (Phase 7 - online via API)

public interface IInvoiceMatchingService {
    Task<IReadOnlyList<InvoiceItemMatch>> MatchAsync(
        IReadOnlyList<ExtractedInvoiceItem> items,
        CancellationToken ct);
}

public interface IInvoiceImportService {
    Task<InvoiceImport> CreateAsync(InvoiceFileRef file);
    Task RunExtractionAsync(int importId);
    Task RunMatchingAsync(int importId);
    Task UpdateUserDecisionAsync(int importItemId, InvoiceItemReviewDecisionDto decision);
    Task<int> ConvertToPurchaseAsync(int importId, ConvertToPurchaseRequestDto req);
    Task CancelAsync(int importId, string reason);
}

public interface IInvoiceFileStore {
    Task<string> SaveEncryptedAsync(Stream input, string fileName);
    Task<Stream> OpenDecryptedAsync(string storedPath);
    Task DeleteAsync(string storedPath);
}
```

### DTOs
`ExtractedInvoiceHeader`, `ExtractedInvoiceItem`, `InvoiceExtractionResult`, `InvoiceItemMatch` (with `MatchStatus` enum + suggestion list + score), `InvoiceImportReviewDto`, `InvoiceItemReviewDecisionDto`, `ConvertToPurchaseRequestDto`. All API-ready (same DTOs round-trip to cloud AI controller).

### Matching algorithm (concrete, not a stub)
1. Normalise extracted name: lowercase, strip pack-size suffixes (`10s`, `100ml`), strip punctuation.
2. Try exact `Medicine.Barcode` match -> Green.
3. Try exact normalised `Medicine.Name` -> Green.
4. Try exact match against `Medicine.Synonyms` -> Green.
5. Compute fuzzy similarity (Jaro-Winkler or Levenshtein-ratio) against name + generic + synonyms; pick top 5; best >= 0.95 -> Green; 0.60-0.94 -> Yellow with top-5 suggestions; else Red.
6. Bonus: if extracted batch matches an existing batch on a Yellow candidate, boost to Green.

### UI placeholder (PMS.Desktop -> Purchases -> "Smart Import")
- **Step 1 - Upload**: drag/drop image or PDF.
- **Step 2 - Extracting**: progress + cancel (instant in Phase 1 with Mock).
- **Step 3 - Review**: grid with colour rows (Green/Yellow/Red); for Yellow, dropdown of suggestions; for Red, inline "Search & Link" + "Create New Medicine"; header fields editable; confidence score visible.
- **Step 4 - Confirm**: system creates Purchase + batches + updates stock; redirects to created Purchase Invoice for final save/print.

### Audit & security
- Every extraction, decision, new-medicine creation, conversion writes to `tbl_AuditLogs` with `InvoiceImportId`.
- Source file kept encrypted via `IInvoiceFileStore`; deletable only by Owner.

### Sync
`tbl_InvoiceImports` and `tbl_InvoiceImportItems` participate in standard sync queue with `RecordGuid`. Cloud AI extraction is online-only; offline mode falls back to Tesseract or "manual fill".

### Why deferring real OCR is correct
Matching + review + conversion is **80% of the user-visible value** and is fully testable with mock data. OCR/AI is a swap-in implementation detail behind `IInvoiceExtractionService`.

## 13. Hardware Integration

Abstraction layer so drivers are swappable per pharmacy.

| Hardware | Use |
|---|---|
| ESC/POS thermal printer (TSP100, Epson) | Receipts |
| USB HID barcode scanner | POS + stock count |
| Cash drawer (RJ11 via printer) | Auto open on cash sale |
| Weighing scale | Loose syrups / creams |
| Customer pole / second display | Show items + total to customer |
| Label printer (Avery sheet + roll) | Repacked / loose-item barcodes |

Interface: `IReceiptPrinter`, `IBarcodeScanner`, `ICashDrawer`, `IWeighingScale`, `ICustomerDisplay`, `ILabelPrinter` - each with a `Null*` implementation as default so the app runs without hardware.

## 14. Compliance Pack [PK]

- **FBR POS Integration**: Tier-1 retailers must integrate. System obtains FBR invoice number and prints QR on receipt. Build hooks even if not every customer enables it.
- **DRAP / MRP compliance**: lock `SaleRetailPrice <= MRP`; override requires audit + reason.
- **Sales Tax / GST**: tax-inclusive vs exclusive items, exempt categories, NTN/STRN on invoice.
- **Narcotics register - government format**: auto-generate the legally required format for inspection.
- **Schedule G / H controlled drugs**: different prescription requirements per schedule.

---

# PART IV - UX/UI SYSTEM

## 15. UX/UI Design System

**Goal:** feels like a premium business system, not a cheap local POS. Owner should sense quality within 2 minutes of demo.

| Area | Direction |
|---|---|
| Top bar | Pharmacy name, user, date/time, online/offline, license, sync status |
| Sidebar | Dashboard, POS, Purchases, Stock, Medicines, Suppliers, Customers, Accounts, Reports, Settings |
| Cards | Clean KPI cards, soft shadow, clear numbers |
| Forms | Grouped sections, two-column layouts, searchable dropdowns, inline validation |
| Tables | Sticky headers, search, filters, export, color-coded statuses |
| Messages | Reusable success/error/warning, no technical jargon |

**Colors:**

| Purpose | Color |
|---|---|
| Primary | Dark Blue / Indigo |
| Success | Green |
| Warning | Orange |
| Danger | Red |
| Background | White / Light Gray |
| Text | Dark Slate |

**NEW v2:**
- Dark mode (night-shift counters).
- Touch / tablet POS layout variant.
- Multi-monitor (POS on screen 1, prescription / customer history on screen 2).
- Global Cmd-K command palette ("Add Medicine", "New Sale", "Today Profit").
- Urdu / RTL language pack.
- Customer-display layout for second monitor.

## 16. Reusable UX Components

- Message helper: `ShowSuccess`, `ShowError`, `ShowWarning`.
- Confirmation dialog helper.
- Loading overlay.
- Common card / panel style.
- Common button style.
- Common grid / table style.
- Empty-state component.
- Global search component.

---

# PART V - DATABASE & CLOUD

## 17. Database Blueprint

All tables prefixed `tbl_`. Standard audit fields on every table. **Every table gets `RecordGuid UNIQUEIDENTIFIER` from day one** (cheap now, painful later).

**Foundation:** `tbl_Pharmacies`, `tbl_PharmacyMachines`, `tbl_Users`, `tbl_Roles`, `tbl_Permissions`, `tbl_RolePermissions`, `tbl_LicenseInfo`, `tbl_ModuleSettings`, `tbl_SyncQueue`, `tbl_SyncLogs`, `tbl_AuditLogs`.

**Operational:** `tbl_Medicines`, `tbl_MedicineBatches`, `tbl_Suppliers`, `tbl_Purchases`, `tbl_PurchaseItems`, `tbl_Sales`, `tbl_SaleItems`, `tbl_SaleReturns`, `tbl_SaleReturnItems`, `tbl_StockAdjustments`, `tbl_Customers`, `tbl_Expenses`, `tbl_CustomerPayments`, `tbl_SupplierPayments`.

**Smart Import (NEW):** `tbl_InvoiceImports`, `tbl_InvoiceImportItems`.

**Compliance (NEW):** `tbl_TaxRates`, `tbl_FBRInvoiceMap` [PK].

**Engagement (NEW):** `tbl_Prescriptions`, `tbl_DrugInteractions`, `tbl_Allergies`, `tbl_RefillReminders`, `tbl_ColdChainLogs`, `tbl_Recalls`, `tbl_InsurancePanels`, `tbl_PanelClaims`, `tbl_DeliveryOrders`, `tbl_Riders`, `tbl_LoyaltyAccounts`, `tbl_LoyaltyTxns`, `tbl_Doctors`.

**Operational Quality (NEW):** `tbl_Shifts`, `tbl_CashDrawerCounts`, `tbl_StockTransfers`, `tbl_GRNs`, `tbl_PurchaseReturns`, `tbl_PurchaseOrders`, `tbl_PriceChangeHistory`, `tbl_LostSales`.

**Cross-cutting (NEW):** `tbl_FeatureFlags`, `tbl_SchemaVersion`, `tbl_RecycleBin`.

**Standard audit fields:**
```
Id            INT IDENTITY PRIMARY KEY
RecordGuid    UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID()
IsActive      BIT
SaveDate      DATETIME
SavedBy       INT NULL
UpdatedDate   DATETIME NULL
UpdatedBy     INT NULL
IsSynced      BIT  -- where sync applies
```

## 18. Cloud API & Admin Panel

The API is **not required for normal offline sales**. It powers license validation, cloud backup, sync, admin module control, mobile reporting, and AI invoice extraction.

**Controllers:**

| Controller | Purpose |
|---|---|
| AuthController | Admin / user token auth |
| LicenseController | Validate, renew, update modules |
| MachineController | Register, replace, deactivate |
| SyncController | Receive / send sync queue |
| BackupController | Upload / download encrypted DB backups |
| ReportsController | Mobile dashboard reports |
| PharmacyController | Pharmacy profile + settings |
| **InvoiceImportController** | Cloud AI invoice extraction (gated by `EnableCloudAIInvoiceReading`) |
| FBRController [PK] | FBR invoice number + QR |
| WhatsAppController | Daily reports, OTP, refill reminders |
| PaymentGatewayController | Subscription billing |
| ResellerController | Agent/reseller commissions |
| SupportTicketController | In-app tickets + KB |
| TelemetryController | Anonymous usage + crash reports |
| FeatureFlagController | A/B + gradual rollout |
| PrescriptionController | Prescription image OCR |
| RecallController | Recall flag + customer-buyer lookup |

**Super Admin Panel features:**
- Create pharmacy + setup code.
- Manage subscription plan + expiry.
- Enable/disable any module per pharmacy.
- View registered machines; deactivate / replace.
- Generate offline renewal codes + offline machine activation codes.
- Monitor sync + backup status.
- Suspend pharmacy on overdue payment / misuse.
- Reseller / agent management + commission report.
- Churn-risk dashboard (last sync, last sale, login frequency per pharmacy).

## 19. Sync & Backup

**Sync rules:**
- Desktop saves locally first, always.
- Sync runs in background only if enabled and internet available.
- Failed sync stored in `tbl_SyncQueue` and retried.
- Each synced record carries `RecordGuid` (and `CloudId` once acknowledged) to avoid duplicates.
- Mobile app reads cloud data only.
- **Idempotency keys** on every cloud sync request from day one.
- Conflict resolution: last-write-wins by default; UI to inspect/override conflicts.

**Backup:**

| Type | Behavior |
|---|---|
| Manual local | User clicks Backup Now -> selects safe location |
| Auto local | Daily/weekly to configured folder |
| Cloud | Encrypted DB backup uploaded if module enabled |
| Restore | Admin password required, audit log written |
| **E2E encrypted backup (NEW)** | Customer-held key option - you can never read their data |

**Important offline rule:** if cloud backup is disabled, the app must NEVER show sync errors. Show "Local Only Mode" clearly, work normally.

---

# PART VI - ROADMAP, AI & ENGAGEMENT

## 20. Development Roadmap

| Phase | Goal |
|---|---|
| 1 | Foundation: solution structure, DB foundation, pharmacy setup (PHM-DEMO/123456), login, machine binding, license foundation, shell UI, dashboard placeholders, reusable UI components |
| 2 | Medicine + Stock foundation: master with refrigerator/narcotics/printable + Schedule + MRP + Synonyms + Chronic; batch-wise stock; short-stock + expiry alerts |
| 3 | Purchase: supplier, purchase invoice + items, batch, expiry, bonus qty, pricing, supplier payable |
| **3.5** | **Smart Invoice Import (foundation only)**: DB tables, services, DTOs, matching engine, 4-step UI wired to MockInvoiceExtractionService |
| 4 | Sales POS: barcode/search, cart, discount, payment, invoice, return, hold, narcotics workflow |
| 4.5 | Hardware Integration: thermal printer, scanner, cash drawer, scale, pole display, label printer |
| 5 | Reports + Accounts: daily sales, profit, purchase, stock, expiry, short-stock, ledgers, hourly heatmap, dead stock, GP%, aging, lost-sale register |
| 5.5 | Compliance Pack [PK]: FBR POS, DRAP MRP enforcement, narcotics register government format, GST/tax engine |
| 6 | Cloud API + Admin Panel + Mobile: license validation, cloud backup, mobile reporting, WhatsApp alerts, admin dashboard, reseller panel, in-app support, telemetry, auto-update, auto-billing |
| 6.5 | Engagement: loyalty points, refill reminders, home delivery, customer mobile app lite |
| **7** | **AI / Intelligence**: activates real OCR for Smart Invoice Import (Tesseract offline + Cloud AI online); sales forecasting; anomaly/pilferage detection; smart search with typo tolerance; OCR prescription -> cart; voice search |

## 21. AI / Intelligence Layer

- Sales forecasting per item per month (last 6-12 months) -> drives auto-PO.
- Anomaly / pilferage detection: unusual void %, return %, large discounts by cashier, after-hours sales.
- Smart search: typo tolerance + synonyms ("panadol", "panadole", "panadl" all match).
- OCR prescription -> cart pre-fill.
- Voice search for hands-busy cashier.
- AI invoice extraction (powers Smart Invoice Import in Phase 7).

## 22. Customer Engagement

- Customer mobile app (lite): browse history, request refill, see loyalty points, order home delivery.
- Home delivery: order intake (phone/WhatsApp/app), rider assignment, delivery status, COD reconciliation.
- Loyalty points: earn per Rs spent, redeem on next visit.
- Refill reminders for chronic meds via WhatsApp/SMS N days before due.
- Birthday + chronic-patient automated discount coupons.
- Health-camp / BP-Sugar checkup log.

## 23. SaaS Business Layer

- Reseller / agent panel + commission tracking.
- In-app support tickets + knowledge base.
- Anonymous telemetry + crash reporter.
- Auto-update with rollback + version pinning per pharmacy.
- Subscription auto-billing (Stripe / local gateway).
- Churn-risk dashboard (last sync, last backup, last sale per pharmacy).
- One-click "Export My Data" for customers.

## 24. Quality Hardening

- Multi-language: English + Urdu (RTL).
- 2FA for Owner / Super Admin.
- E2E encrypted cloud backup.
- Feature flags table separate from license modules.
- Comprehensive seed catalog: 1000+ common Pakistani medicines pre-loaded.
- Idempotency-everywhere rule (every cloud write).
- Soft-delete + recycle bin standard (no hard deletes).
- Time-zone + monotonic clock-tamper hardening.

---

# PART VII - STEP-BY-STEP EXECUTION GUIDE

## 25. Prerequisites & Environment Setup

**Install on every developer machine:**

| Tool | Version / Notes |
|---|---|
| Visual Studio 2022 | Community or higher |
| VS workloads | ".NET desktop development", "ASP.NET and web development", "Data storage and processing" |
| .NET SDK | .NET 8 LTS (recommended) |
| SQL Server | LocalDB (dev) + Express (production target) |
| SSMS | SQL Server Management Studio |
| Git | Latest |
| Cursor IDE | Latest |
| Postman / Insomnia | API testing |

**Recommended NuGet packages:**

| Package | Purpose |
|---|---|
| Dapper (or EF Core 8) | Data access |
| BCrypt.Net-Next | Password hashing |
| Serilog + Serilog.Sinks.File | Structured logging |
| FluentValidation | Input validation |
| AutoMapper | DTO mapping |
| Polly | Retry / resilience |
| Hangfire (optional) | Background jobs |
| RestSharp / HttpClient | API client |
| MaterialSkin.2 (WinForms) or ModernWpf (WPF) | UI styling |
| QuestPDF | Receipt + report PDFs |

**Folder layout:**
```
d:\PharmacyManagementSystem\
   src\
      PMS.Desktop\
      PMS.Core\
      PMS.Data\
      PMS.Services\
      PMS.Security\
      PMS.Sync\
      PMS.Reporting\
      PMS.Database\         (versioned .sql files)
      PMS.Api\              (placeholder)
   docs\
   tests\
```

## 26. Day-1 Bootstrap (Cursor Command)

Paste this into Cursor on day one:

```
You are building a professional offline-first Pharmacy Management System from scratch.
Build the FOUNDATION ONLY. Do not build pharmacy modules yet.

Technology: C# desktop app (.NET 8), SQL Server LocalDB/Express, future ASP.NET Core Web API,
future mobile app.

Create solution projects:
- PMS.Desktop, PMS.Core, PMS.Data, PMS.Services, PMS.Security,
  PMS.Sync, PMS.Reporting, PMS.Database, PMS.Api (placeholder)

Core requirements:
1. Modular architecture: reusable services, repositories, helpers, UI components, permission checks.
2. SQL scripts for: tbl_Pharmacies, tbl_PharmacyMachines, tbl_Users, tbl_Roles, tbl_Permissions,
   tbl_RolePermissions, tbl_LicenseInfo, tbl_ModuleSettings, tbl_SyncQueue, tbl_SyncLogs,
   tbl_AuditLogs, tbl_FeatureFlags, tbl_SchemaVersion, tbl_RecycleBin.
   Every table includes RecordGuid UNIQUEIDENTIFIER.
3. Implement: password hashing (BCrypt), encryption helper, device fingerprint service,
   local encrypted license file, permission service, audit helper.
4. First-time setup screen using mock activation:
   Pharmacy Code: PHM-DEMO   Setup Code: 123456
   On success: create local pharmacy, register machine, create owner/admin, create 30-day trial.
5. Login screen: checks user pharmacy, registered machine, license status, permissions.
6. Modern desktop shell: top bar, sidebar, main content area, status bar.
7. Dashboard with cards: Today Sales, Today Profit, Stock Value, Low Stock, Near Expiry,
   License Days Left.
8. Settings placeholder + "Coming Next" screen for unbuilt modules.
9. Placeholder API controllers: Auth, License, Machine, Sync, Backup, Reports.
10. Module flag stubs in tbl_ModuleSettings including:
    EnableSmartInvoiceImport, EnableCloudAIInvoiceReading, EnableAutoCreateMedicineFromImport.
11. Professional UI/UX: clean spacing, modern cards, consistent indigo/green/orange/red,
    clear messages, reusable controls.

Hard rules:
- Do NOT write business logic in UI events.
- Do NOT duplicate code.
- Build services and reuse them.
- Desktop must be offline-first.
- Future cloud and mobile must be supported by structure.
- After finishing, summarize how to run, test setup credentials, created scripts, next module.
```

## 27. Phase-by-Phase Execution Plan

For each phase: **Goal -> In-scope -> Out-of-scope -> DB tables -> Services -> Screens -> Cursor prompt -> Definition of Done -> Manual test checklist.**

### Phase 1 - Foundation
- **Goal**: secure shell + setup + login + dashboard.
- **In-scope**: solution structure; foundation tables; setup flow with PHM-DEMO/123456; login; machine binding; license foundation; shell UI; dashboard cards (placeholders); module flag stubs.
- **Out-of-scope**: medicines, sales, purchases, real reports, real sync.
- **Tables**: see Day-1 Bootstrap.
- **Services**: `IAuthService`, `IUserService`, `IPharmacyService`, `IMachineService`, `ILicenseService`, `IPermissionService`, `IAuditService`, `IPasswordHasher`, `IDeviceFingerprintService`, `IEncryptionService`.
- **Screens**: First-Time Setup, Login, Shell, Dashboard, Settings placeholder, Coming Next.
- **DoD**: setup with PHM-DEMO/123456 works; trial license created; login works only on registered machine; dashboard shows license days left; permissions can hide a menu item.
- **Manual test**: install fresh; complete setup; close+reopen; login; verify only registered machine can log in; verify license countdown.

### Phase 2 - Medicine + Stock Foundation
- **Goal**: medicine master with all special flags + batch-wise stock.
- **In-scope**: medicines CRUD; batches; reorder/min/max; refrigerator/narcotics/printable; Schedule [PK]; MRP [PK]; Synonyms; IsChronicMed; short-stock + expiry alerts.
- **Out-of-scope**: any sale or purchase logic.
- **Tables**: `tbl_Medicines`, `tbl_MedicineBatches`.
- **Services**: `IMedicineService`, `IBatchService`, `IStockAlertService`.
- **Screens**: Medicines list + add/edit; Batches; Stock Alerts.
- **DoD**: can add medicines with all flags; expired/near-expiry alerts visible on dashboard.
- **Manual test**: add 10 sample medicines (mix flags); add batches with future + past expiry; verify alerts.

### Phase 3 - Purchase Module
- **Goal**: manual purchase entry + supplier ledger.
- **In-scope**: suppliers CRUD; purchase invoice + items; batch creation from purchase; bonus qty; pricing; supplier payable.
- **Out-of-scope**: AI invoice import (next phase), GRN, purchase returns.
- **Tables**: `tbl_Suppliers`, `tbl_Purchases`, `tbl_PurchaseItems`, `tbl_SupplierPayments`.
- **Services**: `ISupplierService`, `IPurchaseService`, `ISupplierLedgerService`.
- **Screens**: Suppliers; New Purchase; Purchase List; Supplier Ledger.
- **DoD**: posting a purchase increases batch stock, creates supplier payable, writes audit.

### Phase 3.5 - Smart Invoice Import (Foundation)
- **Goal**: end-to-end pipeline working with MockInvoiceExtractionService.
- **In-scope**: tables + services + DTOs + matching engine + 4-step UI; encrypted file store; module flags.
- **Out-of-scope**: real OCR (Tesseract/Cloud AI - Phase 7).
- **Tables**: `tbl_InvoiceImports`, `tbl_InvoiceImportItems`.
- **Services**: `IInvoiceExtractionService` + Mock impl; `IInvoiceMatchingService`; `IInvoiceImportService`; `IInvoiceFileStore`.
- **Screens**: Smart Import wizard (Upload -> Extracting -> Review -> Confirm).
- **DoD**: upload sample file -> mock returns 5-10 items -> review grid shows green/yellow/red correctly -> confirm creates a Purchase + batches + stock movement -> audit logged.
- **Manual test**: prepare canned mock JSON with 1 exact match, 2 partial, 1 unknown; verify each gets correct color; link 1 manually; create 1 new medicine; confirm; verify resulting Purchase invoice and stock.

### Phase 4 - Sales POS
- **Goal**: fast counter sales.
- **In-scope**: barcode/search; cart; discount; payment (single method first); invoice print; return; hold (multiple); narcotics workflow with doctor remarks.
- **Out-of-scope**: split payment, JazzCash, prescription image, drug interactions (later).
- **Tables**: `tbl_Sales`, `tbl_SaleItems`, `tbl_SaleReturns`, `tbl_SaleReturnItems`, `tbl_Customers`, `tbl_CustomerPayments`.
- **Services**: `ISaleService`, `ISaleReturnService`, `ICustomerService`, `IInvoicePrintService`.
- **Screens**: POS, Hold list, Returns, Customer master.
- **DoD**: 60 sales/hour benchmark with shortcuts; FEFO works; expired blocked; near-expiry warns; narcotics requires doctor.

### Phase 4.5 - Hardware Integration
- **Goal**: real-world counter setup.
- **In-scope**: ESC/POS thermal printer; USB HID scanner; cash drawer; pole display; label printer.
- **Services**: `IReceiptPrinter`, `IBarcodeScanner`, `ICashDrawer`, `ICustomerDisplay`, `ILabelPrinter` (with Null impls).
- **DoD**: receipt prints on tested printer; scanner inputs into POS; drawer opens on cash sale.

### Phase 5 - Reports + Accounts
- **In-scope**: daily sales, profit, purchase, stock, expiry, short-stock, narcotics, cashier, customer/supplier ledger, expenses, cashbook, hourly heatmap, dead stock, GP%, aging, lost-sale register.
- **Tables**: `tbl_Expenses`, `tbl_StockAdjustments`, `tbl_LostSales`, `tbl_PriceChangeHistory`, `tbl_Shifts`, `tbl_CashDrawerCounts`.
- **Screens**: Reports hub; each report; Shift open/close; Cash count.

### Phase 5.5 - Compliance Pack [PK]
- **In-scope**: FBR POS hooks; DRAP MRP enforcement; narcotics register government format; GST/tax engine.
- **Tables**: `tbl_TaxRates`, `tbl_FBRInvoiceMap`.
- **Services**: `IFbrService`, `ITaxEngine`.

### Phase 6 - Cloud API + Admin Panel + Mobile + Engagement
- **In-scope**: license validation, cloud backup, sync, mobile reporting, WhatsApp daily reports, admin dashboard, reseller panel, in-app support, telemetry, auto-update, auto-billing, prescription module, cold-chain log, recall.
- **Tables**: engagement set + `tbl_Prescriptions`, `tbl_ColdChainLogs`, `tbl_Recalls`.
- **Services**: full controller list per Section 18.

### Phase 6.5 - Engagement
- **In-scope**: loyalty, refill reminders, home delivery, customer mobile app lite, panels/insurance.
- **Tables**: `tbl_LoyaltyAccounts`, `tbl_LoyaltyTxns`, `tbl_RefillReminders`, `tbl_DeliveryOrders`, `tbl_Riders`, `tbl_Doctors`, `tbl_InsurancePanels`, `tbl_PanelClaims`.

### Phase 7 - AI / Intelligence
- **In-scope**: real OCR for Smart Invoice Import (Tesseract offline + Cloud AI online); sales forecasting; anomaly/pilferage detection; smart search; OCR prescription; voice search.
- **Implementation rule**: only swap-in implementations behind existing interfaces; no schema changes required for invoice import.

## 28. Coding Conventions & Anti-Patterns

**Do:**
- Layered architecture: UI -> Service -> Repository -> DB. UI never touches DB.
- Every table prefixed `tbl_` and includes standard audit + `RecordGuid`.
- Audit on every mutation.
- Soft-delete by default (set `IsActive = 0`); admin recycle bin restores.
- Reusable message helpers, dialogs, grids, buttons.
- Versioned schema via `tbl_SchemaVersion` and numbered SQL files (`001_init.sql`, `002_medicines.sql`, ...).
- DTOs in `PMS.Core` shared between desktop and API.

**Don't:**
- No business logic inside event handlers.
- No duplicated SQL across screens.
- No hard-coded pharmacy IDs in UI.
- No screen bypasses permission + license checks.
- No technical jargon in user-facing messages.
- No skipping phases - foundation must be stable before next module.

## 29. Testing Strategy

| Type | What | Tool |
|---|---|---|
| Unit | Services + matching algorithm + license signing | xUnit + Moq |
| Integration | Repositories against LocalDB | xUnit + Respawn |
| Manual | POS flow, Smart Import review, install/uninstall | Test checklist per phase |
| Smoke | Bootstrap + login + dashboard load | Run before every commit |

**Seed data:** 1000+ common Pakistani medicines + 5 sample suppliers + 10 sample customers + 1 demo pharmacy (PHM-DEMO).

## 30. Release & Deployment

- **Installer**: Inno Setup (simpler) or MSIX (modern).
- **Code signing**: purchase a signing certificate (DigiCert / Sectigo).
- **Auto-update**: in-app updater pulls from your server; supports rollback; per-pharmacy version pin from admin panel.
- **Customer onboarding checklist**:
  1. You create pharmacy in Super Admin Panel -> get Pharmacy Code + Setup Code.
  2. Send installer link + codes to customer.
  3. Customer installs -> enters codes -> 30-day trial starts.
  4. Customer creates owner user -> ready.
  5. Day 23: payment reminder. Day 30: trial expires -> grace period.

## 31. Team Workflow with Cursor

- Use **Plan mode** to design before each phase; **Agent mode** to implement.
- Maintain `.cursor/rules/` with project-wide rules (e.g. naming, tbl_ prefix, no hard deletes, audit-on-mutation).
- Suggested rules entries:
  - "Every new table must include `RecordGuid UNIQUEIDENTIFIER`."
  - "UI event handlers must call services, never repositories or DbContext directly."
  - "Every mutation must call `IAuditService.Log(...)`."
  - "Every screen behind permission check via `IPermissionService.CanAccess(...)`."
- Reuse the prompt library in Appendix C for consistency.

## 32. Risks & Mitigations

| # | Risk | Mitigation |
|---|---|---|
| 1 | AI invoice extraction accuracy | Mock-first design; user reviews every line; confidence score visible; fallback to manual fill |
| 2 | FBR API changes [PK] | Isolate FBR logic behind `IFbrService`; version-aware adapter |
| 3 | Machine fingerprint drift | Combined signal with tolerance; admin can replace machine |
| 4 | Sync conflicts | RecordGuid + idempotency keys + last-write-wins UI |
| 5 | License circumvention | Encrypted file + signature + monotonic clock counter + cloud cross-check |
| 6 | Cloud OCR cost per invoice | Tier-gated (Premium); per-month cap with alert |
| 7 | Tablet/touch UX gaps | Dedicated touch layout variant in Phase 4 |
| 8 | Slow POS at high volume | Indexed search; batch FEFO precomputed; in-memory medicine cache |
| 9 | Customer data loss | Auto local backup default ON; cloud E2E backup option |
| 10 | Scope creep | Strict phase gate - DoD must pass before next phase starts |

---

# APPENDICES

## Appendix A - All Database Tables Cheat Sheet

| Table | Purpose |
|---|---|
| tbl_Allergies | Customer allergies for warnings |
| tbl_AuditLogs | Security + data audit trail |
| tbl_CashDrawerCounts | Open/close shift cash count |
| tbl_ColdChainLogs | Daily fridge temperature |
| tbl_Customers | Customer master |
| tbl_CustomerPayments | Customer credit payments |
| tbl_DeliveryOrders | Home delivery orders |
| tbl_Doctors | Prescribing doctor master |
| tbl_DrugInteractions | Interaction groups |
| tbl_Expenses | Shop expenses |
| tbl_FBRInvoiceMap [PK] | FBR invoice number map |
| tbl_FeatureFlags | A/B + gradual rollout flags |
| tbl_GRNs | Goods received notes |
| tbl_InsurancePanels | Panel master |
| tbl_InvoiceImports | Smart Invoice Import header |
| tbl_InvoiceImportItems | Smart Invoice Import lines |
| tbl_LicenseInfo | Local license state |
| tbl_LostSales | Asked-but-not-stocked register |
| tbl_LoyaltyAccounts | Loyalty member accounts |
| tbl_LoyaltyTxns | Loyalty transactions |
| tbl_MedicineBatches | Batch-wise stock |
| tbl_Medicines | Medicine master |
| tbl_ModuleSettings | Per-pharmacy module flags |
| tbl_PanelClaims | Panel monthly claims |
| tbl_Permissions | Permission keys |
| tbl_Pharmacies | Customer pharmacy profile |
| tbl_PharmacyMachines | Registered devices |
| tbl_PriceChangeHistory | Price change audit |
| tbl_Prescriptions | Prescription images |
| tbl_PurchaseItems | Purchase invoice lines |
| tbl_PurchaseOrders | Auto-PO generated orders |
| tbl_PurchaseReturns | Debit notes |
| tbl_Purchases | Purchase invoice header |
| tbl_RecycleBin | Soft-deleted records |
| tbl_Recalls | Drug recall flags |
| tbl_RefillReminders | Chronic refill schedules |
| tbl_Riders | Delivery riders |
| tbl_Roles | Role definitions |
| tbl_RolePermissions | Role -> permission mapping |
| tbl_SaleItems | Sale lines |
| tbl_SaleReturnItems | Return lines |
| tbl_SaleReturns | Return header |
| tbl_Sales | Sale header |
| tbl_SchemaVersion | DB migration tracker |
| tbl_Shifts | Open/close shift |
| tbl_StockAdjustments | Manual adjustments |
| tbl_StockTransfers | Branch transfers |
| tbl_Suppliers | Supplier master |
| tbl_SupplierPayments | Supplier payable |
| tbl_SyncLogs | Sync history |
| tbl_SyncQueue | Pending sync ops |
| tbl_TaxRates | Tax rate master |
| tbl_Users | User accounts |

## Appendix B - Module Flags & License Tier Map

| Flag | Basic | Standard | Premium |
|---|:---:|:---:|:---:|
| Core POS / Purchases / Stock / Medicines | YES | YES | YES |
| Local Backup | YES | YES | YES |
| Cloud Backup | - | YES | YES |
| Advanced Reports | - | YES | YES |
| EnableSmartInvoiceImport | - | YES (offline OCR) | YES |
| EnableCloudAIInvoiceReading | - | - | YES |
| EnableAutoCreateMedicineFromImport | - | YES (admin opt-in) | YES |
| Cold Chain Log | - | YES | YES |
| Drug Recall | - | YES | YES |
| Barcode Label Printing | - | YES | YES |
| Shift Management | - | YES | YES |
| Mobile App Reporting | - | - | YES |
| WhatsApp Reports | - | - | YES |
| Multi-Branch | - | - | YES |
| Drug Interactions | - | - | YES |
| Refill Reminders | - | - | YES |
| Insurance Panels | - | - | YES |
| Home Delivery | - | - | YES |
| Loyalty Points | - | - | YES |
| Sales Forecasting | - | - | YES |
| Anomaly Detection | - | - | YES |
| Prescription OCR -> Cart | - | - | YES |
| FBR POS [PK] | - | - | YES |

## Appendix C - Cursor Prompt Library

**Phase 2 prompt:** "Build the Medicine Master + Batch Stock module for the Pharmacy Management System foundation. Include all special flags (refrigerator, narcotics, printable, Schedule G/H, MRP, Synonyms, IsChronicMed). Add tbl_Medicines and tbl_MedicineBatches with RecordGuid. Implement IMedicineService, IBatchService, IStockAlertService. Build screens for medicines list, add/edit, batches, stock alerts. Wire dashboard alerts (low stock, near expiry, expired). Reuse existing UI components, message helpers, and permission service. Do NOT touch sales or purchases yet."

**Phase 3.5 prompt (Smart Invoice Import):** "Build the Smart Invoice Import / AI Invoice Reader module FOUNDATION for the Pharmacy Management System. Do NOT implement real OCR yet. Create tables tbl_InvoiceImports and tbl_InvoiceImportItems exactly as specified in section 12 of the blueprint. Create interfaces IInvoiceExtractionService (with MockInvoiceExtractionService implementation that returns canned JSON), IInvoiceMatchingService (with the Green/Yellow/Red algorithm using barcode -> exact name -> synonyms -> Jaro-Winkler fuzzy match), IInvoiceImportService orchestrator, IInvoiceFileStore (encrypted local storage). Create DTOs as listed in section 12. Build the 4-step wizard UI (Upload -> Extracting -> Review with colored grid -> Confirm). On final confirm, call existing IPurchaseService to create a Purchase + batches + stock movement. Audit every step via IAuditService. Add module flags EnableSmartInvoiceImport, EnableCloudAIInvoiceReading, EnableAutoCreateMedicineFromImport to tbl_ModuleSettings. Do NOT touch other modules."

(Replicate similar templates for each subsequent phase.)

## Appendix D - Glossary

- **FEFO**: First Expiry, First Out - batch selection rule.
- **FBR**: Federal Board of Revenue (Pakistan tax authority).
- **DRAP**: Drug Regulatory Authority of Pakistan.
- **MRP**: Maximum Retail Price (regulated price ceiling).
- **GRN**: Goods Received Note - acknowledges physical receipt of goods.
- **GP%**: Gross Profit percentage.
- **RecordGuid**: stable global identifier on every row, enables sync without duplicates.
- **OCR**: Optical Character Recognition.
- **PHM-DEMO / 123456**: dev test pharmacy code + setup code.

## Appendix E - Open Decisions for Stakeholder Discussion

Bring these to the team meeting and decide before Phase 1 finishes:

1. **Multi-branch in MVP?** YES adds branch_id to most tables now (cheap). NO is fine, but adding later is painful.
2. **Customer mobile app - YES/NO/Later?** Affects Phase 6.5 scope.
3. **FBR POS - mandatory or optional?** Decides if Phase 5.5 is on critical path.
4. **OCR engine for Phase 7?** Tesseract only (free, lower accuracy) vs Cloud AI (paid, better) vs both?
5. **WPF vs WinForms vs WinUI 3 for Desktop?** Affects look-and-feel and dev velocity.
6. **EF Core or Dapper?** EF Core = faster CRUD, more magic. Dapper = control + performance.
7. **Pricing for Basic / Standard / Premium?** Decides reseller commission economics.
8. **Reseller / agent program - YES/NO?** Decides Phase 6 scope.
9. **End-to-end encrypted backup - default ON or opt-in?** Affects "I forgot my key" support load.
10. **Pakistan only or international from day one?** Affects how many `[PK]` items become hard-coded.

## Appendix F - Change Log v1 -> v2

| Area | v1 | v2 |
|---|---|---|
| Sections | 19 | 32 + 6 appendices |
| Modules | 11 | 24 (added Smart Invoice Import, Prescription, Drug Interactions, Refill Reminders, Cold Chain, Recall, Panels, Home Delivery, Loyalty, Forecasting, Anomaly, FBR PK, Label Print, Shift Mgmt) |
| Tables | 25 | 51 (added 26 new) |
| Phases | 6 | 11 (added 3.5, 4.5, 5.5, 6.5, 7) |
| Compliance | not addressed | Pakistan pack [PK] (FBR, DRAP MRP, GST, narcotics register) |
| AI | not addressed | Phase 7 with concrete plan + interfaces |
| Hardware | mentioned | Dedicated section + interfaces |
| Engagement | mentioned (WhatsApp) | Loyalty + Refill + Delivery + Customer App |
| SaaS biz | not addressed | Reseller, telemetry, auto-update, auto-billing, churn dashboard |
| Quality | basic audit | 2FA, soft-delete, recycle bin, feature flags, E2E backup, RecordGuid everywhere, schema versioning |
| Execution Guide | missing | Part VII - 8 chapters covering setup -> phases -> testing -> release |

---

**End of Blueprint v2.0.**
