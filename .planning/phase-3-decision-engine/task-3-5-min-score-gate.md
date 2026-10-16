# Task 3.5: MinAudioFormatScore import gate (optional)

Status: DONE
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
- [x] Below-min rejected, at/above min passes, null MediaInfo passes.
- [x] Import test suite green.

## Result
Done. New import-time min-audio-score gate, mirroring the grab-time CF `MinFormatScore` gate.

- `AudioFormatAllowedByProfileSpecification.cs` (new import spec): accepts immediately when
  `MinAudioFormatScore <= 0` or `localEpisode.MediaInfo == null` (can't judge); otherwise
  computes `qualityProfile.CalculateAudioFormatScore(localEpisode.AudioFormats)` (same
  access pattern as `UpgradeSpecification`; formats are already populated on the local
  episode by `ImportDecisionMaker` before specs run) and rejects with new
  `ImportRejectionReason.AudioFormatMinimumScore` when below min.
- Second file touched: added `AudioFormatMinimumScore` to `ImportRejectionReason` enum
  (in-memory/transient API value only, not persisted — same pattern as task 3.4's
  `DownloadRejectionReason` addition).
- No manual DI registration needed: import specs are auto-registered as
  `IEnumerable<IImportDecisionEngineSpecification>` implementations (no sibling spec has
  explicit registration).
- Tests: `AudioFormatAllowedByProfileSpecificationFixture.cs` — 6 cases: min not set,
  below-min rejected (+ reason assertion), no-matching-formats rejected, at-min accepted,
  above-min accepted, null MediaInfo accepted.

Verify: `dotnet test src/NzbDrone.Core.Test -c Debug -p:SolutionDir="$PWD/src/" --filter
"FullyQualifiedName~AudioFormatAllowed"` → 6/6 passed (SolutionDir flag needed for the
pre-existing stylecop.json resolution issue, same as tasks 3.2/3.4). Broader
`~MediaFiles.EpisodeImport` run: 238 passed, 0 failed, 3 skipped (pre-existing
Windows-only skips).
