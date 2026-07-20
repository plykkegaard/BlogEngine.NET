# BlogEngine.Core project instructions

This project is the core class library for BlogEngine.NET and targets .NET Framework 4.8.

## Namespace & Directory Structure

```
BlogEngine.Core (root namespace)
├── API/MetaWeblog/                  → BlogEngine.Core.API.MetaWeblog
│   └── MetaWeblog protocol handling (MWA*, MetaWeblogHandler, XMLRPC)
├── Data/                            → BlogEngine.Core.Data
│   ├── Contracts/                   → BlogEngine.Core.Data.Contracts (IRepository interfaces)
│   ├── Models/                      → BlogEngine.Core.Data.Models (DTO/ViewModel classes)
│   ├── Services/                    → BlogEngine.Core.Data.Services (Avatar, Json, TagCloud, Updater)
│   ├── ViewModels/                  → BlogEngine.Core.Data.ViewModels (VM classes)
│   └── *Repository.cs               → BlogEngine.Core.Data (repository implementations)
├── Metadata/                        → BlogEngine.Core.Metadata
│   ├── Schemas/                     → BlogEngine.Core.Metadata.Schemas
│   └── SEO/GEO metadata management
├── Helpers/                         → BlogEngine.Core.Helpers (Pager, Utils, BlogGenerator, Events)
├── Providers/                       → BlogEngine.Core.Providers
│   ├── BlogProvider/                → BlogEngine.Core.Providers (abstract provider base)
│   ├── CacheProvider/               → BlogEngine.Core.Providers.CacheProvider
│   ├── DbProvider/                  → BlogEngine.Core.Providers.DbProvider
│   ├── FileSystemProviders/         → BlogEngine.Core.Providers.FileSystemProviders
│   └── XmlProvider/                 → BlogEngine.Core.Providers.XmlProvider
├── Services/                        → BlogEngine.Core.Services
│   ├── Compilation/                 → BlogEngine.Core.Services.Compilation (expression builders)
│   ├── FileSystem/                  → BlogEngine.Core.Services.FileSystem
│   ├── Messaging/                   → BlogEngine.Core.Services.Messaging (Ping, Protection)
│   ├── Packaging/                   → BlogEngine.Core.Services.Packaging
│   ├── Search/                      → BlogEngine.Core.Services.Search
│   ├── Security/                    → BlogEngine.Core.Services.Security
│   └── Syndication/                 → BlogEngine.Core.Services.Syndication (BlogML)
├── Web/                             → BlogEngine.Core.Web
│   ├── Controls/                    → BlogEngine.Core.Web.Controls (BasePage, CommentForm, PostView, etc.)
│   ├── Extensions/                  → BlogEngine.Core.Web.Extensions (plugins, DataStore, Widgets)
│   ├── HttpHandlers/                → BlogEngine.Core.Web.HttpHandlers (handlers for feeds, images, JS, etc.)
│   ├── HttpModules/                 → BlogEngine.Core.Web.HttpModules (compression, rewriting, security)
│   └── Scripting/                   → BlogEngine.Core.Web.Scripting
├── Root classes
│   ├── AuthorProfile.cs             → BlogEngine.Core.AuthorProfile
│   ├── Blog.cs                      → BlogEngine.Core.Blog
│   ├── BlogConfig.cs                → BlogEngine.Core.BlogConfig
│   ├── BlogSettings.cs              → BlogEngine.Core.BlogSettings
│   ├── BusinessBase.cs              → BlogEngine.Core.BusinessBase (change tracking base)
│   ├── Category.cs                  → BlogEngine.Core.Category
│   ├── Comment.cs                   → BlogEngine.Core.Comment
│   ├── IPublishable.cs              → BlogEngine.Core.IPublishable (interface)
│   ├── Page.cs                      → BlogEngine.Core.Page
│   ├── Post.cs                      → BlogEngine.Core.Post
│   ├── Referrer.cs                  → BlogEngine.Core.Referrer
│   ├── Role.cs                      → BlogEngine.Core.Role
│   └── BlogRollItem.cs              → BlogEngine.Core.BlogRollItem
```

## Core Architecture & Patterns
- Preserve the existing provider-based architecture such as `BlogProvider`, `XmlProvider`, `DbProvider`, and the repository/service abstractions under `Data`, `Providers`, `Services`, `Web`, and `API`.
- Keep the existing content and configuration models intact, including XML-based storage and the current provider contracts. Avoid changing data contracts unless a change specifically requires it.
- Follow the established namespace layout and naming patterns in the library instead of introducing new abstractions or frameworks.
- Prefer incremental changes that fit the current architecture. Avoid introducing modern .NET abstractions, dependency injection containers, or new package patterns unless they are already used in the project.
- Maintain compatibility with the solution's legacy assemblies and `packages.config`-based dependency setup.

## SEO & GEO (Generative Engine Optimization) Guidelines

### What is GEO?
**GEO (Generative Engine Optimization)** is the practice of optimizing content for Generative AI systems (LLMs, RAG systems, AI search engines) in addition to traditional search engines. This includes:
- Structured metadata that AI systems can parse and understand
- Clear entity relationships and semantic annotations
- Optimized content hints for generative summarization
- Schema.org markup for rich semantic understanding

### Blog-Level SEO/GEO (BlogSettings)
The `BlogSettings` class should support blog-wide SEO/GEO defaults that apply across all posts:
- **Schema.org Organization metadata**: `SchemaOrgName`, `SchemaOrgType`, `SchemaOrgUrl`
- **AI-friendly metadata**: `EnableGEO`, `GenerativeAIKeywords`, `EntityHints`
- **Search engine configuration**: `CanonicalUrlBase`, `BreadcrumbEnabled`
- **Structured data**: `GoogleAIDomainVerification`, `SitemapPolicy`

New blog-level GEO properties should be optional and default to empty/false for backward compatibility.

### Post-Level SEO/GEO (Post)
Each `Post` instance should support per-article GEO customization:
- **Content metadata**: `SchemaType` (e.g., "BlogPosting", "Article"), `Authors` (structured), `DatePublished`, `DateModified`
- **AI optimization**: `KeyEntities`, `SemanticSummary`, `ContentMSL` (Main Subject Line)
- **Search & discovery**: `CanonicalUrl`, `MetaKeywords`, `MetaRobots`, `OpenGraphData`
- **Structured data**: `BreadcrumbLabel`, `SchemaOrganization`

GEO properties should be stored and serialized using the existing XML provider infrastructure, following the pattern of current Post properties (e.g., `description`, `author`).

### Naming & State Management
- Use PascalCase for property names following existing BlogSettings/Post conventions
- Use the `SetValue()` method in Post for change tracking (inherited from `BusinessBase<T>`)
- In BlogSettings, use auto-property getters/setters as established in the class
- Document all SEO/GEO properties with XML comments explaining their purpose for search engines and AI systems

### Backward Compatibility
- All GEO properties should be optional (empty strings or false)
- Existing blogs without GEO configuration should function normally
- GEO properties are enhancements, not requirements
