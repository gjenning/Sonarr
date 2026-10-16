# Task 4.4: Quality profile editor section

Status: TODO
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
- [ ] Editing scores/toggles persists and reloads correctly.
- [ ] `yarn build` passes.

## Result
_(fill on completion)_
