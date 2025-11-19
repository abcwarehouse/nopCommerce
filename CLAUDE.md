# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

This is ABC Warehouse's customized nopCommerce 4.40 e-commerce platform built on ASP.NET Core 5.0 (.NET 5.0). The codebase uses a layered architecture with extensive plugin-based extensibility. The solution includes core nopCommerce libraries plus 35+ custom plugins for ABC Warehouse-specific functionality.

## Build & Run Commands

### Build
```powershell
# Clean and build (Debug)
dotnet clean src/NopCommerce.sln
dotnet build src/NopCommerce.sln

# Clean and build (Release)
dotnet clean src/NopCommerce.sln -c Release
dotnet build src/NopCommerce.sln -c Release
```

### Run Locally
```powershell
# VSCode: Press F5 (configured with .NET debugger)
# Or manually:
cd src/Presentation/Nop.Web
dotnet run
```

### Test
```powershell
# Run all tests (NUnit)
dotnet test src/Tests/Nop.Tests/Nop.Tests.csproj

# E2E tests (Playwright) - requires Node.js
npm ci
npx playwright install --with-deps
npx playwright test
```

### Publish
```powershell
dotnet publish -c Release ./src/Presentation/Nop.Web/Nop.Web.csproj --no-restore -o <output-path>
```

## Solution Structure

**Main Solution:** `src/NopCommerce.sln`

The solution is organized into 4 main groups:

1. **Libraries** - Core framework layers
   - `Nop.Core` - Domain models, caching, events, infrastructure
   - `Nop.Data` - Entity Framework Core data access layer
   - `Nop.Services` - Business logic services

2. **Presentation** - Web application
   - `Nop.Web` - Main ASP.NET Core MVC application (public site + admin)
   - `Nop.Web.Framework` - Shared framework, base classes, middleware

3. **Plugins** - 35+ plugin projects (both standard nopCommerce and ABC Warehouse custom)

4. **Tests** - `Nop.Tests` - Unit and integration tests using NUnit, Moq, FluentAssertions

## Architecture Overview

### Layered Architecture

- **Nop.Core** - Contains domain entities (`BaseEntity`), configuration, caching abstractions, event system, and dependency injection setup using Autofac
- **Nop.Data** - EF Core DbContext, entity configurations, migrations, and Linq-based data access
- **Nop.Services** - Service layer with business logic for products, orders, customers, payments, shipping, taxes, localization, logging, and plugin management
- **Nop.Web.Framework** - Web infrastructure including startup configuration, base controllers, view components, admin menu system, and authentication/authorization setup
- **Nop.Web** - MVC controllers, views, models, factories, themes, and wwwroot static assets

### Plugin Architecture

Plugins are the primary extension mechanism. Each plugin:
- Contains a `plugin.json` manifest with metadata (SystemName, Version, DisplayOrder, etc.)
- Inherits from `BasePlugin` and implements one or more interfaces: `IMiscPlugin`, `IPaymentMethod`, `IShippingRateComputationMethod`, `ITaxProvider`, `IWidgetPlugin`, `IAdminMenuPlugin`
- Can implement `IConsumer<T>` for event-driven behavior
- Compiles to `src/Presentation/Nop.Web/Plugins/{PluginSystemName}/`
- Registered in `App_Data/plugins.json` for runtime loading

**Key ABC Warehouse Plugin:**
- `Nop.Plugin.Misc.AbcCore` (DisplayOrder: -1) - Loads first, provides core utilities, constants, helpers, and shared business logic for other ABC plugins

### Configuration Files

All located in `src/Presentation/Nop.Web/App_Data/`:
- `appsettings.json` - Application configuration
- `dataSettings.json` - Database connection string (SQL Server)
- `plugins.json` - Installed plugins list
- `DataProtectionKeys/` - Encryption keys
- `Localization/` - Language resource XML files

### Dependency Injection

Uses **Autofac** as the DI container. Service registration occurs in:
- `Program.cs` - Configures `UseServiceProviderFactory(new AutofacServiceProviderFactory())`
- `Startup.cs` - Calls `services.ConfigureApplicationServices()` extension method
- `Nop.Web.Framework/Infrastructure/Extensions/ServiceCollectionExtensions.cs` - Core service registration
- Plugin `DependencyRegistrar` classes for plugin-specific services

### Event System

Event-driven architecture using `IConsumer<T>` pattern:
- Domain events: `EntityInsertedEvent<T>`, `EntityUpdatedEvent<T>`, `EntityDeletedEvent<T>`
- Plugins implement `IConsumer<T>` to react to events
- Events published via `IEventPublisher` service

### Widget System

Plugins implementing `IWidgetPlugin` can register for widget zones (placeholders in themes):
- `GetWidgetZones()` returns list of zones the plugin renders in
- `GetWidgetViewComponentName()` returns the ViewComponent to render

## Development Workflows

### Local Development Setup

**Prerequisites:**
- .NET 5.0 SDK
- SQL Server database (requires existing database backup)
- Copy `dataSettings.json` and `plugins.json` to `src/Presentation/Nop.Web/App_Data/`

**Codespace Setup:**
```bash
# 1. Create GitHub Codespace (8 CPU, 32GB RAM recommended)
# 2. Run initialization script
bash ./.devcontainer/init.sh
# Prompts for SQL Server IP and password, configures dataSettings.json
```

### Plugin Development

**Naming Convention:**
- Standard: `Nop.Plugin.{Type}.{Name}`
- ABC Warehouse: `Nop.Plugin.Misc.{Name}` or `AbcWarehouse.Plugin.{Type}.{Name}`

**Plugin Structure:**
```
PluginFolder/
├── plugin.json (metadata: SystemName, Version, DisplayOrder, etc.)
├── logo.jpg
├── {PluginName}.csproj
├── Components/ (ViewComponents)
├── Controllers/ (MVC controllers)
├── Models/ (data/view models)
├── Views/ (Razor templates)
├── Infrastructure/ (DI registration)
├── Services/ (business logic)
├── Domain/ (entities)
├── Data/ (data access)
└── Areas/Admin/ (admin interface)
```

**Plugin Base Implementation:**
```csharp
public class MyPlugin : BasePlugin, IMiscPlugin
{
    public override async Task InstallAsync()
    {
        // Install plugin (create tables, settings, etc.)
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        // Remove plugin data
        await base.UninstallAsync();
    }
}
```

### Deployment Process

**Production/Stage Servers:**
- ABC Prod: 163.123.137.18
- ABC Stage: 163.123.137.99
- HAW Prod: 163.123.137.41
- Mickey Prod: 163.123.137.44

**Deployment Steps (PowerShell as Administrator):**
```powershell
cd C:\Users\xby2\nopCommerce
git pull
.\deploy.ps1  # Runs clean, build, publish, and IIS App Pool recycle
```

**Deploy Script Process:**
1. Clean and build solution in Release mode
2. Stop IIS App Pool
3. Delete `C:/NopABC/Plugins` directory
4. Publish to `C:/NopABC`
5. Start IIS App Pool

**Note:** Site experiences ~30 seconds downtime and 2-5 minutes reduced performance after deployment.

**Special Case - Mickey Shorr:** Revert `web.config` changes after deployment to maintain redirect functionality.

## Key Technical Details

### Database
- **Provider:** SQL Server
- **Connection:** `Data Source=server,1433;Initial Catalog=NOPCommerce;User ID=sa;Password=...`
- **ORM:** Entity Framework Core (primary), Dapper (in AbcSync plugin)
- **Test DB:** SQLite in-memory for unit tests

### Dependencies
- Autofac 7.1.0 - DI container
- AutoMapper 8.1.1 - Object mapping
- Entity Framework Core 5.0
- Azure Data Protection - Key storage
- StackExchange.Redis - Caching
- RestSharp 106.10.1 - HTTP client
- Dapper 2.0.35 - Lightweight ORM
- NUnit 3.13.1 - Testing framework
- Moq 4.16.1 - Mocking
- FluentAssertions 5.10.3 - Test assertions

### Important Directories
- `src/Presentation/Nop.Web/App_Data/` - Configuration files (not in source control)
- `src/Presentation/Nop.Web/Plugins/` - Compiled plugin output (not in source control)
- `src/Presentation/Nop.Web/wwwroot/` - Static assets
- `src/Presentation/Nop.Web/Themes/` - Theme templates
- `sql/` - Database utility scripts

### Localization
- Resource files in `App_Data/Localization/` (XML format)
- Managed via `ILocalizationService`
- Plugins can register custom localization strings during installation

## Common Development Tasks

### Adding a New Plugin
1. Create new project under appropriate plugin folder in `src/Plugins/`
2. Reference `Nop.Web.Framework` (and other necessary projects)
3. Create `plugin.json` with unique SystemName
4. Implement `BasePlugin` and appropriate interfaces
5. Build solution - plugin outputs to `Nop.Web/Plugins/{SystemName}/`
6. Add to `plugins.json` to enable at runtime

### Modifying Core Entities
1. Update entity class in `Nop.Core/Domain/`
2. Update entity configuration in `Nop.Data/Mapping/`
3. Create EF Core migration
4. Update related services in `Nop.Services/`
5. Run tests to ensure backward compatibility

### Debugging
- Set breakpoints in VSCode and press F5
- MiniProfiler available for performance profiling
- Enhanced logging via `Nop.Plugin.Misc.EnhancedLogging`

### Working with Widget Zones
- Zones are defined in theme views (e.g., `@await Component.InvokeAsync("Widget", new { widgetZone = "categorydetails_bottom" })`)
- Recent addition: `categorydetails_bottom` widget zone for category pages
- Register zones in plugin's `GetWidgetZones()` method

## Version Control
- **Main Branch:** Primary development and production branch
- **PR Process:** Feature branch → PR to main → Code review → Merge
- **Commit Style:** Descriptive messages with PR numbers (e.g., "Fixes mattress savings story calculations (#226)")
