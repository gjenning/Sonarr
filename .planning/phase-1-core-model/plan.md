# Phase 1 — Core Model & Scoring (backend, no API/UI)

**Goal:** a working `NzbDrone.Core/AudioFormats/` subsystem: entity, repository, service,
specification system (generic `AudioStreamSpecification`), and a calculation service that can
score a `MediaInfoModel` / `EpisodeFile` / `LocalEpisode` against all defined audio formats.
Also extend MediaInfo with audio disposition flags so AD detection is possible.

Nothing in this phase changes existing behavior — it is all additive code + a MediaInfo schema
bump. Ship gate: solution builds, new unit tests pass, existing MediaInfo tests pass.

| Task | Title |
|------|-------|
| 1.1 | MediaInfo audio disposition fields (AD detection groundwork) |
| 1.2 | `IAudioFormatSpecification` + `AudioFormatInput` |
| 1.3 | `AudioFormat` entity + repository + TableMapping |
| 1.4 | `AudioFormatService` (CRUD) |
| 1.5 | `AudioStreamSpecification` (all fields) |
| 1.6 | `AudioFormatCalculationService` |

Verify at phase end:
```bash
dotnet build src/Sonarr.sln -c Debug
dotnet test src/NzbDrone.Core.Test -c Debug --filter "FullyQualifiedName~AudioFormats|FullyQualifiedName~VideoFileInfoReader"
```
