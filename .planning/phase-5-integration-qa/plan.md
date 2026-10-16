# Phase 5 — Integration Tests, QA, Final Gate

**Goal:** prove the feature end-to-end and that nothing regressed. API integration test for the
AudioFormats endpoints, a written manual QA checklist covering the grab→import→replace flows and
loop scenarios, and a final full build + full Core test run.

| Task | Title |
|------|-------|
| 5.1 | API integration test (`NzbDrone.Integration.Test`) |
| 5.2 | Manual QA checklist (documented scenarios) |
| 5.3 | Final gate: full build + full test suite + regression review |

Verify at phase end:
```bash
dotnet build src/Sonarr.sln -c Debug
dotnet test src/NzbDrone.Core.Test -c Debug
```
