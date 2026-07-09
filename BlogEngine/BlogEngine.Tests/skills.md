# BlogEngine.Tests skills

## Primary skills
- MSTest-based unit testing with the existing `Microsoft.VisualStudio.QualityTools.UnitTestFramework` setup.
- Testing Web API controllers and related behavior under `WebApi` using the current fake repository pattern.
- Maintaining coverage for provider-facing logic and controller interactions without introducing a new test framework.

## Implementation guidance
- Prefer the existing fake repositories in `Fakes` when new controller tests are needed.
- Keep test names and organization consistent with the current `*ControllerTests.cs` pattern.
- Add focused tests for behavior changes and regression scenarios instead of broad refactoring.
- **Lazy ponytail approach**: Write only tests that verify actual requirements, reuse existing fakes, avoid overly complex test setups.
