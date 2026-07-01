# BlogEngine.Core project instructions

This project is the core class library for BlogEngine.NET and targets .NET Framework 4.8.

- Preserve the existing provider-based architecture such as `BlogProvider`, `XmlProvider`, `DbProvider`, and the repository/service abstractions under `Data`, `Providers`, `Services`, `Web`, and `API`.
- Keep the existing content and configuration models intact, including XML-based storage and the current provider contracts. Avoid changing data contracts unless a change specifically requires it.
- Follow the established namespace layout and naming patterns in the library instead of introducing new abstractions or frameworks.
- Prefer incremental changes that fit the current architecture. Avoid introducing modern .NET abstractions, dependency injection containers, or new package patterns unless they are already used in the project.
- Maintain compatibility with the solution’s legacy assemblies and `packages.config`-based dependency setup.
