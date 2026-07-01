# BlogEngine.NET skills

## Primary skills
- Web Forms and ASP.NET page lifecycle debugging for `.aspx`, `.ascx`, and code-behind files.
- Working with the legacy admin experience, including the AngularJS-based admin UI under `admin/app` and the server-rendered account flow under `Account`.
- Preserving compatibility with the existing `Web.config`, `Global.asax`, and custom handlers/modules.
- Maintaining the current content and theme model for pages, posts, widgets, and extensions.

## Implementation guidance
- Prefer incremental edits that stay aligned with the existing Web Forms structure rather than introducing MVC or ASP.NET Core patterns.
- Keep custom assets, theme files, and resource paths intact when changing UI or rendering code.
- Preserve the existing API controller structure and route behavior when changing admin or public-facing features.
