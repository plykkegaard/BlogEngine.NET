# BlogEngine.NET project instructions

This project is the ASP.NET Web Forms web application for BlogEngine.NET and targets .NET Framework 4.8.

- Preserve the existing Web Forms architecture: `.aspx` and `.ascx` pages, code-behind classes, `Global.asax`, `Web.config`, and `App_Code`/`AppCode` conventions.
- Keep the current folder structure intact (`Account`, `admin`, `AppCode`, `Custom`, `Content`, `Scripts`, `setup`) and prefer to extend existing files rather than introducing new app patterns.
- When changing UI or page logic, follow the established server-control and code-behind patterns already used by the project. Avoid introducing ASP.NET Core, MVC, or modern host-based patterns unless a file already uses them.
- Keep the existing XML-based content storage, provider-style services, and legacy package management approach compatible with the current solution. Avoid replacing these with new abstractions unless the change truly requires it.
- Follow the existing namespaces and naming conventions in the codebase and prefer small, focused changes over broad rewrites.
