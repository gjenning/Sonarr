# Phase 2 — Profile Fields, Migration 237, REST API

**Goal:** quality profiles can carry audio format scores/cutoffs and the two toggles; the
`AudioFormats` table exists in the DB; full CRUD + schema REST API exists so the frontend can
manage audio formats. Depends on Phase 1 (entity + specs must exist).

| Task | Title |
|------|-------|
| 2.1 | `QualityProfile` audio fields + `ProfileAudioFormatItem` |
| 2.2 | Migration 237 (AudioFormats table + profile columns) |
| 2.3 | `AudioFormatController` (CRUD + `/schema`) + resource |
| 2.4 | `QualityProfileResource` + validator updates (V3 + V5) |

Verify at phase end:
```bash
dotnet build src/Sonarr.sln -c Debug
dotnet test src/NzbDrone.Core.Test -c Debug --filter "FullyQualifiedName~Migration|FullyQualifiedName~QualityProfile"
```
