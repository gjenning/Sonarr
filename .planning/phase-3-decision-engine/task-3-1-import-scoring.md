# Task 3.1: LocalEpisode.AudioFormats + import-time scoring hook

Status: DONE
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
- [x] After `ImportDecisionMaker.GetDecision`, `localEpisode.AudioFormats` is populated when
      MediaInfo exists; empty when MediaInfo null.
- [x] Existing import tests still pass (no behavior change yet — specs don't read it until 3.2).

## Result
`LocalEpisode` now carries `List<AudioFormat> AudioFormats` + `int AudioFormatScore`, populated
at import-decision time exactly parallel to custom formats:

- `LocalEpisode.cs`: added `AudioFormats` (defaults to empty list) + `AudioFormatScore` right
  after the CF properties.
- `AudioFormatCalculationService`: added `UpdateEpisodeAudioFormats(LocalEpisode)` to the
  interface + impl — sets `AudioFormats` via the existing `ParseAudioFormats(LocalEpisode)`
  overload and computes `AudioFormatScore` from `Series?.QualityProfile?.Value` (null-safe:
  no series/profile/MediaInfo → empty formats + score 0).
- `QualityProfile`: added `CalculateAudioFormatScore(List<AudioFormat>)` mirroring
  `CalculateCustomFormatScore` (sums scores of `AudioFormatItems` whose format matched).
- `ImportDecisionMaker`: injected `IAudioFormatCalculationService`; calls
  `UpdateEpisodeAudioFormats` immediately after `UpdateEpisodeCustomFormats` in
  `GetDecision` (~line 155).
- `ManualImportService` (`MediaFiles/EpisodeImport/Manual/ManualImportService.cs`): injected
  `IAudioFormatCalculationService`; added the audio call after both
  `UpdateEpisodeCustomFormats` sites (`ReprocessItem` ~188, `Execute` ~556).
- Tests: 2 new tests in `AudioFormatCalculationServiceFixture` (formats + score written to
  LocalEpisode from profile; empty formats/score without MediaInfo).

Verify: `dotnet test src/NzbDrone.Core.Test -c Debug -p:SolutionDir=$PWD/src/ --filter
"FullyQualifiedName~EpisodeImport"` → Passed 224, Failed 0, Skipped 3. AudioFormats filter →
Passed 96, Failed 0.

Deviations / notes:
- Task file listed `MediaFiles/ManualImportService.cs`; actual path is
  `MediaFiles/EpisodeImport/Manual/ManualImportService.cs`.
- Task's third `UpdateEpisodeCustomFormats` site (~line 457) is actually a
  `ParseEpisodeCustomFormats` call that fills `ManualImportItem.CustomFormats` for UI display,
  not a LocalEpisode update — skipped. Surfacing audio formats in manual-import UI responses
  needs API/frontend changes (phase 4).
- Task text suggested "formats on LocalEpisode; score computed later where CF score is
  computed". The actual CF pattern computes the score *inside* the update method from
  `Series.QualityProfile?.Value`, so `UpdateEpisodeAudioFormats` does the same (required
  adding `CalculateAudioFormatScore` to `QualityProfile` — file not listed in the task's
  Files section).
- `dotnet test`/`build` needs `-p:SolutionDir=$PWD/src/` in this environment or StyleCop
  SA1200 errors appear in `NzbDrone.Test.Common`.

