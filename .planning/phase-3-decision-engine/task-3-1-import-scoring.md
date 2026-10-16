# Task 3.1: LocalEpisode.AudioFormats + import-time scoring hook

Status: TODO
Phase: 3
Depends: 1.6, 2.1
Verify: `dotnet test src/NzbDrone.Core.Test -c Debug --filter "FullyQualifiedName~EpisodeImport"`

## Goal
Compute audio formats on `LocalEpisode` during import decisions, exactly parallel to how custom
formats are computed.

## Context
- `src/NzbDrone.Core/Parser/Model/LocalEpisode.cs` has `List<CustomFormat> CustomFormats` +
  `int CustomFormatScore`. Add `List<AudioFormat> AudioFormats` (+ `int AudioFormatScore` if the
  CF pattern uses a cached score — check how `CustomFormatScore` is set/used first).
- `src/NzbDrone.Core/MediaFiles/EpisodeImport/ImportDecisionMaker.cs` ~line 153 calls
  `_formatCalculator.UpdateEpisodeCustomFormats(localEpisode)`; add
  `_audioFormatCalculator.UpdateEpisodeAudioFormats(localEpisode)` right after (inject
  `AudioFormatCalculationService`).
- `ManualImportService.cs` calls `UpdateEpisodeCustomFormats` at ~lines 186, 457, 553 — add the
  audio call at each site for parity (manual import must score audio too).
- The calculator's `UpdateEpisodeAudioFormats` (add to 1.6's service if not present) needs the
  **profile** to compute score; CF's `UpdateEpisodeCustomFormats` computes formats only and
  scores later — follow the same split (formats on LocalEpisode; score computed where CF score
  is computed, grep `CustomFormatScore` assignments).

## Files
- Modify: `src/NzbDrone.Core/Parser/Model/LocalEpisode.cs`
- Modify: `src/NzbDrone.Core/MediaFiles/EpisodeImport/ImportDecisionMaker.cs`
- Modify: `src/NzbDrone.Core/MediaFiles/ManualImportService.cs`
- Modify: `src/NzbDrone.Core/AudioFormats/AudioFormatCalculationService.cs` (if update-method
  missing)

## Acceptance Criteria
- [ ] After `ImportDecisionMaker.GetDecision`, `localEpisode.AudioFormats` is populated when
      MediaInfo exists; empty when MediaInfo null.
- [ ] Existing import tests still pass (no behavior change yet — specs don't read it until 3.2).

## Result
_(fill on completion)_
