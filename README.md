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
- **BlogEngine.Core** - Core business logic and data access layer (.NET Framework 4.8)
- **BlogEngine.Tests** - Unit tests (xUnit framework)

## Documentation

- **[Settings.xml Configuration Reference](docs/SETTINGS-XML-REFERENCE.md)** — Complete guide to configuring SEO and Generative Engine Optimization (GEO) settings

## Getting Started

### Requirements

- Windows Hosting that supports ASP.NET 4.5 and above
- .NET Framework 4.8 (for development)

### Installation Steps

1. **Download** - Get the latest BlogEngine and extract to the root of your website
2. **Write Permissions** - Add write permissions to the App_Data and Custom folders on your server
3. **Navigate to Admin** - Add /admin/ to your website's URL (e.g., `https://yourwebsite.com/admin/`)
   - Default Username: `admin`
   - Default Password: `admin`

## Development

### Environment Setup

- Visual Studio 2015 or later
- .NET Framework 4.8
- ASP.NET 4.5+

### Build and Run

1. Clone the repository
2. Open `BlogEngine.sln` in Visual Studio
3. Build the solution
4. Run the solution (local development server will start)
5. Navigate to `http://localhost:64079/admin/`
   - Username: `admin`
   - Password: `admin`

### Running Tests

Tests are located in the `BlogEngine.Tests` project and use xUnit for test execution.

## Technology Stack

- **Framework**: ASP.NET with Razor Pages and MVC
- **Language**: C# (.NET Framework 4.8)
- **Frontend**: HTML5, CSS3, JavaScript (AngularJS, jQuery)
- **Database**: Supports XML-based storage, SQL Server, MySQL, SQLite, SQL CE
- **API**: RESTful Web API

## Security

After installation, update the `machineKey` in `Web.config` with values generated using a [tool like this](https://www.allkeysgenerator.com/Random/ASP-Net-MachineKey-Generator.aspx). This prevents known exploits (reported Sep 2019) and is especially important if using the default `admin` account.

## Copyright and License

- **Code**: Released under the MS-RL License
- **Documentation**: Released under Creative Commons
- **Copyright**: 2007–2023 BlogEngine
