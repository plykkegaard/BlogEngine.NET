# BlogEngine.NET

<p>
    <strong>BlogEngine</strong> is an open source blogging platform since 2007. Easily customizable with many free built-in Themes, Widgets, and Plugins.
</p>

- **[Website](https://blogengine.io/)**
- **[Docs](https://blogengine.io/support/get-started/)**
- **[Themes](https://blogengine.io/themes/)**
- **[Custom Design Theme](https://blogengine.io/themes/custom/)**
- **[Contact us](https://blogengine.io/support/)**

## Project Structure

This solution contains three main projects:

- **BlogEngine.NET** - Main web application (ASP.NET Razor Pages, Web API, MVC)
- **BlogEngine.Core** - Core business logic and data access layer
- **BlogEngine.Tests** - Unit tests (xUnit framework)

## Technical Specifications

- **Framework:** .NET Framework 4.8
- **IDE Support:** Visual Studio 2015 or later (tested with Visual Studio 2026)
- **MSBuild Tools:** Version 12.0
- **Language:** C# with full XML documentation

## Documentation

- **[Settings.xml Configuration Reference](docs/SETTINGS-XML-REFERENCE.md)** — Complete guide to configuring SEO and Generative Engine Optimization (GEO) settings

## Recent Updates & Features

### Security Enhancements
- **XSS Protection** - Comprehensive cross-site scripting protection for post rendering and comment systems
- **Content Security** - Enhanced sanitization for user-generated content
- **File Upload Validation** - MIME type detection and security validation
- **Security Headers Module** - HTTP security headers implementation

### SEO & Modern Web Features
- **SEO/GEO Support** - Full Search Engine Optimization and Generative Engine Optimization
- **Schema.org Integration** - Rich structured data for search engines
- **Open Graph & Twitter Cards** - Enhanced social media sharing
- **Metadata Management** - Flexible blog-wide and post-level metadata

### Key Dependencies
- **AngularJS** 1.8.2 - Admin panel framework
- **jQuery** 3.7.1 - Client-side scripting
- **Bootstrap** 3.4.1 - Responsive UI framework
- **Newtonsoft.Json** 13.0.4 - JSON serialization
- **ASP.NET Web API** 5.3.0 / 6.0.0 - RESTful services
- **AntiXSS** 4.3.0 - Additional XSS protection
- **SimpleInjector** 4.10.2 - Dependency injection
- **xUnit** - Test framework

## Getting Started

### Requirements

#### Production
- Windows Hosting with ASP.NET 4.5 or above
- .NET Framework 4.8 runtime
- Write permissions on App_Data and Custom folders

#### Development
- Windows OS (Windows 10/11 recommended)
- Visual Studio 2015 or later (Visual Studio 2022/2026 recommended)
- .NET Framework 4.8 Developer Pack
- ASP.NET and Web Development workload

### Installation Steps

1. **Download** - Get the latest BlogEngine and extract to the root of your website
2. **Write Permissions** - Ensure write permissions on:
   - App_Data folder (for XML storage)
   - Custom folder (for themes, widgets, extensions)
3. **Configure Database** (Optional) - XML storage is default, but supports:
   - SQL Server
   - MySQL
   - SQLite
   - SQL Server Compact Edition
4. **Navigate to Admin** - Add `/admin/` to your website's URL
   - Example: `https://yourwebsite.com/admin/`
   - Default Username: `admin`
   - Default Password: `admin`

## Development

### Environment Setup

1. **Install Prerequisites:**
   - Visual Studio 2015 or later
   - .NET Framework 4.8 Developer Pack
   - ASP.NET and Web Development tools

2. **Clone and Build:**
   ```bash
   git clone https://github.com/plykkegaard/BlogEngine.NET.git
   cd BlogEngine.NET/BlogEngine
   ```

3. **Open Solution:**
   - Open `BlogEngine.sln` in Visual Studio
   - Restore NuGet packages
   - Build solution (Ctrl+Shift+B)

4. **Run Application:**
   - Press F5 to start debugging
   - Navigate to `http://localhost:64079/`
   - Admin panel: `http://localhost:64079/admin/`
   - Default credentials: `admin` / `admin`

### Running Tests

The solution includes comprehensive unit tests:

```bash
# In Visual Studio Test Explorer, or via command line:
dotnet test BlogEngine.Tests.csproj
```

Tests cover:
- Security features (XSS protection, file upload validation)
- Web API controllers
- Core business logic
- Data repositories

### Project Configuration

All three projects target **.NET Framework 4.8**:
- `BlogEngine.NET.csproj` - ToolsVersion 12.0
- `BlogEngine.Core.csproj` - ToolsVersion 12.0
- `BlogEngine.Tests.csproj` - ToolsVersion 12.0

## Technology Stack

### Backend
- **Framework:** ASP.NET Web Forms with Razor Pages and MVC
- **Language:** C# (.NET Framework 4.8)
- **API:** RESTful Web API with JSON
- **Dependency Injection:** SimpleInjector

### Frontend
- **UI Framework:** Bootstrap 3.4.1
- **JavaScript:** AngularJS 1.8.2, jQuery 3.7.1
- **Icons:** Font Awesome 4.7.0
- **Notifications:** Toastr 2.1.1

### Data Storage
- **Default:** XML-based file storage
- **Optional:** SQL Server, MySQL, SQLite, SQL CE
- **ORM:** Custom data provider pattern

### Security
- **XSS Protection:** Built-in sanitization + AntiXSS library
- **Authentication:** ASP.NET Membership/Forms Authentication
- **File Upload:** MIME validation and security checks

## Security

### Critical Security Steps

1. **Update Machine Key** (Required)

   After installation, generate and update the `machineKey` in `Web.config`:
   - Use a secure generator: [ASP.NET MachineKey Generator](https://www.allkeysgenerator.com/Random/ASP-Net-MachineKey-Generator.aspx)
   - Prevents known exploits (reported September 2019)
   - Especially critical if using default `admin` account

2. **Change Default Credentials** (Highly Recommended)
   - Change the default admin password immediately
   - Use strong, unique passwords

3. **Review Security Documentation**
   - [Comment XSS Verification Guide](BlogEngine/BlogEngine.NET/COMMENT_XSS_VERIFICATION_GUIDE.md)
   - [XSS Implementation Summary](BlogEngine/BlogEngine.NET/XSS_IMPLEMENTATION_SUMMARY.md)

## Database Support

BlogEngine.NET supports multiple storage backends:

- **XML** (Default) - File-based storage in `App_Data` folder
- **SQL Server** - See `setup/SQLServer/` for scripts
- **MySQL** - See `setup/MySQL/` for scripts  
- **SQLite** - See `setup/SQLite/` for configuration
- **SQL CE** - See `setup/SQL_CE/` for configuration

Configuration is managed through `Web.config` connection strings.

## Contributing

Contributions are welcome! Please ensure:
- Code follows existing conventions
- All tests pass
- New features include appropriate tests
- XML documentation for public APIs

## Copyright and License

- **Code:** Released under the [MS-RL License](https://opensource.org/licenses/MS-RL)
- **Documentation:** Released under Creative Commons
- **Copyright:** 2007–2023 BlogEngine

---

## Additional Resources

- **[Copilot Instructions](BlogEngine/copilot-instructions.md)** - Development guidelines and patterns
- **[Skills Documentation](BlogEngine/BlogEngine.NET/skills.md)** - Component skills and capabilities
- **[XSS Test Cases](BlogEngine/BlogEngine.NET/XSS_TEST_CASES.md)** - Security test scenarios
