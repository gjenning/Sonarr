# Task 5.3: Final gate — full build + full test suite

Status: DONE
Phase: 5
Depends: 5.1, 5.2, 4.5, 4.6
Verify: `dotnet build src/Sonarr.sln -c Debug && dotnet test src/NzbDrone.Core.Test -c Debug`

## Goal
Everything green, whole repo. The last gate before handing off for review/PR (per AGENTS.md the
human opens the PR).

## Steps
1. `dotnet build src/Sonarr.sln -c Debug` — zero warnings introduced beyond baseline.
2. `dotnet test src/NzbDrone.Core.Test -c Debug` — full Core suite.
3. `dotnet test src/NzbDrone.Integration.Test -c Debug` (if feasible locally).
4. `yarn lint && yarn typecheck && yarn build`.
5. `git log --oneline` review: one commit per task, messages describe the change; squash-fix
   anything sloppy BEFORE handing to the human.
6. Write a short feature summary (what/why/how-to-test) in `## Result` for the human to base
   the PR description on — the human writes the actual PR text themselves (AGENTS.md).

## Acceptance Criteria
- [x] All commands above pass. (12 Core failures proven pre-existing at baseline `a2b6054a8f` — identical set; see Result.)
- [x] Feature summary written.

## Result
Executed 2026-10-16. All gates green; **zero regressions from the feature**.

1. **Build** — `dotnet build src/Sonarr.sln -c Debug` → succeeded, **0 warnings, 0 errors**.
2. **Core suite** — `dotnet test src/NzbDrone.Core.Test -c Debug` → 5557 passed, 12 failed, 28
   skipped. The 12 failures are **pre-existing**: rebuilt the pre-feature baseline (`a2b6054a8f`,
   the commit immediately before 1.1) in a throwaway worktree and ran the full suite there — the
   failing set is identical (1 Deluge reconnect, 8 import-block/title-mismatch fixtures, 2 auth
   user-seeding tests). All 12 share one harness pattern ("0 Warn(s) were expected but N were
   logged" — warnings that are part of each test's own scenario counted as unexpected; an
   environment/harness issue unrelated to audio formats). Targeted: `~AudioFormats` 96/96,
   `~UpgradeSpecification` 58/58.
3. **Integration suite** — `dotnet test src/NzbDrone.Integration.Test -c Debug` → 96 passed,
   0 failed, 12 skipped (skips are pre-existing network/env-dependent tests).
4. **Frontend** — `yarn lint` ✅, `yarn typecheck` ✅ (types:css + `tsc --noEmit`), `yarn build` ✅
   (only the pre-existing chunk-size warning).
5. **git log review** — one `feat(audio-formats): N.M` commit per task plus separate planning-doc
   commits; messages describe each change; nothing to squash-fix.

### Feature summary (source material for the human's PR description — rewrite in own words per AGENTS.md)

**What.** Custom Audio Formats: a scoring system parallel to Custom Formats, matched against a
file's MediaInfo **audio streams** instead of the release title. One generic
`AudioStreamSpecification` with a `Field` selector (Codec, Channels, Bitrate, Title, Language,
AudioDescription, StreamCount) covers all conditions; a format matches when at least one audio
stream satisfies all specs in its group. Quality profiles gain `AudioFormatItems`,
`MinAudioFormatScore`, `CutoffAudioFormatScore`, `UpgradeForAudio`, and
`AudioUpgradeRequiresTitleHint` (DB migration 237). With `UpgradeForAudio` on, Sonarr can upgrade
an existing episode file for better audio **even when the video-quality and CF cutoffs are
already met** — only at equal video quality; video downgrades for audio are never allowed.

**Why.** Audio quality (TrueHD / DTS-HD MA / Atmos, channel layout, language, audio description)
is invisible to the existing quality/CF cutoff logic, so Sonarr cannot prefer or upgrade for it.
This makes audio an explicit, scoreable dimension across the whole pipeline: grab decisions,
import gating, and UI.

**How it works.**
- *Scoring*: `AudioFormatCalculationService` scores MediaInfo audio streams at import
  (`ImportDecisionMaker` hook → `LocalEpisode.AudioFormats/Score`), mirroring the CF pipeline.
- *Import gate*: `UpgradeSpecification` accepts a same-video-quality file only if the audio score
  improves and stays under the audio cutoff; `MinAudioFormatScore` blocks low-score imports.
- *Grab gate*: `UpgradableSpecification`/`UpgradeDiskSpecification` allow past-cutoff grabs when
  the existing file's audio score is below `CutoffAudioFormatScore`;
  `AudioUpgradeTitleHintSpecification` (default on) additionally requires an audio keyword
  (TrueHD, ATMOS, DTS-HD…) in the release title to avoid burning download slots.
- *API*: `AudioFormatController` (CRUD + `GET schema`) on V3 and V5; profile resource/validator
  updates on both API versions.
- *UI*: Settings → Custom Audio Formats page (editor + specification editor), quality-profile
  editor audio section (scores, cutoffs, toggles), manual-import preview shows audio
  formats/score next to the CF badge.
- *Re-analysis*: MediaInfo schema revision bumped 14→15 so "Update Media Info" refreshes existing
  library files with the new audio disposition fields. Profiles with no audio formats /
  `UpgradeForAudio=false` behave exactly as before (regression-tested in every phase).

**How to test.**
1. Settings → Custom Audio Formats: create a format, e.g. `Codec ~ truehd|dts-hd` score 10 plus a
   negated `AudioDescription` spec.
2. Edit a quality profile: add the format, set an audio cutoff, enable `UpgradeForAudio`.
3. Import a same-video-quality release with better audio → accepted as upgrade, old file
   recycled; an AC3 release → rejected (no gain); a second TrueHD at cutoff → rejected.
4. With cutoffs met: a release titled "…TRUEHD…" is grabbed (title hint on); a keywordless
   same-quality release is not — flipping the hint toggle changes this.
5. Disable `UpgradeForAudio` → behavior identical to pre-feature.
6. Run "Update Media Info" — existing files re-analyze and gain disposition flags.

**Still requires a human before merge** (carried over from 5.2): visual browser QA of the settings
page, profile editor and interactive-import rows; a live "Update Media Info" run against a real
library.

**Deviations / notes.** None. Two environment notes: (a) building a project directly (not via the
solution) fails StyleCop because `stylecop.json` is included via `$(SolutionDir)` — use the
solution build or `-p:SolutionDir=$PWD/src/`; (b) `yarn` is not installed globally here — run via
`npx yarn@1.22.22` (repo-pinned version).

Commit: 217240067f
