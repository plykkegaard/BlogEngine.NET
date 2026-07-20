# BlogEngine.Tests project instructions

This project is the test project for BlogEngine.NET and targets .NET Framework 4.8.

## Namespace & Directory Structure

```
BlogEngine.Tests (root namespace)

Directory Layout:
├── Fakes/                           → BlogEngine.Tests.Fakes
│   ├── FakeBlogRepository.cs
│   ├── FakeCategoryRepository.cs
│   ├── FakeCommentRepository.cs
│   ├── FakeCustomFieldsRepository.cs
│   ├── FakeLookupsRepository.cs
│   ├── FakePackagesRepository.cs
│   ├── FakePageRepository.cs
│   ├── FakePostRepository.cs
│   ├── FakeRolesRepository.cs
│   ├── FakeStatsRepository.cs
│   ├── FakeTagsRepository.cs
│   ├── FakeTrashRepository.cs
│   └── FakeUsersRepository.cs
├── WebApi/                          → BlogEngine.Tests.WebApi (API controller tests)
│   ├── *ControllerTests.cs          → MSTest classes for API endpoints
│   └── Test patterns: BlogControllerTests, PostControllerTests, etc.
├── Security/                        → BlogEngine.Tests.Security
│   ├── FileUploadValidatorTests.cs
│   └── SecurityHeadersModuleTests.cs
└── Properties/                      → Assembly info
    └── AssemblyInfo.cs
```

## Test Organization

- **Fake Repositories** — In-memory implementations for testing without database
- **WebApi Tests** — MSTest classes following `*ControllerTests` naming convention
- **Security Tests** — Validation and security module tests
- **Test Framework** — Microsoft.VisualStudio.QualityTools.UnitTestFramework (MSTest)

- Preserve the existing MSTest-based structure and use `Microsoft.VisualStudio.QualityTools.UnitTestFramework` patterns already used in the project.
- Add or update tests in the existing `WebApi` and `Fakes` conventions rather than introducing a new test framework or test infrastructure.
- Prefer the existing fake repositories under `Fakes` when testing controllers or services; avoid creating parallel test helpers unless necessary.
- Keep test naming and organization consistent with the current `*ControllerTests.cs` style.
- When changing production code, add or update focused tests that verify behavior and compatibility with the current solution rather than implementation details.
