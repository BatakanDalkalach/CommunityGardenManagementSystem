# Community Garden Management System

A full-featured ASP.NET Core MVC web application for managing community garden plots, members, harvest records, maintenance requests, and announcements — with role-based access control and a dedicated admin area.

**GitHub:** [BatakanDalkalach/CommunityGardenManagementSystem](https://github.com/BatakanDalkalach/CommunityGardenManagementSystem)

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Features](#features)
3. [Technology Stack](#technology-stack)
4. [Architecture](#architecture)
5. [Entity Models](#entity-models)
6. [Setup Instructions](#setup-instructions)
7. [Test Coverage](#test-coverage)
8. [Security](#security)
9. [Default Seed Accounts](#default-seed-accounts)

---

## Project Overview

The Community Garden Management System (CGMS) is a web application designed to digitise the day-to-day operations of a shared community garden. Garden administrators can manage plots, publish announcements, oversee maintenance work orders, and review all member activity from a dedicated admin dashboard. Registered gardeners can browse available plots, log harvest records, submit maintenance requests, and manage their own profiles.

The application is built on ASP.NET Core MVC with Entity Framework Core and ASP.NET Core Identity, targeting .NET 10. It defaults to an SQLite database for zero-configuration setup while transparently switching to SQL Server when a LocalDB or SQL Server Express connection string is supplied.

---

## Features

### Member-Facing

| Feature | Description |
|---|---|
| Plot browsing | View all garden plots, filter by availability, see plot details including soil type, area, and annual fee |
| Plot statistics | Aggregated stats bar showing totals for plots, occupied vs. vacant, and average fee |
| Harvest records | Full CRUD for harvest logs — crop name, quantity (kg), quality rating (1–5), organic certification, and free-text notes |
| Harvest search & filter | Filter harvest records by crop name (partial match) and/or organic-only flag with paginated results |
| Maintenance requests | Submit, view, edit, and delete maintenance requests linked to a specific plot |
| Member directory | Browse all registered gardeners with membership tier and experience details |
| Member statistics | Aggregated stats on membership tiers and organic preference distribution |
| Announcements | Read published announcements posted by administrators |
| User profile | View and update personal profile details including full name |
| Authentication | Registration, login, logout, and access-denied pages |

### Admin-Facing (Admin Area)

| Feature | Description |
|---|---|
| Admin dashboard | Overview of key garden metrics: total plots, members, pending maintenance requests, and recent activity |
| User management | List all registered application users with their assigned roles |
| Announcement management | Full CRUD for announcements with publish/unpublish toggle |
| Maintenance request management | View all maintenance requests across all plots, update status (Pending / InProgress / Completed) |

---

## Technology Stack

### Backend

| Component | Technology |
|---|---|
| Framework | ASP.NET Core MVC 10.0 |
| Language | C# 12 |
| ORM | Entity Framework Core 10.0 |
| Database | SQLite (default) / SQL Server (LocalDB or Express) |
| Identity | ASP.NET Core Identity 10.0 |
| Dependency injection | Built-in .NET DI container |

### Frontend

| Component | Technology |
|---|---|
| Templating | Razor Views (.cshtml) |
| CSS framework | Bootstrap 5 |
| Client validation | jQuery Unobtrusive Validation |
| Scripting | Vanilla JavaScript (no AJAX — all pages use server-side rendering) |

### Testing

| Component | Technology |
|---|---|
| Test framework | xUnit 2.9 |
| Test runner | Microsoft.NET.Test.Sdk 17.13 |
| In-memory DB for tests | EF Core SQLite in-memory provider |
| Coverage collection | coverlet 6.0 |

---

## Architecture

The application follows the standard ASP.NET Core MVC pattern, extended with an Admin Area and a dedicated Services layer for business logic.

```
WebApplication1-master/
├── WebApplication1/                        # Main web application
│   ├── Areas/
│   │   └── Admin/                          # Admin Area (role-restricted)
│   │       ├── Controllers/
│   │       │   ├── DashboardController.cs  # Admin dashboard & user list
│   │       │   ├── AnnouncementsController.cs
│   │       │   └── MaintenanceRequestsController.cs
│   │       └── Views/
│   │           ├── Dashboard/
│   │           ├── Announcements/
│   │           ├── MaintenanceRequests/
│   │           └── Shared/_AdminLayout.cshtml
│   ├── Controllers/                         # Standard MVC controllers
│   │   ├── AccountController.cs            # Login, Register, Logout
│   │   ├── AnnouncementsController.cs      # Public announcements read
│   │   ├── ErrorController.cs             # Custom 404 / 500 pages
│   │   ├── HarvestRecordsController.cs    # Harvest log CRUD + search
│   │   ├── HomeController.cs              # Landing page, About, Privacy
│   │   ├── MaintenanceRequestsController.cs
│   │   ├── MembersController.cs           # Member directory & statistics
│   │   ├── PlotsController.cs             # Plot CRUD & statistics
│   │   └── ProfileController.cs           # Authenticated user profile
│   ├── DatabaseContext/
│   │   ├── CommunityGardenDatabase.cs     # EF Core DbContext
│   │   └── DbSeeder.cs                    # Runtime seed data
│   ├── Migrations/                         # EF Core migration history
│   ├── Models/                             # Domain entities & view models
│   ├── Services/
│   │   ├── PlotManagementService.cs       # Plot data access & business logic
│   │   └── MemberManagementService.cs     # Member data access & business logic
│   ├── Views/                              # Razor views per controller
│   │   └── Shared/                         # Layouts, partial views, error pages
│   └── Program.cs                          # Application bootstrap & middleware
└── WebApplication1.Tests/                  # xUnit test project
    ├── AnnouncementTests.cs
    ├── HarvestRecordTests.cs
    ├── MaintenanceRequestTests.cs
    ├── MemberManagementServiceTests.cs
    └── PlotManagementServiceTests.cs
```

### MVC Controllers

Each controller handles a distinct domain:

- **HomeController** — public landing page, About, and Privacy pages.
- **PlotsController** — full CRUD for `GardenPlot` plus an aggregated statistics view; uses `PlotManagementService`.
- **MembersController** — member directory, per-member profile view, and membership statistics; uses `MemberManagementService`.
- **HarvestRecordsController** — harvest log CRUD with crop-name search and organic filter; directly queries `CommunityGardenDatabase`.
- **MaintenanceRequestsController** — member-side CRUD for maintenance requests linked to plots.
- **AnnouncementsController** — read-only list of published announcements for all authenticated users.
- **ProfileController** — allows the authenticated user to view and update their own profile.
- **AccountController** — handles Identity login, registration, logout, and access-denied redirect.
- **ErrorController** — renders custom 404 and 500 error pages.

### Admin Area

All Admin Area controllers are decorated with `[Area("Admin")]` and `[Authorize(Roles = "Admin")]`:

- **DashboardController** — aggregated statistics page and a full user list (reads from `UserManager<ApplicationUser>`).
- **Admin/AnnouncementsController** — full CRUD for announcements, including publish toggle.
- **Admin/MaintenanceRequestsController** — read all requests across the system; update status.

### Services Layer

Business logic and data-access queries are encapsulated in two scoped services registered in `Program.cs`:

- **PlotManagementService** — `RetrieveAllPlotsAsync`, `FindPlotByIdentifierAsync`, `RegisterNewPlotAsync`, `ModifyPlotDetailsAsync`, `RemovePlotAsync`, `CheckPlotExistsAsync`, `GetVacantPlotsAsync`.
- **MemberManagementService** — `RetrieveAllMembersAsync`, `FindMemberByIdAsync`, `EnrollNewMemberAsync`, `SearchByMembershipTypeAsync`, `UpdateMemberAsync`.

---

## Entity Models

### GardenPlot

Represents a single rentable plot in the garden.

| Property | Type | Notes |
|---|---|---|
| `PlotIdentifier` | `int` | Primary key |
| `PlotDesignation` | `string` | Unique 4-char code, pattern `[A-Z]\d{3}` (e.g. `A001`) |
| `SquareMeters` | `double` | 5–100 sq m |
| `SoilType` | `string` | Max 50 chars, default `"Loamy"` |
| `WaterAccessAvailable` | `bool` | Default `true` |
| `IsOccupied` | `bool` | Default `false` |
| `YearlyRentalFee` | `decimal` | 0–10 000 |
| `LastMaintenanceDate` | `DateTime` | Date only |
| `AssignedGardenerId` | `int?` | FK → `GardenMember` |
| `CurrentTenant` | `GardenMember?` | Navigation property |
| `HarvestHistory` | `ICollection<HarvestRecord>?` | Navigation property |

### GardenMember

Represents a registered gardener.

| Property | Type | Notes |
|---|---|---|
| `MemberId` | `int` | Primary key |
| `FullLegalName` | `string` | 3–80 chars |
| `EmailContact` | `string` | Validated email, unique |
| `MembershipTier` | `string` | `"Basic"` / `"Standard"` / `"Premium"` |
| `RegistrationDate` | `DateTime` | Date only, default today |
| `YearsOfExperience` | `int` | 0–50 |
| `PreferOrganicOnly` | `bool` | Default `true` |
| `GardeningInterests` | `string?` | Max 300 chars |
| `ManagedPlots` | `ICollection<GardenPlot>?` | Navigation property |
| `RecordedHarvests` | `ICollection<HarvestRecord>?` | Navigation property |

### HarvestRecord

Tracks a single crop harvest event.

| Property | Type | Notes |
|---|---|---|
| `RecordId` | `int` | Primary key |
| `PlotIdentifier` | `int` | FK → `GardenPlot` |
| `MemberId` | `int` | FK → `GardenMember` |
| `CropName` | `string` | 2–60 chars |
| `QuantityKilograms` | `double` | 0.1–1 000 kg |
| `CollectionDate` | `DateTime` | Date only, default today |
| `QualityScore` | `int` | 1 (poor) – 5 (excellent), default 3 |
| `HarvestNotes` | `string?` | Max 400 chars |
| `IsOrganicCertified` | `bool` | Default `false` |

### MaintenanceRequest

A work-order submitted for a specific plot.

| Property | Type | Notes |
|---|---|---|
| `Id` | `int` | Primary key |
| `PlotId` | `int` | FK → `GardenPlot` |
| `Description` | `string` | 10–1 000 chars |
| `RequestDate` | `DateTime` | Date only, default today |
| `Status` | `MaintenanceStatus` | `Pending` / `InProgress` / `Completed` |
| `Plot` | `GardenPlot?` | Navigation property |

### Announcement

A public notice published by admins.

| Property | Type | Notes |
|---|---|---|
| `Id` | `int` | Primary key |
| `Title` | `string` | 3–200 chars |
| `Content` | `string` | 10–5 000 chars |
| `CreatedAt` | `DateTime` | UTC timestamp |
| `IsPublished` | `bool` | Default `false`; only published entries shown to members |

### ApplicationUser

Extends `IdentityUser` with one additional field:

| Property | Type | Notes |
|---|---|---|
| `FullName` | `string` | Display name shown in the nav bar and profile page |

---

## Setup Instructions

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- Any IDE: Visual Studio 2022+, VS Code with C# Dev Kit, or JetBrains Rider
- (Optional) SQL Server LocalDB or SQL Server Express for production-style setup

### 1. Clone the repository

```bash
git clone https://github.com/BatakanDalkalach/CommunityGardenManagementSystem.git
cd CommunityGardenManagementSystem/WebApplication1-master
```

### 2. Restore NuGet packages

```bash
dotnet restore
```

### 3. (Optional) Configure a connection string

By default the application uses **SQLite** (`CommunityGarden.db` in the working directory). No configuration changes are required.

To use **SQL Server LocalDB** instead, add or update `appsettings.json` in `WebApplication1/`:

```json
{
  "ConnectionStrings": {
    "GardenDbConnection": "Server=(localdb)\\mssqllocaldb;Database=CommunityGardenDb;Trusted_Connection=True;"
  }
}
```

The application detects `localdb` or `sqlexpress` in the connection string and switches the EF Core provider automatically.

### 4. Apply database migrations

```bash
cd WebApplication1
dotnet ef database update
```

### 5. Run the application

```bash
dotnet run
```

The application starts on `https://localhost:5001` (HTTPS) and `http://localhost:5000` (HTTP, redirected to HTTPS).

### 6. Run the tests

```bash
cd ../WebApplication1.Tests
dotnet test
```

---

## Test Coverage

The test suite uses **xUnit** backed by an **EF Core SQLite in-memory provider** for fast, isolated tests with no external dependencies.

| Test suite | Tests | Status |
|---|---|---|
| `HarvestRecordTests` | 21 | Passed |
| `MaintenanceRequestTests` | 20 | Passed |
| `AnnouncementTests` | 16 | Passed |
| `PlotManagementServiceTests` | 15 | Passed |
| `MemberManagementServiceTests` | 13 | Passed |
| **Total** | **106** | **All Passed** |

**Duration:** ~2.3 s | **Failed:** 0 | **Skipped:** 0

### What is covered

- Full CRUD operations on all five domain entities via their controllers and service methods
- Model validation: required fields, string-length constraints, range limits, and regex patterns
- Business-logic edge cases: concurrency conflicts, vacant-plot filtering, tier-based member search
- HarvestRecord filtering: crop-name partial-match search, organic-only flag, and combined filter combinations
- Announcement publish/unpublish state transitions

---

## Security

### HTTP Security Headers

Applied unconditionally to every response via inline middleware in `Program.cs`:

| Header | Value | Purpose |
|---|---|---|
| `Content-Security-Policy` | `default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; form-action 'self'; frame-ancestors 'none'` | Restricts resource origins; blocks inline scripts; prevents framing |
| `X-Frame-Options` | `DENY` | Clickjacking protection for non-CSP-aware browsers |
| `X-Content-Type-Options` | `nosniff` | Prevents MIME-type sniffing |

### HTTPS & HSTS

- `app.UseHttpsRedirection()` redirects all HTTP traffic to HTTPS.
- `app.UseHsts()` (production only) sends the `Strict-Transport-Security` header to pin clients to HTTPS.

### CSRF Protection

ASP.NET Core's built-in anti-forgery token system is enabled globally. Every state-mutating form (`POST`, `PUT`, `DELETE`) includes an `[ValidateAntiForgeryToken]` attribute on the corresponding action method, and all Razor form tags emit a hidden `__RequestVerificationToken` field automatically via the `asp-action` tag helper.

### XSS Protection

All user-supplied content is HTML-encoded by Razor's `@expression` syntax by default. No `Html.Raw()` calls are present for user data. The CSP `script-src 'self'` directive adds a second layer of defence against injected scripts.

### Authentication & Authorisation

- **ASP.NET Core Identity** manages password hashing, account lockout thresholds, and secure cookie issuance.
- Password policy: minimum 6 characters, must contain at least one digit, one lowercase letter, and one uppercase letter.
- Cookies use sliding expiration and are configured with `HttpOnly` and `Secure` flags.
- All non-public pages require `[Authorize]`. The Admin Area additionally requires `[Authorize(Roles = "Admin")]`.
- Custom error pages are served for 403, 404, and 500 status codes via `UseStatusCodePagesWithReExecute`.

---

## Default Seed Accounts

On first launch the application automatically seeds two user accounts and sample domain data (plots, members, harvest records, announcements, and maintenance requests).

| Role | Email | Password |
|---|---|---|
| Admin | `admin@garden.com` | `Admin123!` |
| User | `user@garden.com` | `User123!` |

The **Admin** account has access to the `/Admin` area (dashboard, announcements management, maintenance request status updates, and user list). The **User** account has access to all member-facing features.

> These credentials are intended for development and demonstration only. Change them before deploying to any shared or public environment.
