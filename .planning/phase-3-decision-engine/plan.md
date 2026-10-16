# Phase 3 — Decision Engine (grab + import gates)

**Goal:** the audio score actually drives decisions:
- **Import:** downloaded files are scored; an equal-video-quality release with a higher audio
  score (while below audio cutoff) is approved as an upgrade → existing file replaced via the
  normal upgrade path.
- **Grab:** past-cutoff grabs allowed when `UpgradeForAudio` and existing audio score <
  `CutoffAudioFormatScore`, optionally gated on an audio keyword in the release title.

Depends on Phases 1–2. The hard invariant: with no audio formats defined / `UpgradeForAudio`
false, every decision must be byte-for-byte what it is today (regression fixtures prove it).

| Task | Title |
|------|-------|
| 3.1 | `LocalEpisode.AudioFormats` + import-time scoring hook |
| 3.2 | Audio-aware import `UpgradeSpecification` |
| 3.3 | Audio-aware grab cutoff (`UpgradableSpecification` / `UpgradeDiskSpecification`) |
| 3.4 | Title-hint pre-filter spec |
| 3.5 | `MinAudioFormatScore` import gate (optional) |

Verify at phase end:
```bash
dotnet build src/Sonarr.sln -c Debug
dotnet test src/NzbDrone.Core.Test -c Debug --filter "FullyQualifiedName~DecisionEngineTests|FullyQualifiedName~EpisodeImport"
```
