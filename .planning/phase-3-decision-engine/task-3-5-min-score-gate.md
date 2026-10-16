# Task 3.5: MinAudioFormatScore import gate (optional)

Status: TODO
Phase: 3
Depends: 3.1
Verify: `dotnet test src/NzbDrone.Core.Test -c Debug --filter "FullyQualifiedName~AudioFormatAllowed"`

## Goal
Mirror `MinFormatScore` (CF) at import: reject downloads whose audio score is below
`profile.MinAudioFormatScore`, so users can hard-block e.g. stereo-only releases.

**Optional task** — skip if 3.2/3.3 surface blockers; the core feature works without it.

## Context
Pattern: `src/NzbDrone.Core/DecisionEngine/Specifications/CustomFormatAllowedByProfileSpecification.cs`
(grab-time CF min-score gate). For audio, the score is only known post-download, so the gate
belongs at **import**: new spec
`src/NzbDrone.Core/MediaFiles/EpisodeImport/Specifications/AudioFormatAllowedByProfileSpecification.cs`
(mirror `UpgradeSpecification.cs`'s access to profile + `localEpisode.AudioFormats`; register in
the import decision pipeline the same way sibling import specs are registered in
`ImportDecisionMaker`).

Rule: reject when `profile.MinAudioFormatScore > 0 && localEpisode.MediaInfo != null &&
audioScore < profile.MinAudioFormatScore`. Null MediaInfo → pass (can't judge).

Fixture: new file under `src/NzbDrone.Core.Test/MediaFiles/EpisodeImport/Specifications/`.

## Files
- Create: import spec + fixture

## Acceptance Criteria
- [ ] Below-min rejected, at/above min passes, null MediaInfo passes.
- [ ] Import test suite green.

## Result
_(fill on completion)_
