# Task 3.2: Audio-aware import UpgradeSpecification

Status: DONE
Phase: 3
Depends: 3.1
Verify: `dotnet test src/NzbDrone.Core.Test -c Debug --filter "FullyQualifiedName~UpgradeSpecification"`

## Goal
At import, approve an equal-video-quality release as an upgrade when it improves the audio score
while the existing file is below the audio cutoff.

## Context
`src/NzbDrone.Core/MediaFiles/EpisodeImport/Specifications/UpgradeSpecification.cs` today:
rejects lower quality; on equal quality approves only if new CF score > current (or CF cutoff
not met). Extend the equal-quality branch **before rejecting**:

```
audioUpgradeEligible =
    profile.UpgradeForAudio
    && profile.AudioFormatItems.Any()
    && existingFile.MediaInfo != null
    && localEpisode.MediaInfo != null
    && existingAudioScore < profile.CutoffAudioFormatScore
    && newAudioScore > existingAudioScore
```

- Scores: `AudioFormatCalculationService` — existing score from `existingFile.MediaInfo`
  (re-compute on the fly, same as CF scores are), new score from `localEpisode.AudioFormats`.
- Video quality must be **equal** (locked decision — no video downgrade for audio). The existing
  `qualityCompare < 0` rejection stays untouched.
- CF cutoff-not-met path stays as-is; audio is an additional OR-branch for approval.
- Update fixture `src/NzbDrone.Core.Test/MediaFiles/EpisodeImport/Specifications/UpgradeSpecificationFixture.cs`:
  new cases (audio upgrade approved; rejected when score not higher; rejected when existing
  already at cutoff; rejected when `UpgradeForAudio` false; rejected when MediaInfo null;
  **regression**: no audio formats → identical outcomes to today).

## Files
- Modify: `src/NzbDrone.Core/MediaFiles/EpisodeImport/Specifications/UpgradeSpecification.cs`
- Modify: `src/NzbDrone.Core.Test/MediaFiles/EpisodeImport/Specifications/UpgradeSpecificationFixture.cs`

## Acceptance Criteria
- [x] All new cases + regressions pass.
- [x] Rejection reasons use the existing `Reason.Rejected` pattern with a clear message.

## Result
Implemented in `UpgradeSpecification.cs`:
- Injected `IAudioFormatCalculationService` (constructor; DI auto-wires, no manual constructions exist).
- In the `qualityCompare == 0 && newFormatScore < currentFormatScore` reject branch, a new
  private `IsAudioUpgradeEligible(localEpisode, episodeFile, qualityProfile)` check runs first.
  When eligible (UpgradeForAudio, profile has audio format items, both MediaInfos present,
  existing audio score < cutoff, new audio score > existing), the release is accepted
  (`continue`) with a debug log. Otherwise the existing CF rejection logic runs unchanged,
  so rejection reasons (`NotCustomFormatUpgrade` / `NotCustomFormatUpgradeAfterRename`) are
  preserved — no new `ImportRejectionReason` needed.
- Eligibility gates run cheap checks before calling the audio service, so profiles without
  audio formats never touch it (regression-safe).
- `qualityCompare < 0` rejection untouched (no video downgrade for audio — locked decision).

Tests added to `UpgradeSpecificationFixture.cs` (8 new, plus 2 helpers
`GivenCustomFormatDowngradeScenario` / `GivenAudioProfile`):
- audio upgrade approved; rejected when audio score not higher; rejected when existing meets
  audio cutoff; rejected when `UpgradeForAudio` false (audio service never called);
  rejected when profile has no audio format items; rejected when existing file MediaInfo null;
  rejected when local episode MediaInfo null; regression: video downgrade + better audio is
  still rejected (`NotQualityUpgrade`). All 45 pre-existing tests act as the
  "no audio formats → identical outcomes" regression.

Verify: `dotnet test src/NzbDrone.Core.Test -c Debug --filter "FullyQualifiedName~UpgradeSpecification" -p:SolutionDir=$PWD/src/`
→ Passed! Failed: 0, Passed: 53, Skipped: 0, Total: 53.
