# Task 1.6: AudioFormatCalculationService

Status: TODO
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
- [ ] Group semantics correct incl. negate + StreamCount exception.
- [ ] Null-safe everywhere; tests pass.

## Result
_(fill on completion)_
