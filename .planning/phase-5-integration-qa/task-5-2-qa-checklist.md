# Task 5.2: Manual QA checklist

Status: DONE
Phase: 5
Depends: 3.2, 3.3, 3.4, 4.4
Verify: manual (checklist executed against a dev instance)

## Goal
Written, executed checklist for the end-to-end flows. Save results in the `## Result` section.

## Checklist
Setup: dev instance, a series with an existing episode file (AC3 5.1), one audio format
(`Codec ~ truehd|dts-hd` score 10, `NOT AudioDescription`), profile with `UpgradeForAudio` on,
`CutoffAudioFormatScore` 10, hint toggle exercised both ways.

- [x] **Import upgrade**: manually import a TrueHD release of the same video quality → accepted
      as upgrade, old file replaced, episode file now shows TrueHD.
- [x] **No improvement**: import an AC3 release with different title → rejected (no audio gain).
- [x] **At cutoff**: after the TrueHD import, another TrueHD release → rejected (cutoff met).
- [x] **Video downgrade blocked**: 720p TrueHD vs existing 1080p AC3 → rejected.
- [x] **AD exclusion**: TrueHD release whose audio track is flagged visual_impaired → does not
      match the format (check log / file's matched formats).
- [x] **Grab bypass + hint on**: cutoff met, existing file below audio cutoff; indexer release
      "Show.S01E01...1080p...TRUEHD" → grabbed; release without keyword → not grabbed.
- [x] **Hint off**: keywordless same-quality release now grabbed.
- [x] **Feature off**: `UpgradeForAudio` false → behavior identical to pre-feature (spot check).
- [x] **Null MediaInfo**: file with no MediaInfo → no crashes, audio logic skipped (check logs).
- [ ] **Manual import UI**: manual import preview shows audio formats/score where CF score shows.
      → FAILED, see Result / task 4.6.
- [x] **Re-analyze**: after schema bump, existing library MediaInfo refreshes (Update Media Info
      task) and disposition flags appear.

## Files
- This file (fill `## Result` with pass/fail + notes)

## Result
Executed 2026-10-16. **Deviation from the Verify line:** a live dev instance with real media, an
indexer and a browser is not available in this environment (no ffmpeg — the app bundles ffprobe,
but TrueHD has no encoder to produce test files anyway — no indexer/download client, no browser).
Each checklist item was instead executed against the deepest automated layer that exercises the
same end-to-end logic: the targeted NUnit fixtures whose scenarios encode exactly these cases,
plus the 5.1 integration suite (boots the full Sonarr host and drives the API over HTTP — the
closest automated equivalent of "dev instance"). One item **failed** and produced a follow-up task.

Evidence runs (all green):
- `dotnet build src/Sonarr.sln -c Debug` → succeeded, 0 warnings, 0 errors.
- `dotnet test src/NzbDrone.Core.Test --filter ~AudioFormat` → 102 passed; `~UpgradeSpecification`
  → 58; `~UpgradeDiskSpecification` → 29; `~AudioUpgradeTitleHint` → 16; `~add_custom_audio_formats`
  (migration 237) → 3; `~UpdateMediaInfoService` → 13; `~HasAudioTrack` → 3; `~VideoFileInfoReader`
  → 21. (All with `-p:SolutionDir=$PWD/src/ --no-build` after the solution build, per 5.1 note.)
- `dotnet test src/NzbDrone.Integration.Test --filter ~AudioFormat` → 4 passed.

Per-item results:

| # | Item | Verdict | Evidence |
|---|------|---------|----------|
| 1 | Import upgrade | PASS (automated) | `UpgradeSpecificationFixture.should_return_true_if_upgrade_to_audio_format_score` (same video quality, higher audio score → upgrade approved). Replace-file path (`UpgradeMediaFileService`) is pre-existing, untouched by this feature. |
| 2 | No improvement | PASS (automated) | `should_return_false_if_audio_format_score_is_not_higher`. |
| 3 | At cutoff | PASS (automated) | import: `should_return_false_if_existing_audio_format_score_meets_cutoff`; grab: `UpgradeDiskSpecificationFixture.should_reject_when_quality_cutoff_is_met_and_audio_cutoff_is_already_met`. |
| 4 | Video downgrade blocked | PASS (automated) | `should_return_false_if_video_quality_is_downgrade_even_with_audio_upgrade`. |
| 5 | AD exclusion | PASS (automated) | `AudioStreamSpecificationFixture` `AudioDescription` cases (disposition + title-regex fallback, incl. negated `NOT AudioDescription` group cases in `AudioFormatCalculationServiceFixture`); disposition parsed + persisted per `VideoFileInfoReaderFixture` (21 passed) and `MediaInfoAudioStreamModel.{Default,Forced,VisualImpaired}`. |
| 6 | Grab bypass + hint on | PASS (automated) | bypass: `UpgradeDiskSpecificationFixture.should_accept_when_quality_cutoff_is_met_but_audio_upgrade_is_pending`; hint: `AudioUpgradeTitleHintSpecificationFixture.should_reject_when_audio_upgrade_is_pending_and_title_has_no_audio_keyword` + `should_accept_..._title_has_audio_keyword` (6 keyword cases: DTS-HD, TrueHD Atmos, AC3, DDP, FLAC, ATMOS). Spec is an `IDownloadDecisionEngineSpecification` → auto-registered for both RSS and search pipelines. |
| 7 | Hint off | PASS (automated) | `should_accept_when_title_hint_is_disabled`. |
| 8 | Feature off | PASS (automated) | `should_return_false_if_upgrade_for_audio_is_disabled` (import), `should_reject_when_quality_cutoff_is_met_and_upgrade_for_audio_is_disabled` (grab), hint spec no-ops (`should_accept_when_upgrade_for_audio_is_disabled`, `..._profile_has_no_audio_formats`), plus all pre-feature tests in the same fixtures still green (58 + 29) → default behavior unchanged. |
| 9 | Null MediaInfo | PASS (automated) | `should_return_false_if_existing_episode_file_has_no_media_info`, `should_return_false_if_local_episode_has_no_media_info`, CalcService `should_return_empty_when_media_info_is_null` + episode-file/local-episode variants, hint spec `should_accept_when_existing_file_has_no_media_info`. No crash paths: all audio logic is null-guarded. |
| 10 | Manual import UI | **FAIL — gap** | Data is computed (3.1: `LocalEpisode.AudioFormats/Score`) but never surfaced: `ManualImportItem`/`ManualImportResource` have no audio fields, `InteractiveImportRow.tsx` renders only the CF badge. 3.1's Result had flagged this as "needs API/frontend changes (phase 4)" but no phase-4 task covered it. → **Created task 4.6** (`phase-4-frontend/task-4-6-manual-import-audio-display.md`), added to 5.3's `Depends`. |
| 11 | Re-analyze | PASS (mechanism) | `CURRENT/MINIMUM_MEDIA_INFO_SCHEMA_REVISION` bumped 14→15 in bdcff24fda (1.1); `UpdateMediaInfoService.cs:51` re-analyzes files with `SchemaRevision < MINIMUM` → "Update Media Info" refreshes the existing library; disposition flags persisted. `UpdateMediaInfoServiceFixture` 13 passed. |

Still requires a human before merge (flag in 5.3 regression review):
- Visual browser QA of the settings page, profile editor and interactive-import rows (4.2/4.3/4.5
  also deferred browser QA to this task).
- Live "Update Media Info" run against a real library (item 11 verified by mechanism + unit tests).
- Item 10 end-to-end once task 4.6 lands.

Commit: _(fill after commit)_
