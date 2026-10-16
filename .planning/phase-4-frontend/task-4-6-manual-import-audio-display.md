# Task 4.6: Manual import preview shows audio formats + score

Status: DONE
Phase: 4
Depends: 3.1, 4.1
Verify: `dotnet build src/Sonarr.sln -c Debug && dotnet test src/NzbDrone.Core.Test -c Debug --no-build -p:SolutionDir=$PWD/src/ --filter "FullyQualifiedName~EpisodeImport"` + `npx tsc --noEmit -p frontend/tsconfig.json`

## Goal
Found by task 5.2 QA (checklist item "Manual import UI"): the manual/interactive import preview
shows custom formats + CF score per file, but nothing for audio formats. Task 3.1 computes
`LocalEpisode.AudioFormats`/`AudioFormatScore` at decision time, and its Result flagged that
surfacing them needs API/frontend changes that no phase-4 task covered. Close that gap: the
interactive import row should show audio formats/score wherever it shows the CF score.

## Context
CF display path to mirror, end to end:

- Core item: `src/NzbDrone.Core/MediaFiles/EpisodeImport/Manual/ManualImportItem.cs` has
  `List<CustomFormat> CustomFormats` + `int CustomFormatScore` (lines ~25–26). Add
  `List<AudioFormat> AudioFormats = new()` + `int AudioFormatScore`.
- Core population: `src/NzbDrone.Core/MediaFiles/EpisodeImport/Manual/ManualImportService.cs`
  fills the CF fields at two sites:
  - parse path (~line 462): `item.CustomFormats = _localEpisodeFormatCalculator.ParseEpisodeCustomFormats(decision.LocalEpisode);`
    + score via `item.Series.QualityProfile?.Value.CalculateCustomFormatScore(...)`. For audio,
    `decision.LocalEpisode` already carries `AudioFormats`/`AudioFormatScore` (set by
    `UpdateEpisodeAudioFormats` in `ImportDecisionMaker.GetDecision`, task 3.1) — copy them;
    if a code path reaches here without the hook having run, call
    `_audioFormatCalculationService.UpdateEpisodeAudioFormats(decision.LocalEpisode)` first
    (the service is already injected since 3.1).
  - existing-file path (~line 489): `item.CustomFormats = _formatCalculator.ParseCustomFormat(episodeFile, series);`
    — mirror with `_audioFormatCalculationService.ParseAudioFormats(episodeFile)` +
    `CalculateAudioFormatScore` on the profile.
- API: `src/Sonarr.Api.V3/ManualImport/ManualImportResource.cs` — add
  `List<AudioFormatResource> AudioFormats` + `int AudioFormatScore`; map in
  `ToResource` mirroring the `CustomFormats`/`CustomFormatScore` lines
  (`AudioFormatResource.ToResource(IEnumerable, includeDetails)` already exists).
- Frontend types: `frontend/src/InteractiveImport/InteractiveImport.ts` — add
  `audioFormats?: AudioFormat[]` + `audioFormatScore?: number` (import `AudioFormat` from
  `Settings/AudioFormats/AudioFormats/useAudioFormats` — path as created in 4.1/4.2; the row
  props in `InteractiveImportRow.tsx` already accept `customFormatScore?: number` even though
  the type omits it, so keep both optional and consistent).
- Frontend display: `frontend/src/InteractiveImport/Interactive/InteractiveImportRow.tsx`
  (~line 506) renders the CF score badge via `formatCustomFormatScore(customFormatScore,
  customFormats.length)` with a tooltip listing formats. Add an equivalent audio badge next to
  it, same helper, only when `audioFormats?.length`. Reuse existing localization keys where
  possible (e.g. `Settings/AudioFormats` labels from 4.2/4.3); add `en.json` keys only if a
  new label is unavoidable.

## Files
- `src/NzbDrone.Core/MediaFiles/EpisodeImport/Manual/ManualImportItem.cs`
- `src/NzbDrone.Core/MediaFiles/EpisodeImport/Manual/ManualImportService.cs`
- `src/Sonarr.Api.V3/ManualImport/ManualImportResource.cs`
- `frontend/src/InteractiveImport/InteractiveImport.ts`
- `frontend/src/InteractiveImport/Interactive/InteractiveImportRow.tsx`

## Acceptance Criteria
- [x] `ManualImportResource` carries `audioFormats` + `audioFormatScore`; populated on both the
      parse path and the existing-episode-file path (spot-check via a Core test or manual run).
- [x] Interactive import row shows an audio formats/score badge wherever it shows the CF badge,
      hidden when the profile/file has no audio format matches.
- [x] No behavior change when no audio formats exist (fields empty/0, badge hidden).
- [x] Backend build + `~EpisodeImport` Core tests pass; `tsc --noEmit` clean.

## Result
Done. Mirrored the CF display path end to end:

- `ManualImportItem`: added `AudioFormats` + `AudioFormatScore`.
- `ManualImportService`: parse path (`MapItem(ImportDecision, …)`) populates audio formats via
  `_audioFormatCalculator.ParseAudioFormats(decision.LocalEpisode)` + profile score; existing-file
  path (`MapItem(EpisodeFile, …)`) via `ParseAudioFormats(episodeFile)` + profile score.
- `ManualImportResource`: added `AudioFormats` (`AudioFormatResource.ToResource(false)`) +
  `AudioFormatScore`; `ToResource` recomputes the score from the profile, mirroring the CF lines.
- Frontend: `InteractiveImport` type gained optional `audioFormats`/`audioFormatScore`;
  `InteractiveImportRow` renders an audio score badge (same `formatCustomFormatScore` helper +
  `EpisodeFormats` tooltip) next to the CF badge in the same cell, only when `audioFormats?.length`.
  Reused the existing `CustomAudioFormats` localization key — no `en.json` change.

Deviations from the task file:
1. Parse path **re-parses** audio formats instead of copying `decision.LocalEpisode.AudioFormats`:
   in `ReprocessItem` the `ImportDecisionMaker` hook runs *before* `Augment` (MediaInfo still null →
   copied value would be empty), and some `MapItem` callers never run the hook. Re-parsing mirrors
   exactly what the CF path does (`ParseEpisodeCustomFormats`) and is correct on every path.
2. Injected field is `_audioFormatCalculator` (not `_audioFormatCalculationService` as written).
3. Frontend `AudioFormat` type imported from `typings/AudioFormat` — `useAudioFormats` does not
   re-export it.
4. Extra file: `frontend/src/Episode/EpisodeFormats.tsx` — widened prop type from `CustomFormat[]`
   to `Array<{ id: number; name: string }>` so the audio list can reuse the tooltip component
   (`AudioFormat` lacks `includeCustomFormatWhenRenaming`); all existing callers unaffected.
5. Extra file: `src/NzbDrone.Core.Test/MediaFiles/EpisodeImport/Manual/ManualImportServiceFixture.cs`
   — spot-check test for both population paths (acceptance criterion); runs under the `~EpisodeImport`
   filter (2 new tests).

Verify: `dotnet build src/Sonarr.sln -c Debug` succeeded; `~EpisodeImport` tests 240 passed / 3 skipped;
`npx tsc --noEmit -p frontend/tsconfig.json` clean; eslint clean on changed frontend files.

Commit: PENDING
