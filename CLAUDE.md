# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

This is ABC Warehouse's customized nopCommerce **4.80.8** e-commerce platform built on **ASP.NET Core / .NET 9.0**. The solution includes the core nopCommerce libraries plus 30+ custom plugins for ABC Warehouse-specific functionality. The SDK version is pinned to `9.0.100` via `global.json`.

## Build & Run Commands

```powershell
# Debug build
dotnet clean src/NopCommerce.sln
dotnet build src/NopCommerce.sln

# Release build
dotnet clean src/NopCommerce.sln -c Release
dotnet build src/NopCommerce.sln -c Release

# Run locally (or press F5 in VSCode)
cd src/Presentation/Nop.Web
dotnet run

# Publish
dotnet publish -c Release ./src/Presentation/Nop.Web/Nop.Web.csproj --no-restore -o <output-path>

# Unit tests (NUnit)
dotnet test src/Tests/Nop.Tests/Nop.Tests.csproj

# E2E tests (Playwright)
cd e2e && npm test
```

## Local Development Setup

Prerequisites: .NET 9.0 SDK, SQL Server access.

```bash
# In a GitHub Codespace (8 CPU / 32GB RAM recommended):
bash ./.devcontainer/init.sh
# Prompts for SQL Server IP and password → writes App_Data/dataSettings.json
```

Manually: copy `.devcontainer/dataSettings.json.template` → `src/Presentation/Nop.Web/App_Data/dataSettings.json`, fill in SQL Server connection details, and copy `.devcontainer/plugins.json` → `App_Data/plugins.json`.

## Solution Structure

**Solution:** `src/NopCommerce.sln`

| Group | Projects |
|---|---|
| Libraries | `Nop.Core`, `Nop.Data`, `Nop.Services` |
| Presentation | `Nop.Web` (public site + admin), `Nop.Web.Framework` |
| Tests | `Nop.Tests` (NUnit 4, Moq, FluentAssertions 6, SQLite in-memory) |
| Plugins | 30 plugin projects in `src/Plugins/` |

## Architecture Overview

### Layered Architecture

- **Nop.Core** — Domain entities (extend `BaseEntity`), caching abstractions, event infrastructure, `ITypeFinder` for assembly scanning
- **Nop.Data** — Data access via **linq2db** (not EF Core). Schema changes use **FluentMigrator** migrations. Supports SQL Server (primary), MySQL, and PostgreSQL. Tests use `SqLiteNopDataProvider`.
- **Nop.Services** — All business logic. Services are registered via Autofac or standard DI and resolved through `IEngineContext`.
- **Nop.Web.Framework** — Base controllers, view components, admin menu system, `INopStartup` middleware pipeline wiring, CSS/JS bundling (WebOptimizer), HTML minification (WebMarkupMin).
- **Nop.Web** — MVC controllers, views, factories, and `wwwroot` static assets.

### Application Startup

`Program.cs` uses the minimal top-level host pattern (no `Startup.cs`):
1. `services.ConfigureApplicationSettings(builder)` — registers `ITypeFinder`, binds all `IConfig` implementations
2. Optionally uses Autofac via `CommonConfig.UseAutofac` in `appsettings.json`; otherwise falls back to default ASP.NET DI with scope validation disabled
3. `app.ConfigureRequestPipeline()` — discovers and runs all `INopStartup` implementations ordered by priority
4. `app.StartEngineAsync()` — installs/updates plugins, inserts ACL permissions, runs FluentMigrator migrations

### Plugin Architecture

Plugins are the primary extension mechanism:
- Each plugin has a `plugin.json` manifest: `SystemName`, `FriendlyName`, `Version`, `SupportedVersions: ["4.80"]`, `FileName` (output DLL)
- Plugin `.csproj` sets `<OutputPath>` to `..\..\Presentation\Nop.Web\Plugins\{Folder}` so built DLLs land directly in the web app
- `<CopyLocalLockFileAssemblies>false</CopyLocalLockFileAssemblies>` prevents duplicating framework DLLs
- A post-build `NopTarget` MSBuild target runs `ClearPluginAssemblies.proj` to strip unnecessary DLLs from plugin output
- Plugins implement `BasePlugin` + one or more interfaces: `IMiscPlugin`, `IPaymentMethod`, `IShippingRateComputationMethod`, `ITaxProvider`, `IWidgetPlugin`, `IAdminMenuPlugin`
- Plugins can implement `IConsumer<T>` for event-driven behavior (domain events: `EntityInsertedEvent<T>`, `EntityUpdatedEvent<T>`, `EntityDeletedEvent<T>`)
- Runtime plugin list is managed via `App_Data/plugins.json`

**Naming conventions:**
- `Nop.Plugin.{Type}.{Name}` — standard nopCommerce-style
- `AbcWarehouse.Plugin.{Type}.{Name}` — newer ABC Warehouse plugins

**Key ABC plugins:**
- `Nop.Plugin.Misc.AbcCore` (DisplayOrder: -1, loads first) — shared utilities, constants, helpers used by all other ABC plugins. Its post-build also writes `branch.txt` and `sha.txt` from git for a build-info endpoint.
- `Nop.Plugin.Misc.AbcFrontend` — frontend overrides and customizations
- `SevenSpikes.Theme.Pavilion` — active storefront theme (pre-built DLL, not in source)

**Widget zones:** Plugins implementing `IWidgetPlugin` return zone names from `GetWidgetZones()` and a ViewComponent name from `GetWidgetViewComponentName()`. Zone placeholders live in theme views as `@await Component.InvokeAsync("Widget", new { widgetZone = "..." })`.

### Theme

The active theme is **Pavilion** by SevenSpikes (pre-built binary at `src/Presentation/Nop.Web/Plugins/SevenSpikes.Theme.Pavilion/`). Theme view overrides live in `src/Presentation/Nop.Web/Themes/Pavilion/`.

### Data Access Pattern

nopCommerce 4.80 uses **linq2db** for all ORM operations, replacing EF Core from older versions. Schema changes are written as **FluentMigrator** migrations, not EF Core migrations.

### Dependency Injection

Autofac 10.0.0 is supported but optional (toggle `CommonConfig.UseAutofac`). Plugin-specific services are registered in each plugin's `DependencyRegistrar` class (implements `IDependencyRegistrar`).

## Configuration Files

All in `src/Presentation/Nop.Web/App_Data/` (excluded from source control except non-sensitive files):
- `appsettings.json` — hosting, Redis cache, Azure Blob, plugin shadow copy settings
- `dataSettings.json` — DB connection string + provider (`"DataProvider": "sqlserver"`)
- `plugins.json` — installed/pending-uninstall/pending-delete plugin lists
- `Localization/` — XML language resource files (managed via `ILocalizationService`)

The publish profile excludes all `*.json` from `App_Data/` and never copies `DataProtectionKeys/`.

## Key Dependencies

| Package | Version | Purpose |
|---|---|---|
| linq2db | 5.4.1 | ORM (replaces EF Core) |
| FluentMigrator | 6.2.0 | DB schema migrations |
| Autofac.Extensions.DependencyInjection | 10.0.0 | DI container (optional) |
| AutoMapper | 13.0.1 | Object mapping |
| FluentValidation.AspNetCore | 11.3.0 | Model validation |
| LigerShark.WebOptimizer.Core | 3.0.426 | CSS/JS bundling |
| WebMarkupMin | 2.18.0 | HTML minification |
| SkiaSharp | 2.88.9 | Image processing |
| QuestPDF | 2022.12.15 | PDF generation |
| NUnit | 4.2.2 | Test framework |
| Moq | 4.20.72 | Mocking |
| FluentAssertions | 6.12.2 | Test assertions |

## Deployment

**Servers:**
- ABC Prod: 163.123.137.18 | ABC Stage: 163.123.137.99
- HAW Prod: 163.123.137.41 | Mickey Prod: 163.123.137.44

**Process** (run as Administrator on the server):
```powershell
cd C:\Users\xby2\nopCommerce
git pull
.\deploy.ps1   # IISAppPoolName env var must be set
```

`deploy.ps1` sequence: debug build → release build → stop IIS app pool → delete `C:/NopABC/Plugins` → publish to `C:/NopABC` → start app pool. Expect ~30s downtime, 2–5 min reduced performance.

**Mickey Shorr special case:** Revert `web.config` after deployment to preserve redirect rules.

## CI/CD

`.github/workflows/e2e-nightly.yml` — runs Playwright E2E tests nightly at 2:00 AM UTC against `https://www.abcwarehouse.com` across chromium/firefox/webkit. On failure, sends email via Resend API (`secrets.RESEND_API_KEY`). Artifacts retained 30 days. No automated build/deploy pipeline — deployments are manual via `deploy.ps1`.

## Version Control

- **Main branch:** primary development and production branch
- **PR process:** feature branch → PR to main → code review → merge
- **Commit style:** descriptive messages with PR numbers (e.g., `Fix mattress savings calculations (#226)`)
