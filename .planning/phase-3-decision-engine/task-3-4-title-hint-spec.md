# Task 3.4: Title-hint pre-filter spec

Status: TODO
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
- [ ] With hint required: keywordless same-quality releases rejected; keyword releases allowed.
- [ ] With hint off: 3.3 behavior unchanged.
- [ ] All decision-engine tests green.

## Result
_(fill on completion)_
