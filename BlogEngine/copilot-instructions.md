# BlogEngine.NET Solution-Level Copilot Instructions

## Solution Overview

**BlogEngine.NET** is a .NET Framework 4.8 blogging platform with three core projects:

1. **BlogEngine.Core** — Core library containing business logic, data access, providers, and services
2. **BlogEngine.NET** — ASP.NET Web Forms web application with MVC/AngularJS admin interface
3. **BlogEngine.Tests** — MSTest unit tests for core and API functionality

---

## Project Scope & Boundaries

### ✅ BlogEngine.Core (Class Library)
**Location:** `BlogEngine/BlogEngine.Core/`

**In Scope:**
- Business logic and domain models (Post, Page, Comment, Category, etc.)
- Data access layer (repositories, contracts, models)
- Provider-based architecture (XmlProvider, DbProvider, FileSystemProvider, CacheProvider)
- Services (Search, Packaging, Messaging, Security, Syndication)
- Web components and controls (Web/Controls, Web/HttpHandlers, Web/HttpModules)
- Metadata and SEO/GEO management
- Helpers and utilities

**Out of Scope (DO NOT CREATE):**
- ❌ `.aspx`, `.ascx`, `.master` files (these belong in BlogEngine.NET)
- ❌ Web Forms application-specific code
- ❌ Admin UI views or AngularJS controllers
- ❌ Global.asax or Web.config (application config)
- ❌ Script files (`*.js`, `*.css`) — keep in BlogEngine.NET
- ❌ Theme or Widget customizations (use CustomProvider pattern)

---

### ✅ BlogEngine.NET (ASP.NET Web Application)
**Location:** `BlogEngine/BlogEngine.NET/`

**In Scope:**
- Web Forms pages (`.aspx`, `.ascx`, `.master`)
- Admin UI (AngularJS app in `admin/app/`)
- HTML editors and themes
- Scripts, styles, and media assets
- Global.asax and Web.config
- REST API controllers (AppCode/Api/)
- App_Data (XML storage, configuration)
- Custom controls, extensions, widgets, and themes
- Resource files for multi-language support

**Out of Scope (DO NOT CREATE):**
- ❌ Core business logic (belongs in BlogEngine.Core)
- ❌ Data repository interfaces or implementations (belongs in BlogEngine.Core)
- ❌ Provider abstract classes (belongs in BlogEngine.Core)
- ❌ Unit tests (use BlogEngine.Tests)
- ❌ Compiled/built files or bin/obj directories (build artifacts)
- ❌ External libraries (managed by NuGet via packages.config)

---

### ✅ BlogEngine.Tests (Unit Test Library)
**Location:** `BlogEngine/BlogEngine.Tests/`

**In Scope:**
- MSTest test classes (`*ControllerTests`, `*ValidatorTests`)
- Fake/mock repositories (in Fakes/ directory)
- Security and validation tests
- API controller tests

**Out of Scope (DO NOT CREATE):**
- ❌ Production code (belongs in Core or BlogEngine.NET)
- ❌ Integration tests requiring external services (test only in-memory mocks)
- ❌ Performance benchmarks without explicit request
- ❌ Non-test code files

---

## What NOT to Create at Solution Root

The following should **NEVER** be created directly under `BlogEngine/`:

- ❌ Loose `.cs` files (use appropriate project)
- ❌ Documentation outside `*.md` files (readme.md, instructions.md, etc. are okay)
- ❌ Configuration files like `web.config` (only in BlogEngine.NET)
- ❌ Additional projects without explicit request
- ❌ Temporary or scratch files
- ❌ Build outputs or NuGet packages
- ❌ IDE/editor specific directories (`.vs`, `.vscode` — use .gitignore)

---

## Where Code Should Go

### New Business Logic or Models
→ **BlogEngine.Core** (appropriate subfolder)

**Example:** Adding a new `Author` repository
- Create: `BlogEngine/BlogEngine.Core/Data/Contracts/IAuthorRepository.cs`
- Create: `BlogEngine/BlogEngine.Core/Data/AuthorRepository.cs`
- Create: `BlogEngine/BlogEngine.Core/Data/Models/AuthorItem.cs`

### New Web Page or Admin UI Feature
→ **BlogEngine.NET**

**Example:** Adding a new admin settings page
- Create: `BlogEngine/BlogEngine.NET/admin/app/settings/newSettingView.html`
- Create: `BlogEngine/BlogEngine.NET/admin/app/settings/newSettingController.js`
- Modify: `BlogEngine/BlogEngine.NET/AppCode/Api/SettingsController.cs` (if API endpoint needed)

### New Tests for Existing Code
→ **BlogEngine.Tests**

**Example:** Adding tests for BlogsController
- Create: `BlogEngine/BlogEngine.Tests/WebApi/BlogControllerTests.cs`
- Use fakes from `BlogEngine/BlogEngine.Tests/Fakes/` for mocking

### New Provider or Service
→ **BlogEngine.Core** in appropriate provider/services folder

**Example:** New file system provider
- Create: `BlogEngine/BlogEngine.Core/Providers/FileSystemProviders/NewFileSystemProvider.cs`

---

## Cross-Project Dependencies

```
BlogEngine.NET (Web Application)
	↓ references
BlogEngine.Core (Class Library)

BlogEngine.Tests (Test Library)
	↓ references
BlogEngine.Core & BlogEngine.NET (for testing)
```

**Rule:** Do NOT create circular dependencies or add BlogEngine.NET-specific code to BlogEngine.Core.

---

## Individual Project Instructions

Each project has its own detailed `copilot-instructions.md` file:

- **[BlogEngine.Core Instructions](./BlogEngine.Core/copilot-instructions.md)** — Namespace layout, patterns, SEO/GEO guidelines
- **[BlogEngine.NET Instructions](./BlogEngine.NET/copilot-instructions.md)** — Web Forms patterns, Web Application structure
- **[BlogEngine.Tests Instructions](./BlogEngine.Tests/copilot-instructions.md)** — Test organization, MSTest conventions

Refer to these for detailed patterns, naming conventions, and architectural guidelines within each project.

---

## Git & Repository Notes

- **Repository:** https://github.com/plykkegaard/BlogEngine.NET
- **Current Branch:** master
- **Target Framework:** .NET Framework 4.8
- **Build Output:** Ignored in `.gitignore` (don't commit `bin/`, `obj/`, or `.vs/`)
- **NuGet Packages:** Managed via `packages.config` (do NOT manually edit)

---

## Quick Reference Checklist

Before creating a file ask yourself:

- [ ] Does this belong in BlogEngine.Core, BlogEngine.NET, or BlogEngine.Tests?
- [ ] Am I adding code to the right project structure?
- [ ] Is this file outside the project scope?
- [ ] Have I checked the individual project's copilot-instructions.md?
- [ ] Will this create a circular dependency?
- [ ] Should this go in a `Custom/` folder for user extensions?

If unsure, refer to the appropriate project's `copilot-instructions.md` or ask for clarification.
