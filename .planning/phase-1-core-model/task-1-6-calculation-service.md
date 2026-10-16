# Task 1.6: AudioFormatCalculationService

Status: DONE
Phase: 1
Depends: 1.3, 1.5
Verify: `dotnet test src/NzbDrone.Core.Test -c Debug --filter "FullyQualifiedName~AudioFormats"`

## Goal
Service that evaluates all stored `AudioFormat`s against a `MediaInfoModel` and returns the
matched formats + total score for a profile — the audio analogue of
`CustomFormatCalculationService`.

## Context
Mirror `src/NzbDrone.Core/CustomFormats/CustomFormatCalculationService.cs`:
- `ParseAudioFormats(MediaInfoModel, string sceneName)` → `List<AudioFormat>` (formats whose
  spec group matches: **at least one stream satisfies all specs**, negate per-stream — see 1.5).
- Convenience overloads: `ParseAudioFormats(EpisodeFile)` (uses `file.MediaInfo` +
  `file.SceneName`/`OriginalFilePath`) and `ParseAudioFormats(LocalEpisode)` (uses
  `localEpisode.MediaInfo`, `localEpisode.ReleaseTitle`).
- Score helper: `int GetAudioFormatScore(QualityProfile profile, List<AudioFormat> formats)`
  summing `profile.AudioFormatItems` scores (profile type lands in 2.1 — if not done yet, define
  against an interface or defer the score helper to 3.1; note whichever you do).
- **Null MediaInfo → empty list** (audio logic silently off; locked decision).
- Empty spec list on a format → does not match (CF convention — verify in
  `CustomFormat.IsSatisfiedBy`).

## Files
- Create: `src/NzbDrone.Core/AudioFormats/AudioFormatCalculationService.cs`
- Test: `src/NzbDrone.Core.Test/AudioFormats/AudioFormatCalculationServiceFixture.cs`

## Steps
1. Implement service (constructor: `IAudioFormatService`), caching formats like the CF service
   does if it caches.
2. Implement group matching over streams (delegating per-spec per-stream to 1.5's overload).
3. Tests: match/no-match, multi-stream group, null MediaInfo → empty, score summing (once
   profile type exists).

## Acceptance Criteria
- [x] Group semantics correct incl. negate + StreamCount exception.
- [x] Null-safe everywhere; tests pass.

## Result
Created `AudioFormatCalculationService` (`IAudioFormatCalculationService` + impl) mirroring
`CustomFormatCalculationService`:

- `ParseAudioFormats(MediaInfoModel, string sceneName)` → formats whose spec groups match,
  ordered by name. Group semantics (locked decision): specs grouped by type; a group matches
  when **at least one stream satisfies all specs in the group**, using 1.5's per-stream
  `IsSatisfiedBy(input, stream)` overload (negate per-spec per-stream, StreamCount evaluated
  on the stream set).
- Overloads: `ParseAudioFormats(EpisodeFile)` (release title = SceneName → OriginalFilePath
  filename → RelativePath filename, like CF) and `ParseAudioFormats(LocalEpisode)`.
- Null `MediaInfo` → empty list (locked decision).
- Empty specification list → format does not match.
- No caching: the CF service calls `_formatService.All()` per call, so this one does too.

Tests: `AudioFormatCalculationServiceFixture` — 19 tests (null MediaInfo, empty specs,
single-spec match/no-match, same-stream group semantics, cross-stream non-match, negate via
value and via `Negate`, StreamCount set-level exception incl. `=0` with no streams, name
ordering, EpisodeFile/LocalEpisode overloads incl. null MediaInfo). Verify run:
`Passed: 94, Failed: 0` (includes the 75 spec tests).

Deviations / notes:
- **Score helper deferred to 3.1**: `QualityProfile` has no `AudioFormatItems` until 2.1, so
  `GetAudioFormatScore` is not defined here (task allowed deferring; noted as instructed).
- Task said "empty spec list → does not match (CF convention — verify in
  `CustomFormat.IsSatisfiedBy`)". `CustomFormat.IsSatisfiedBy` does not exist; the actual CF
  calculation service *matches* formats with no specs (`All()` over zero groups is true).
  Implemented as the task explicitly requires (no match) — this intentionally differs from CF.
- `LocalEpisode` has no `ReleaseTitle` property; used `GetSceneOrFileName()` (SceneName with
  filename fallback), the same source the CF local-episode path uses.
- Added the per-stream `IsSatisfiedBy(AudioFormatInput, MediaInfoAudioStreamModel)` overload
  to `IAudioFormatSpecification` (it was only on the concrete class after 1.5) so the service
  can group over the interface.
- Constructor takes `Logger` in addition to `IAudioFormatService` to mirror the CF service's
  trace logging of release-title selection.
