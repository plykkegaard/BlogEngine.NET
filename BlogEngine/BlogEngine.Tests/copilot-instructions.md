# BlogEngine.Tests project instructions

This project is the test project for BlogEngine.NET and targets .NET Framework 4.8.

- Preserve the existing MSTest-based structure and use `Microsoft.VisualStudio.QualityTools.UnitTestFramework` patterns already used in the project.
- Add or update tests in the existing `WebApi` and `Fakes` conventions rather than introducing a new test framework or test infrastructure.
- Prefer the existing fake repositories under `Fakes` when testing controllers or services; avoid creating parallel test helpers unless necessary.
- Keep test naming and organization consistent with the current `*ControllerTests.cs` style.
- When changing production code, add or update focused tests that verify behavior and compatibility with the current solution rather than implementation details.
