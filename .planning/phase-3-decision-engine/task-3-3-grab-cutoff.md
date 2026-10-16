# Task 3.3: Audio-aware grab cutoff (UpgradableSpecification / UpgradeDiskSpecification)

Status: DONE
Phase: 3
Depends: 2.1, 1.6
Verify: `dotnet test src/NzbDrone.Core.Test -c Debug --filter "FullyQualifiedName~UpgradeDiskSpecification|FullyQualifiedName~UpgradableSpecification"`

## Goal
At grab time, treat "cutoff met" as **not met** when audio upgrades are still pending, so
same-quality releases keep being grabbed until audio cutoff is reached.

## Context
`src/NzbDrone.Core/DecisionEngine/Specifications/UpgradableSpecification.cs` — `IsUpgradable`
returns None when quality cutoff met + CF score ≥ cutoff. Bypass rule (grab time; new release's
audio is unknown, so we only know the **existing** file's audio):

```
audioBypass =
    profile.UpgradeForAudio
    && profile.AudioFormatItems.Any()
    && existingFile?.MediaInfo != null
    && existingAudioScore < profile.CutoffAudioFormatScore
    && qualityCompare >= 0   // never bypass a video downgrade
```

When `audioBypass`, `IsUpgradable` returns None (allow) even if quality/CF cutoffs are met.
Plumbing: `UpgradeDiskSpecification.cs` (and `UpgradeAllowedSpecification.cs` if it exists)
already computes existing CF score from `episodeFile` — compute existing audio score the same
way (inject `AudioFormatCalculationService`) and pass it into `IsUpgradable` (extend signature
with the audio score + profile pieces; keep old overloads compiling or update all callers —
grep callers of `IsUpgradable` first: grab decision engine + possibly manual import).

Fixtures: `src/NzbDrone.Core.Test/DecisionEngineTests/UpgradeDiskSpecificationFixture.cs` and
`UpgradableSpecificationFixture.cs` (grep exact names). Add: bypass allows same-quality grab when
audio below cutoff; no bypass when UpgradeForAudio false / audio at cutoff / MediaInfo null /
quality would downgrade; regression suite unchanged.

## Files
- Modify: `src/NzbDrone.Core/DecisionEngine/Specifications/UpgradableSpecification.cs`
- Modify: `src/NzbDrone.Core/DecisionEngine/Specifications/UpgradeDiskSpecification.cs`
- Modify: fixtures above

## Acceptance Criteria
- [x] Bypass only when all conditions hold; never on video downgrade.
- [x] All existing decision-engine fixtures still green.

## Result
Done. Grab-time cutoff logic is now audio-aware:

- `UpgradableSpecification.cs`: `IsUpgradable` and `CutoffNotMet` gained an optional
  `int? currentAudioScore = null` parameter (null = audio unknown → no bypass, so existing
  callers `QueueSpecification` / `HistorySpecification` compile and behave unchanged).
  New private helper `IsAudioUpgradePending` implements the bypass rule:
  `UpgradeForAudio && AudioFormatItems.Any() && currentAudioScore.HasValue && newQuality != null
  && qualityCompare >= 0 && currentAudioScore < CutoffAudioFormatScore`.
  In `IsUpgradable` the bypass sits after the BetterQuality/BetterRevision guards and before the
  QualityCutoff rejection, so it can bypass QualityCutoff/CustomFormatScore/CustomFormatCutoff/
  MinCustomFormatScore but never a video downgrade or revision downgrade. `CutoffNotMet` also
  consults it (needed because `UpgradeDiskSpecification` rejects with "Existing file meets
  cutoff" before ever calling `IsUpgradable`).
- `UpgradeDiskSpecification.cs`: injected `IAudioFormatCalculationService`; new private
  `GetExistingAudioScore` returns null when `file.MediaInfo == null`, otherwise
  `qualityProfile.CalculateAudioFormatScore(_audioFormatService.ParseAudioFormats(file))`.
  The score is computed once per file and passed into both `CutoffNotMet` and `IsUpgradable`
  in the season-pack and single-episode paths. `UpgradeAllowedSpecification.cs` needed no change.
- Tests: 5 new tests in `UpgradeSpecificationFixture.cs` (the task file said
  `UpgradableSpecificationFixture.cs`, but that fixture doesn't exist — `UpgradeSpecificationFixture`
  is the `UpgradableSpecification` test) and 4 new tests in `UpgradeDiskSpecificationFixture.cs`
  covering: bypass allows same-quality grab when audio below cutoff; no bypass when
  UpgradeForAudio false / audio cutoff already met / MediaInfo null (disk) or audio score
  unknown (unit) / quality downgrade.

Verify: `dotnet test src/NzbDrone.Core.Test -c Debug -p:SolutionDir="$PWD/src/" --filter
"FullyQualifiedName~UpgradeDiskSpecification|FullyQualifiedName~UpgradeSpecification"` →
87/87 passed (the task file's filter `~UpgradableSpecification` matches no fixture; `-p:SolutionDir`
is needed so stylecop.json resolves). Broader `~DecisionEngineTests` run: 393/393 passed.
