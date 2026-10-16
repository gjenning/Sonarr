# Task 3.3: Audio-aware grab cutoff (UpgradableSpecification / UpgradeDiskSpecification)

Status: TODO
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
- [ ] Bypass only when all conditions hold; never on video downgrade.
- [ ] All existing decision-engine fixtures still green.

## Result
_(fill on completion)_
