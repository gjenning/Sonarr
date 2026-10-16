# Task 2.1: QualityProfile audio fields + ProfileAudioFormatItem

Status: DONE
Phase: 2
Depends: 1.3
Verify: `dotnet build src/NzbDrone.Core/Sonarr.Core.csproj -c Debug -p:SolutionDir=$PWD/src/`

## Goal
Extend `QualityProfile` with audio scoring fields and the embedded score-item list.

## Context
`src/NzbDrone.Core/Profiles/Qualities/QualityProfile.cs` currently has `FormatItems`
(`List<ProfileFormatItem>`) + `MinFormatScore`/`CutoffFormatScore`/`MinUpgradeFormatScore`.
Add:
- `List<ProfileAudioFormatItem> AudioFormatItems`
- `int MinAudioFormatScore`
- `int CutoffAudioFormatScore`
- `bool UpgradeForAudio` (default **false** — feature off per profile until enabled)
- `bool AudioUpgradeRequiresTitleHint` (default **true**)

`ProfileAudioFormatItem` mirrors `src/NzbDrone.Core/Profiles/ProfileFormatItem.cs`
(`IEmbeddedDocument`, `AudioFormat Format` + `int Score`).

Also check `QualityProfileService` (and wherever new profiles get defaults, e.g.
`QualityProfileFactory` if it exists) so a newly created profile gets an empty
`AudioFormatItems` and sane defaults — mirror how `FormatItems` is handled there. Note:
`src/NzbDrone.Core/Profiles/Qualities/QualityProfileService.cs` has
`CleanupQualityProfileFormatItems` logic for deleted CFs; mirror it for audio formats (remove
items whose format no longer exists) — small addition, include it here.

## Files
- Create: `src/NzbDrone.Core/Profiles/ProfileAudioFormatItem.cs`
- Modify: `src/NzbDrone.Core/Profiles/Qualities/QualityProfile.cs`
- Modify: `src/NzbDrone.Core/Profiles/Qualities/QualityProfileService.cs`
- Test: extend `src/NzbDrone.Core.Test/Profiles/Qualities/QualityProfileServiceFixture.cs`

## Acceptance Criteria
- [ ] Fields exist with defaults; ORM embeds the list (TableMapping: `ProfileFormatItem` is an
      `IEmbeddedDocument` serialized with the profile — verify no extra mapping needed).
- [ ] Cleanup logic drops orphaned audio format items; test added.

## Result
_(fill on completion)_
