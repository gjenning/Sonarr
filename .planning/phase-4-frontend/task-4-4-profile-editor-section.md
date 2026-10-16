# Task 4.4: Quality profile editor section

Status: DONE
Phase: 4
Depends: 4.1, 4.3
Verify: `yarn typecheck && yarn build`

## Goal
"Custom Audio Formats" section in the quality profile editor: per-format score list, min/cutoff
score inputs, and the two toggles — mirroring the existing Custom Formats section there.

## Context
Reference: the Custom Formats section inside
`frontend/src/Settings/Profiles/Quality/Edit/EditQualityProfileModalContent.tsx` (renders
`formatItems` with score inputs + `minFormatScore`/`cutoffFormatScore` inputs — find the exact
sub-components it uses, e.g. a `QualityProfileFormatItem`-style row component, and mirror).
Add:
- Score rows for each audio format (from `useAudioFormats`), stored in `audioFormatItems`.
- `minAudioFormatScore` + `cutoffAudioFormatScore` number inputs.
- `upgradeForAudio` toggle (label: "Upgrade for audio") — and a dependent `audioUpgradeRequiresTitleHint`
  toggle (label: "Require audio hint in release title"), shown/enabled only when upgradeForAudio.
- Help text (tooltips) explaining: upgrades only when video quality equal; grab-time hint filter.
- Ensure the profile form state (whatever reducer/form library pattern the modal uses) round-trips
  the new fields on save — check `EditQualityProfileModal.tsx` submit mapping.

## Files
- Modify: `frontend/src/Settings/Profiles/Quality/Edit/EditQualityProfileModalContent.tsx`
- Create: audio format score row component if CF uses a dedicated one
- Modify: profile form state/submit mapping as needed

## Acceptance Criteria
- [x] Editing scores/toggles persists and reloads correctly.
- [x] `yarn build` passes.

## Result
Added a "Custom Audio Formats" section to the quality profile editor, mirroring the Custom
Formats section:

- **Created** `frontend/src/Settings/Profiles/Quality/QualityProfileAudioFormatItems.tsx` —
  score list (sorted score desc, then name), errors/warnings display, empty-state message
  linking to `/settings/audioformats`. Reuses the generic `QualityProfileFormatItem` row
  component and `QualityProfileFormatItems.module.css` styles (no new CSS module needed).
- **Modified** `EditQualityProfileModalContent.tsx`:
  - Score rows for `audioFormatItems` (rendered in both `formatItemLarge` and
    `formatItemSmall` containers, same responsive pattern as CF), with a debounced
    `handleAudioFormatItemScoreChange` mirroring the CF one.
  - `upgradeForAudio` check input (label "Upgrade for Audio") + `audioUpgradeRequiresTitleHint`
    check input (label "Require Audio Hint in Release Title"), the latter shown only when
    `upgradeForAudio` is on.
  - `minAudioFormatScore` number input; `cutoffAudioFormatScore` number input shown only when
    `upgradeForAudio` is on (audio cutoff is meaningless without it).
  - Score inputs and toggles are hidden when no audio formats exist, mirroring CF behavior;
    the list then shows the "add a custom audio format" message.
  - Help texts explain: audio upgrades only replace files with equal video quality; the
    title-hint grab-time filter.
- **Modified** `src/NzbDrone.Core/Localization/Core/en.json` — added 11 English keys
  (`UpgradeForAudio`, `AudioUpgradeRequiresTitleHint`, `MinimumAudioFormatScore`,
  `UpgradeUntilAudioFormatScore` + help texts, `CustomAudioFormat`, `CustomAudioFormatHelpText`,
  `WantMoreControlAddACustomAudioFormat`).

Deviations from the task file:
- Actual file path is `frontend/src/Settings/Profiles/Quality/EditQualityProfileModalContent.tsx`
  (no `Edit/` subfolder as the task file stated); `EditQualityProfileModal.tsx` likewise.
- No submit-mapping change was needed: `useManageQualityProfile` → `useManageProviderSettings`
  is schema-driven and `saveProvider` spreads `{...provider, ...changedValues}`, so the new
  fields round-trip automatically; `updateValue('audioFormatItems' | 'upgradeForAudio' | ...)`
  flows through the same pending-changes store as `formatItems`.
- `yarn` is not installed in this environment; ran the underlying commands instead:
  `node_modules/.bin/tcm ... && tsc --noEmit -p frontend/tsconfig.json` (typecheck) and
  `node_modules/.bin/vite build` (build). Both pass; eslint (with --fix) clean on changed files.
