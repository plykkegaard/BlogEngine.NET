# BlogEngine.Core skills

## Primary skills
- Provider-based architecture for blogs, posts, pages, categories, comments, roles, users, widgets, and settings.
- XML and file-system backed persistence, including providers under `Providers/XmlProvider`, `Providers/DbProvider`, and `Providers/FileSystemProviders`.
- Core domain models and services such as blog content, security, syndication, packaging, and search.
- Legacy .NET Framework 4.8 compatibility, including `packages.config`-based dependencies and older ASP.NET integration points.

## Implementation guidance
- Keep existing provider contracts and repository abstractions intact unless a task explicitly requires a change.
- Preserve the current data model and serialization behavior when editing providers or services.
- Prefer small, architecture-aligned changes over broad rewrites or new dependency injection patterns.
- **Lazy ponytail approach**: Write minimal code, reuse what exists, avoid abstractions and optimizations unless immediately necessary.
