# Task 3.4: Title-hint pre-filter spec

Status: DONE
Phase: 3
Depends: 3.3
Verify: `dotnet test src/NzbDrone.Core.Test -c Debug --filter "FullyQualifiedName~AudioUpgradeTitleHint"`

## Goal
When `AudioUpgradeRequiresTitleHint` is on, only allow the audio-bypass grab (3.3) if the release
title contains an audio keyword — don't burn download slots on releases that can't improve audio.

## Context
New grab-decision spec `src/NzbDrone.Core/DecisionEngine/Specifications/AudioUpgradeTitleHintSpecification.cs`
(mirror sibling spec structure; `ISpecification<ReleaseInfo>`-style with `Order` set **after**
`UpgradeDiskSpecification` so it only filters releases that were allowed *because* of the audio
bypass).

Rejection condition (reject = block the grab):
```
profile.UpgradeForAudio
&& profile.AudioUpgradeRequiresTitleHint
&& audioBypass was the reason UpgradeDiskSpecification allowed it
&& release.Title has no audio keyword
```
Simplest robust implementation: recompute the same `audioBypass` predicate (3.3) inside this
spec; if it's true and the title lacks a keyword → `Reason.Rejected` ("release title does not
indicate an audio upgrade"). If recomputing "was the reason" is awkward, acceptable v1: reject
whenever `audioBypass && !titleHint` regardless of which spec allowed — same net effect because
without bypass the release was already rejected by cutoff logic.

Keyword list (constant array, case-insensitive word-boundary regex):
`truehd, atmos, dts-hd, dts hd, dts-x, dts x, dd+, ddpa, dd 5.1, dd5.1, eac3, ac3, pcm, flac,
24bit, 24 bit, 7.1, 5.1` — tune conservatively; document that it's a v1 heuristic and where to
extend. (Future: derive from spec values; out of scope.)

Fixture: `src/NzbDrone.Core.Test/DecisionEngineTests/AudioUpgradeTitleHintSpecificationFixture.cs`
— hint present/absent, toggle off → spec inert, no existing-file → inert, regression: profiles
without audio formats unaffected.

## Files
- Create: `src/NzbDrone.Core/DecisionEngine/Specifications/AudioUpgradeTitleHintSpecification.cs`
- Create: fixture above

## Acceptance Criteria
- [x] With hint required: keywordless same-quality releases rejected; keyword releases allowed.
- [x] With hint off: 3.3 behavior unchanged.
- [x] All decision-engine tests green.

## Result
Done. New grab-decision spec rejects audio-bypass grabs whose release title lacks an
audio keyword.

- `AudioUpgradeTitleHintSpecification.cs`: inert unless `UpgradeForAudio &&
  AudioUpgradeRequiresTitleHint && AudioFormatItems.Any()`. For each existing episode
  file with MediaInfo, it determines whether the audio bypass (3.3) is *the reason* the
  grab is allowed by flipping `UpgradableSpecification.IsUpgradable`: rejected with
  `currentAudioScore = null` but `None` with the file's actual audio score. If any file
  flips and `Release.Title` matches no keyword → `Reject` ("Release title does not
  indicate an audio upgrade"). Missing/blank title → accept (mirrors RawDiskSpecification's
  null-release tolerance).
- Keyword list exactly as specified in the task, compiled into one case-insensitive regex
  built from a constant string array via `Regex.Escape`. Deviation note: lookaround
  boundaries `(?<!\w)…(?!\w)` are used instead of `\b` so keywords ending in a non-word
  char (`dd+`) can match; documented as a v1 heuristic in the code comment with where to
  extend.
- Deviation: the task's "acceptable v1" (reject whenever the raw 3.3 predicate holds)
  would also block genuine video/CF upgrades (they satisfy `qualityCompare >= 0` too),
  so the flip test above was implemented instead — it is the precise "was the reason"
  check the task lists as the preferred option.
- Deviation: this repo's specs have no numeric `Order` (only `SpecificationPriority`
  groups; all Default-priority specs run together), so "Order after UpgradeDiskSpecification"
  is not applicable; the spec recomputes the bypass reason itself instead of relying on
  ordering.
- Third file touched (beyond the task's Files list): added `AudioUpgradeTitleHintMissing`
  to `DownloadRejectionReason` enum (in-memory only, not persisted; frontend shows the
  message string).
- Tests: `AudioUpgradeTitleHintSpecificationFixture.cs` — 16 test cases: reject without
  keyword (+ reason assertion), accept with keyword (6 title variants: DTS-HD/TrueHD
  Atmos/AC3/DDPA/FLAC 24bit/ATMOS), hint off, UpgradeForAudio off, no audio formats,
  no existing file, MediaInfo null, audio cutoff met, video upgrade unaffected, revision
  upgrade unaffected, null Release.

Verify: `dotnet test src/NzbDrone.Core.Test -c Debug -p:SolutionDir="$PWD/src/" --filter
"FullyQualifiedName~AudioUpgradeTitleHint"` → 16/16 passed. Broader `~DecisionEngineTests`
run: 409/409 passed.
