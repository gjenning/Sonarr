# Task 3.2: Audio-aware import UpgradeSpecification

Status: TODO
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
- [ ] All new cases + regressions pass.
- [ ] Rejection reasons use the existing `Reason.Rejected` pattern with a clear message.

## Result
_(fill on completion)_
