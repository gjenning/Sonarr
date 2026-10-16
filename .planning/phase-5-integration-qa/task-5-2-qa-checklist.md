# Task 5.2: Manual QA checklist

Status: TODO
Phase: 5
Depends: 3.2, 3.3, 3.4, 4.4
Verify: manual (checklist executed against a dev instance)

## Goal
Written, executed checklist for the end-to-end flows. Save results in the `## Result` section.

## Checklist
Setup: dev instance, a series with an existing episode file (AC3 5.1), one audio format
(`Codec ~ truehd|dts-hd` score 10, `NOT AudioDescription`), profile with `UpgradeForAudio` on,
`CutoffAudioFormatScore` 10, hint toggle exercised both ways.

- [ ] **Import upgrade**: manually import a TrueHD release of the same video quality → accepted
      as upgrade, old file replaced, episode file now shows TrueHD.
- [ ] **No improvement**: import an AC3 release with different title → rejected (no audio gain).
- [ ] **At cutoff**: after the TrueHD import, another TrueHD release → rejected (cutoff met).
- [ ] **Video downgrade blocked**: 720p TrueHD vs existing 1080p AC3 → rejected.
- [ ] **AD exclusion**: TrueHD release whose audio track is flagged visual_impaired → does not
      match the format (check log / file's matched formats).
- [ ] **Grab bypass + hint on**: cutoff met, existing file below audio cutoff; indexer release
      "Show.S01E01...1080p...TRUEHD" → grabbed; release without keyword → not grabbed.
- [ ] **Hint off**: keywordless same-quality release now grabbed.
- [ ] **Feature off**: `UpgradeForAudio` false → behavior identical to pre-feature (spot check).
- [ ] **Null MediaInfo**: file with no MediaInfo → no crashes, audio logic skipped (check logs).
- [ ] **Manual import UI**: manual import preview shows audio formats/score where CF score shows.
- [ ] **Re-analyze**: after schema bump, existing library MediaInfo refreshes (Update Media Info
      task) and disposition flags appear.

## Files
- This file (fill `## Result` with pass/fail + notes)

## Result
_(fill on completion)_
