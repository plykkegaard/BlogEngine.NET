# Copilot Instructions for BlogEngine.NET

## Project Context
- Repository: `BlogEngine.NET`
- Solution: Multi-project ASP.NET Web Forms blogging engine
- Primary framework target: **.NET Framework 4.8**
- Main purpose: Open-source blogging platform with extensible architecture
- Git remote: `https://github.com/plykkegaard/BlogEngine.NET`

## Solution Structure
The solution contains three main projects:

### 1. BlogEngine.Core (Class Library)
Core business logic, data providers, and service abstractions.
- Targets .NET Framework 4.8
- Provider-based architecture (`BlogProvider`, `XmlProvider`, `DbProvider`)
- See `BlogEngine\BlogEngine.Core\copilot-instructions.md` for detailed guidance

### 2. BlogEngine.NET (Web Application)
Main ASP.NET Web Forms web application.
- Targets .NET Framework 4.8
- Web Forms architecture with `.aspx`, `.ascx`, code-behind
- Admin interface using AngularJS
- See `BlogEngine\BlogEngine.NET\copilot-instructions.md` for detailed guidance

### 3. BlogEngine.Tests (Test Project)
MSTest-based unit and integration tests.
- Targets .NET Framework 4.8
- Uses MSTest framework with Fakes for mocking
- See `BlogEngine\BlogEngine.Tests\copilot-instructions.md` for detailed guidance

## .NET Framework 4.8 Standard Copilot Guidance
- This is a **legacy .NET Framework** project, not modern .NET Core/5+
- Use `packages.config` for NuGet package management (not PackageReference)
- Avoid suggesting async/await patterns unless they already exist in the file
- Keep Web.config-based configuration (not appsettings.json)
- Respect Web Forms lifecycle and event-driven patterns
- Use `System.Web` namespaces, not ASP.NET Core equivalents
- Do not suggest `ILogger<T>`, `IConfiguration`, or dependency injection containers unless they're already established in the codebase

## Architecture Patterns
- **Provider Model**: Core data access uses a provider pattern with XML and database options
- **Web Forms**: UI uses traditional ASP.NET Web Forms with server controls and ViewState
- **AngularJS Admin**: Admin interface (`/admin`) uses AngularJS 1.x for SPA-like experience
- **Extension System**: Supports custom widgets, themes, and extensions
- **Multi-blog**: Supports multiple blogs in a single installation

## Coding Conventions
- Follow existing namespace patterns (`BlogEngine.Core`, `BlogEngine.Core.Data`, etc.)
- Preserve XML documentation comments on public APIs
- Use established naming conventions in each project
- Maintain compatibility with existing provider contracts and data models
- Prefer incremental changes over major refactoring

## When Making Changes

### For .NET Code (C#)
- Preserve the existing architecture (Web Forms, providers, services)
- Do not introduce modern .NET patterns (Razor Pages, MVC controllers, minimal APIs) unless file already uses them
- Keep compatibility with .NET Framework 4.8 BCL
- Avoid suggesting `async Task` unless the surrounding code already uses it
- Respect existing patterns like `Page_Load`, provider abstractions, and XML-based storage

### For JavaScript/AngularJS Code
- Admin interface uses **AngularJS 1.x** (not modern Angular)
- Follow existing controller, service, and directive patterns
- Use established `$http`, `$scope`, and routing conventions
- Do not suggest TypeScript, modern Angular, React, or Vue unless explicitly requested

### For Web Forms (.aspx/.ascx)
- Maintain server control declarations and ViewState patterns
- Keep code-behind event handlers consistent with existing style
- Preserve master page inheritance and content placeholder structure

### For Database/Storage
- Respect both XML-based and SQL-based storage providers
- Do not break existing provider contracts
- Test changes against both XML and database storage modes when possible

## Testing
- Use MSTest conventions for unit tests
- Leverage existing Fakes for mocking dependencies
- Name tests consistently: `MethodName_Scenario_ExpectedBehavior`
- Keep tests focused on behavior, not implementation details

## Documentation
- Update XML comments for public APIs when adding or changing methods
- Include `<summary>` for all public types and members
- Add `<remarks>` for complex logic or important usage notes
- Update README or inline comments for significant architectural changes

## DO NOT Suggest
- Migrating to ASP.NET Core (this is a .NET Framework 4.8 project)
- Replacing Web Forms with Razor Pages or MVC
- Converting AngularJS to modern Angular/React/Vue
- Switching from `packages.config` to PackageReference
- Introducing dependency injection containers (Unity, Autofac, etc.)
- Using modern .NET features not available in .NET Framework 4.8

## WHEN TO SUGGEST
- Security improvements (XSS, SQL injection, etc.)
- Bug fixes that maintain compatibility
- Performance optimizations within existing patterns
- Accessibility improvements
- Code clarity and maintainability within established architecture
- New features that extend existing patterns (widgets, themes, providers)
