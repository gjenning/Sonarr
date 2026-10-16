# Task 4.1: Types + API client hook

Status: TODO
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
_(fill on completion)_
