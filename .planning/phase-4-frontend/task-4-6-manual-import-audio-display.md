# Task 4.6: Manual import preview shows audio formats + score

Status: TODO
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
- [ ] `ManualImportResource` carries `audioFormats` + `audioFormatScore`; populated on both the
      parse path and the existing-episode-file path (spot-check via a Core test or manual run).
- [ ] Interactive import row shows an audio formats/score badge wherever it shows the CF badge,
      hidden when the profile/file has no audio format matches.
- [ ] No behavior change when no audio formats exist (fields empty/0, badge hidden).
- [ ] Backend build + `~EpisodeImport` Core tests pass; `tsc --noEmit` clean.

## Result
_(fill on completion)_
