# Custom Audio Formats — Overview & Architecture

## Feature summary

Add **Custom Audio Formats**: a scoring system parallel to Custom Formats, but matched against
the file's **MediaInfo audio streams** (only known after download). Quality profiles gain audio
format scores + cutoffs, and can upgrade an existing episode file for better audio **even when
the video quality cutoff and custom-format cutoff are already met**. The audio check runs after
download but **before the move to the final destination**; a win flows into the existing replace
path (`ImportApprovedEpisodes` → `UpgradeMediaFileService.UpgradeEpisodeFile`, old file recycled).

## Locked design decisions

1. **Video quality rules unchanged.** Audio upgrade only when video quality is **equal**
   (and CF score equal or cutoff met). No video downgrades for audio.
2. **Grab-time gate is per-profile.** `UpgradeForAudio` enables past-cutoff grabs when the
   existing file's audio score < `CutoffAudioFormatScore`. `AudioUpgradeRequiresTitleHint`
   (default **on**) additionally requires an audio keyword (TrueHD, ATMOS, DTS-HD…) in the
   release title, to avoid burning download slots on hopeless releases.
3. **Generic spec design.** One `AudioStreamSpecification` with a `Field` selector:
   `Codec, Channels, Bitrate, Title, Language, AudioDescription, StreamCount`. Language uses
   the Sonarr language selector (include/exclude via negate). AudioDescription = include/exclude
   audio-descriptive tracks (ffprobe disposition + title regex fallback).
4. **Matching semantics.** An audio format matches when **at least one audio stream satisfies
   all specs in the group** (negate applies per-stream). So `[Codec~truehd, Lang=eng, NOT AD]`
   matches iff some stream is non-AD English TrueHD.
5. **Loop control.** Import rejects non-upgrades; `HistorySpecification` prevents re-grabbing
   the same release. Title-hint default keeps cycling conservative.

## Key pipeline facts (verified in this repo)

- Grab-time cutoff logic: `src/NzbDrone.Core/DecisionEngine/Specifications/UpgradableSpecification.cs`
  (`IsUpgradable`, `CutoffNotMet`), called by `UpgradeDiskSpecification.cs` / `UpgradeAllowedSpecification.cs`.
- Import-time upgrade gate: `src/NzbDrone.Core/MediaFiles/EpisodeImport/Specifications/UpgradeSpecification.cs`.
- Import scoring hook: `ImportDecisionMaker.cs` ~line 153 (`UpdateEpisodeCustomFormats`);
  `localEpisode.MediaInfo` is populated by `AggregationService.Augment` for new downloads.
- `EpisodeFile.MediaInfo` is stored → existing-file audio score is computable at grab time.
  If `MediaInfo` is null (analysis off), audio logic is skipped entirely (no behavior change).
- Audio codec strings via `MediaInfoFormatter.FormatAudioCodec(stream, sceneName)` →
  "TrueHD Atmos", "EAC3 Atmos", "DTS-HD MA", "AC3", …; channels via `FormatAudioChannels`.
- ffprobe `disposition` is already read for subtitles (`VideoFileInfoReader.cs` ~137–145);
  audio disposition (`visual_impaired`, `default`, `forced`) follows the same pattern.
- CF pattern to mirror: `src/NzbDrone.Core/CustomFormats/` (entity, service, specs,
  `CustomFormatCalculationService`), API `src/Sonarr.Api.V3/CustomFormats/CustomFormatController.cs`
  (has `GET schema`), frontend `frontend/src/Settings/CustomFormats/`.
- Next DB migration number: **237** (latest is 236).
- Profile fields today: `UpgradeAllowed, Cutoff, MinFormatScore, CutoffFormatScore,
  MinUpgradeFormatScore, FormatItems` (`Profiles/Qualities/QualityProfile.cs`).

## Phase map

| Phase | Scope | Tasks | Depends on |
|-------|-------|-------|-----------|
| 1 | Core model & scoring (Core lib, no API/UI) | 1.1–1.6 | — |
| 2 | Profile fields, migration 237, REST API | 2.1–2.4 | 1 |
| 3 | Decision engine (grab + import gates) | 3.1–3.5 | 1, 2 |
| 4 | Frontend (settings page + profile editor) | 4.1–4.5 | 2 (4.4 needs 2.4) |
| 5 | Integration tests + QA + final gate | 5.1–5.3 | 1–4 |

Within a phase, follow task order; `Depends:` lines are authoritative.

## Global notes / risks

- **Re-grab loop**: past-cutoff grabs that fail the audio check at import can be retried as
  *different* releases of the same quality. Mitigated by title-hint default; revisit with a
  per-episode attempt counter if it bites in practice (follow-up, not in scope).
- **MediaInfo availability**: bump `CURRENT_MEDIA_INFO_SCHEMA_REVISION` in `VideoFileInfoReader.cs`
  when adding disposition fields so existing files re-analyze via `UpdateMediaInfoService`.
- **Default behavior must not change**: profiles with no audio formats / `UpgradeForAudio=false`
  must behave exactly as today — every phase has a regression test for this.
- Repo layout differs from upstream Sonarr: `DecisionEngine/` (not `Decision/`),
  `CustomFormats/` (not `Profiles/CustomFormat/`), plus newer `MinUpgradeFormatScore` + V5 API.
