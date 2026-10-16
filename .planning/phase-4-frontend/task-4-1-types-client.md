# Task 4.1: Types + API client hook

Status: DONE
Phase: 4
Depends: 2.3, 2.4
Verify: `yarn typecheck`

## Goal
Frontend TypeScript types and API client for audio formats, mirroring the Custom Formats ones.

## Context
Mirror set (grep/read each before writing):
- `frontend/src/typings/CustomFormat.ts` (+ `frontend/src/typings/QualityProfile.ts` for
  `formatItems` shape) → create `frontend/src/typings/AudioFormat.ts`; extend the QualityProfile
  typings with `audioFormatItems`, `minAudioFormatScore`, `cutoffAudioFormatScore`,
  `upgradeForAudio`, `audioUpgradeRequiresTitleHint`.
- `frontend/src/Settings/CustomFormats/CustomFormats.ts` (or the `useCustomFormats` query hook —
  grep `useCustomFormats` to find the react-query wrapper + `client.get('/v3/customformat')`
  calls) → `useAudioFormats` / `useAudioFormatSchema` hitting `/v3/audioformat` and
  `/v3/audioformat/schema`.
- Add the new profile fields to the profile default/blank object used by the profile editor
  (grep `minFormatScore` in `frontend/src/Settings/Profiles/Quality/` to find defaults).

## Files
- Create: `frontend/src/typings/AudioFormat.ts`
- Modify: `frontend/src/typings/QualityProfile.ts`
- Create: audio format hooks file (same location/naming convention as CF hooks)
- Modify: profile editor defaults (locate via grep)

## Acceptance Criteria
- [ ] `yarn typecheck` passes.
- [ ] Hooks follow existing query-key conventions (`queryKey: ['audioformat']` style).

## Result
Done 2026-10-16.

- Created `frontend/src/typings/AudioFormat.ts`: `AudioFormat` + `AudioFormatSpecification`
  interfaces mirroring the backend `AudioFormatResource` / `AudioFormatSpecificationSchema`
  (no `includeCustomFormatWhenRenaming` — the audio resource has no such field).
- Extended `QualityProfileModel` in `frontend/src/Settings/Profiles/Quality/useQualityProfiles.ts`
  with `audioFormatItems` (new `QualityProfileAudioFormatItem` interface, same shape as
  `QualityProfileFormatItem`), `minAudioFormatScore`, `cutoffAudioFormatScore`,
  `upgradeForAudio`, `audioUpgradeRequiresTitleHint` — names match `QualityProfileResource`.
- Created `frontend/src/Settings/AudioFormats/AudioFormats/useAudioFormats.ts` mirroring
  `useCustomFormats.ts`: `useAudioFormats`, `useSortedAudioFormats`, `useAudioFormat`,
  `useAudioFormatsWithIds`, `useDeleteAudioFormat`, `useAudioFormatSchema`,
  `useManageAudioFormat` (incl. spec save/delete/clone helpers for task 4.3). Query keys
  derive from path via `useApiQuery` → `['/audioformat']`, same convention as `['/customformat']`.

Deviations from the task file (repo layout differs):
- `frontend/src/typings/CustomFormat.ts` and `frontend/src/typings/QualityProfile.ts` do not
  exist; CF types live in `Settings/CustomFormats/CustomFormats/useCustomFormats.ts` and the
  profile model in `Settings/Profiles/Quality/useQualityProfiles.ts`. Types were placed in
  `typings/AudioFormat.ts` per the Files list; profile fields went into `useQualityProfiles.ts`.
- No hardcoded profile default/blank object exists (grep `minFormatScore` only hits the model +
  edit modal); new-profile defaults come from the server `/qualityprofile/schema`, which already
  includes the new fields (task 2.4), so no defaults change was needed.
- Omitted bulk edit/delete hooks: `AudioFormatController` has no bulk routes.

Verify: `yarn typecheck` exit 0 (via `npx yarn@1.22.22` — yarn not globally installed);
eslint clean on all three files.
