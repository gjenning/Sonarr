# Task 1.1: MediaInfo audio disposition fields

Status: DONE
Phase: 1
Depends: -
Verify: `dotnet test src/NzbDrone.Core.Test -c Debug --filter "FullyQualifiedName~VideoFileInfoReader" -p:SolutionDir=$PWD/src/`

## Goal
Capture ffprobe audio-stream `disposition` flags (`default`, `forced`, `visual_impaired`) on
`MediaInfoAudioStreamModel` so audio-description (AD) tracks can be detected, and force existing
files to re-analyze.

## Context
AD detection is a required spec field (Phase 1.5). ffprobe returns a `Disposition` dictionary per
stream; `VideoFileInfoReader` already reads it for subtitles (`Default`, `Forced`,
`HearingImpaired` — see the subtitle mapping around lines 137–145 of the file). Audio streams
expose the same dictionary; AD tracks are typically flagged `visual_impaired=1`.

## Files
- Modify: `src/NzbDrone.Core/MediaFiles/MediaInfo/MediaInfoAudioStreamModel.cs`
  (currently has Language, Format, CodecId, Profile, Bitrate, Channels, ChannelPositions, Title)
- Modify: `src/NzbDrone.Core/MediaFiles/MediaInfo/VideoFileInfoReader.cs`
  (audio stream mapping ~lines 89–115; subtitle disposition precedent ~137–145;
  `CURRENT_MEDIA_INFO_SCHEMA_REVISION` const near line 151)
- Test: `src/NzbDrone.Core.Test/MediaFiles/MediaInfo/VideoFileInfoReaderFixture.cs` (extend;
  check for an existing audio-parsing test to copy the fixture style from)

## Steps
1. Add `public bool Default { get; set; }`, `Forced`, `VisualImpaired` to
   `MediaInfoAudioStreamModel`.
2. In the audio mapping block of `VideoFileInfoReader.GetMediaInfo`, read
   `stream.Disposition` (null-guard) and map `default`, `forced`, `visual_impaired` → the new
   properties, mirroring the subtitle block.
3. Bump `CURRENT_MEDIA_INFO_SCHEMA_REVISION` **and `MINIMUM_MEDIA_INFO_SCHEMA_REVISION`** by 1
   (forces re-analysis of stored MediaInfo: `UpdateMediaInfoService` filters on
   `SchemaRevision < MINIMUM`, so bumping CURRENT alone would not re-analyze existing files).
4. Add/extend a unit test asserting disposition flags are parsed for an audio stream.

## Acceptance Criteria
- [ ] New fields populated from ffprobe JSON; missing disposition dict → all false, no crash.
- [ ] Schema revision bumped.
- [ ] Existing + new VideoFileInfoReader tests pass.

## Result
Done as specified:
- `MediaInfoAudioStreamModel`: added `Default`, `Forced`, `VisualImpaired` (non-nullable `bool`,
  default false; kept per task spec — note the subtitle model uses `bool?`).
- `VideoFileInfoReader.GetMediaInfo`: audio mapping block now reads `stream.Disposition`
  (null-guarded, mirrors subtitle block) for `default` / `forced` / `visual_impaired`.
- Schema revision bumped 14 → 15.
- New test `get_info_audio_disposition` in `VideoFileInfoReaderFixture` asserts flags on the
  sample file (ffprobe reports `default=1`, `forced=0`, `visual_impaired=0`).

Deviations:
1. **`MINIMUM_MEDIA_INFO_SCHEMA_REVISION` also bumped 14 → 15.** Step 3 said CURRENT only, but
   `UpdateMediaInfoService.Handle` re-analyzes files with `SchemaRevision < MINIMUM`, so a
   CURRENT-only bump would not force re-analysis (the task's stated goal). `UpdateMediaInfoServiceFixture`
   uses the constants symbolically and still passes (13/13).
2. **Verify command needs `-p:SolutionDir=$PWD/src/`.** `src/Directory.Build.props` wires
   `stylecop.json` via `$(SolutionDir)`, which is undefined when building a single project →
   537 spurious SA1200 errors. Verified: 21/21 VideoFileInfoReader tests pass.
3. Environment: .NET SDK 10.0.402 was not installed; installed via `brew install --cask dotnet-sdk`
   (user-approved).

"Missing disposition dict → all false" is covered by the null-guard pattern (same as subtitles);
no ffprobe output lacks the dict, so no separate test.
