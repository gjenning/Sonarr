# Task 4.3: Edit modal + specification editor

Status: TODO
Phase: 4
Depends: 4.2
Verify: `yarn typecheck && yarn build`

## Goal
Modal to create/edit an audio format: name + list of `AudioStreamSpecification` rows
(field selector, value input, negate/required toggles), driven by the `/schema` endpoint.

## Context
Mirror `frontend/src/Settings/CustomFormats/CustomFormats/Edit/` (edit modal) and
`frontend/src/Settings/CustomFormats/Specifications/` (spec row editor + `Field` input rendering:
text, enum select, tags…). Key differences from CF:
- Only ONE implementation (`AudioStreamSpecification`), so each spec row = `Field` select +
  value input whose widget depends on the field:
  - `Codec`, `Channels`, `Title` → text input (regex)
  - `Bitrate`, `StreamCount` → numeric input
  - `Language` → the existing language selector component (grep `LanguageSelector` in
    `frontend/src/`; CF's language spec UI is the reference)
  - `AudioDescription` → boolean select (yes/no/any)
- The schema endpoint (task 2.3) must carry the `Field` enum options + per-field widget hints;
  adapt rendering to whatever shape 2.3 exposes (coordinate — if 2.3's schema is simpler,
  hardcode the widget map in the frontend and note it).
- Wire save/delete through the 4.1 hooks; show validation errors (duplicate name, bad regex)
  using the CF modal's error handling pattern.

## Files
- Create: `frontend/src/Settings/AudioFormats/Edit/` (modal + content, mirror CF Edit/)
- Create: `frontend/src/Settings/AudioFormats/Specifications/` (spec row editor)
- Modify: `frontend/src/Settings/AudioFormats/AudioFormats.tsx` (wire real modal)

## Acceptance Criteria
- [ ] Create/edit/delete an audio format end-to-end against a running dev server.
- [ ] `yarn build` passes.

## Result
_(fill on completion)_
