# BlogEngine.NET project instructions

This project is the ASP.NET Web Forms web application for BlogEngine.NET and targets .NET Framework 4.8.

## Namespace & Directory Structure

```
BlogEngine.NET (root namespace)

Web Application Directories:
├── Account/                         → AccountPages & code-behind
│   └── Login, Register, ChangePassword, etc. (.aspx/.master)
├── admin/                           → Administration UI (mixed Web Forms & MVC)
│   ├── app/                         → AngularJS/MVC admin interface
│   │   ├── content/                 → Content management (blogs, posts, pages, categories, comments, tags)
│   │   ├── custom/                  → Plugins, Themes, Widgets management
│   │   ├── security/                → Users, Roles, Profile management
│   │   ├── settings/                → Blog settings UI
│   │   ├── dashboard/               → Dashboard view
│   │   └── editor/                  → Post/Page editor
│   ├── editors/                     → HTML editors (tinymce, summernote, bootstrap-wysiwyg)
│   ├── themes/                      → Admin themes (standard theme with SCSS)
│   ├── Extensions/                  → Admin extension settings UI
│   ├── menu.ascx                    → Admin menu control
│   └── index.cshtml                 → Admin entry point
├── AppCode/                         → Compiled code (Auto-compiled in Web Forms)
│   ├── Api/                         → ApiControllers (BlogsController, PostsController, etc.)
│   ├── App_Start/                   → BlogEngineConfig.cs (MVC/WebAPI initialization)
│   ├── Controls/                    → User controls (MobileThemeSwitch, PageMenu, WidgetBase, etc.)
│   └── Wlw/                         → Windows Live Writer support
├── App_Data/                        → XML storage & application data
│   ├── blogs/                       → Multi-blog data storage
│   ├── datastore/                   → Widget data storage
│   ├── profiles/                    → User profiles
│   ├── posts/                       → Blog posts (XML)
│   └── *.xml                        → Configuration XML files
├── App_GlobalResources/             → Global resource files (multi-language labels)
├── Content/                         → CSS & Images
│   ├── Auto/                        → Auto-generated CSS
│   ├── images/blog/                 → Blog images, avatars, icons, flags
│   └── *.css                        → Bootstrap, Font Awesome, Toastr styles
├── Custom/                          → Customizable blog components
│   ├── Controls/                    → Custom user controls & defaults
│   ├── Extensions/                  → Blog extensions (plugins, filters, captcha)
│   ├── Media/                       → Sample media files
│   ├── Themes/                      → Blog themes (RazorHost, SimpleBlue, Standard, etc.)
│   └── Widgets/                     → Blog widgets (BlogRoll, CategoryList, PostList, etc.)
├── Scripts/                         → JavaScript
│   ├── Auto/                        → Auto-bundled scripts
│   ├── angular*/                    → AngularJS library
│   ├── i18n/                        → AngularJS localization
│   ├── jQuery/                      → jQuery library
│   ├── mediaelement/                → Media player
│   ├── syntaxhighlighter/           → Code highlighting
│   └── *.js                         → Bootstrap, moment.js, toastr, and admin scripts
├── setup/                           → Database setup scripts
│   ├── MySQL/
│   ├── SQL_CE/
│   ├── SQLite/
│   ├── SQLServer/
│   └── upgrade/                     → Database upgrade utilities
├── fonts/                           → Font files (FontAwesome, Bootstrap glyphicons)
├── Properties/                      → AssemblyInfo.cs
├── Global.asax                      → HttpApplication entry point
├── Web.config                       → Configuration
├── Web.sitemap                      → Site map for navigation
└── Root .aspx pages
    ├── default.aspx                 → Home page
    ├── post.aspx, page.aspx        → Content display pages
    ├── search.aspx, archive.aspx   → Search and archives
    ├── contact.aspx                 → Contact form
    └── error*.aspx                  → Error pages
```

## Key Namespaces

- **BlogEngine.NET.Api** (AppCode/Api/) — REST API controllers for admin UI
- **BlogEngine.NET.App_Start** — MVC/WebAPI configuration
- **BlogEngine.NET.Controls** — Reusable user controls
- **BlogEngine.NET** (root) — Main page code-behind classes

- Preserve the existing Web Forms architecture: `.aspx` and `.ascx` pages, code-behind classes, `Global.asax`, `Web.config`, and `App_Code`/`AppCode` conventions.
- Keep the current folder structure intact (`Account`, `admin`, `AppCode`, `Custom`, `Content`, `Scripts`, `setup`) and prefer to extend existing files rather than introducing new app patterns.
- When changing UI or page logic, follow the established server-control and code-behind patterns already used by the project. Avoid introducing ASP.NET Core, MVC, or modern host-based patterns unless a file already uses them.
- Keep the existing XML-based content storage, provider-style services, and legacy package management approach compatible with the current solution. Avoid replacing these with new abstractions unless the change truly requires it.
- Follow the existing namespaces and naming conventions in the codebase and prefer small, focused changes over broad rewrites.
