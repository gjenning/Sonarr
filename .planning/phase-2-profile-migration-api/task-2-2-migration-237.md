# Task 2.2: Migration 237 — AudioFormats table + profile columns

Status: TODO
Phase: 2
Depends: 2.1
Verify: `dotnet test src/NzbDrone.Core.Test -c Debug --filter "FullyQualifiedName~Migration"`

## Goal
DB migration `237_add_custom_audio_formats.cs`: create the `AudioFormats` table and add the five
audio columns to `QualityProfiles`.

## Context
- Migration folder: `src/NzbDrone.Core/Datastore/Migration/`; next number is **237** (236 exists).
- Table creation template: `171_add_custom_formats.cs` (columns: `Id INTEGER PK`, `Name TEXT`,
  `Specifications TEXT` — check what CF table actually has via the migration; do NOT add
  `IncludeCustomFormatWhenRenaming`).
- Column-add template: `212_add_minium_upgrade_format_score_to_quality_profiles.cs`.
  Columns on `QualityProfiles`: `AudioFormatItems TEXT`, `MinAudioFormatScore INTEGER NOT NULL
  DEFAULT 0`, `CutoffAudioFormatScore INTEGER NOT NULL DEFAULT 0`, `UpgradeForAudio BOOLEAN NOT
  NULL DEFAULT 0`, `AudioUpgradeRequiresTitleHint BOOLEAN NOT NULL DEFAULT 1`.
- Migration test pattern: `src/NzbDrone.Core.Test/Datastore/Migration/` (e.g.
  `225_mediainfo_multiple_streamsFixture.cs`) — in-memory DB, run migration, assert schema.

## Files
- Create: `src/NzbDrone.Core/Datastore/Migration/237_add_custom_audio_formats.cs`
- Create: `src/NzbDrone.Core.Test/Datastore/Migration/237_add_custom_audio_formatsFixture.cs`

## Steps
1. Write migration (both halves in one class: `Up()` creates table + adds columns).
2. Test: migration applies on a pre-237 schema; defaults land (title-hint = true); idempotent
   re-run not required (NzbDrone migration runner handles versioning).
3. Run the full Migration test filter to ensure ordering doesn't break later migrations (none
   exist after 237 yet).

## Acceptance Criteria
- [ ] Fresh migration chain run succeeds (existing `MigrationTest` fixture passes).
- [ ] New test passes.

## Result
_(fill on completion)_
