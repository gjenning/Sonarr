# Task 4.3: Edit modal + specification editor

Status: DONE
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
- [ ] Create/edit/delete an audio format end-to-end against a running dev server. (Deferred to 5.2 — no dev server run here; see Result.)
- [x] `yarn build` passes.

## Result
Done 2026-10-16.

- Replaced the placeholder `EditAudioFormatModal.tsx` with the real modal: thin `Modal`
  wrapper + new `EditAudioFormatModalContent.tsx` (mirrors CF's `EditCustomFormatModalContent`
  minus Import/Export — no such endpoints). Name field, Conditions section with spec cards,
  add-condition card, save via `useManageAudioFormat().saveAudioFormat` (auto-closes on
  success), validation errors surfaced through `Form` + `SpinnerErrorButton` (duplicate name,
  empty specs, bad regex come back from the API and render like CF).
- Created `Specifications/` (spec row editor):
  - `Specification.tsx` — card (name, implementation label, negate/required labels, clone,
    edit → spec modal, delete confirm), mirrors CF `Specification.tsx`.
  - `EditSpecificationModal.tsx` + `EditSpecificationModalContent.tsx` — pending-changes
    pattern from CF (`usePendingChangesStore` + `usePendingFieldsStore` + `selectSettings`),
    but with a purpose-built form instead of generic `ProviderFieldFormGroup` rendering:
    Name, `Field` select (options from the spec's schema `selectOptions`), field-dependent
    value widget, Negate, Required. Switching `Field` resets `Value` (values are not
    interchangeable across widgets).
  - `audioStreamValue.ts` — widget map + value (de)serialization. The 2.3 schema is the
    simple shape (one `field` select + one free-text `value`, no per-field widget hints),
    so the widget map is hardcoded in the frontend as the task anticipated:
    Codec/Channels/Title → text (regex), Bitrate/StreamCount → numeric (operator prefix
    `>=,<=,>,<,=` preserved on round-trip, bare number = `>=`, kbps unit shown for Bitrate),
    Language → multi-select of `/language` ids stored as `"1,3"`, AudioDescription →
    Yes/No select (`true`/`false`; "any" omitted — the backend validator requires a bool).
- Added 4 keys to `src/NzbDrone.Core/Localization/Core/en.json`
  (`CustomAudioFormatsMatchInfo`, `AudioFormatValueHelpText{Number,Language,AudioDescription}`),
  removed the now-unused `CustomAudioFormatEditorPlaceholder`.

Deviations from the task file:
- Files live under `frontend/src/Settings/AudioFormats/AudioFormats/` (not `.../Edit/`):
  4.1/4.2 established the `AudioFormats/AudioFormats/` layout mirroring CF, and the modal
  was already wired in `AudioFormats.tsx`/`AudioFormat.tsx`, so no change was needed there.
- `LanguageSelector` doesn't exist in this repo; used `useFilteredLanguages` +
  `EnhancedSelectInput` (multi-select) — same data source as `LanguageSelectInput`, but
  multi-select because the backend `Value` holds comma-separated ids and group semantics
  (all specs must pass on one stream) make single-language-per-spec too limiting.
- Add-condition skips CF's implementation-picker modal (only one implementation): the new
  spec is seeded from the first `/audioformat/schema` entry with `id: 0, name: ''`.
- Delete stays on the format card (4.2 pattern), not the modal footer.
- End-to-end browser check not performed (no dev server run in this session), same as 4.2;
  deferred to task 5.2 manual QA.

Verify: `yarn typecheck` exit 0, `yarn build` exit 0 (ran `tcm` + `tsc` + `vite build` from
`node_modules/.bin` — yarn not globally installed); eslint clean on all new/changed files;
en.json valid JSON.

Commit: 62fd08efa8
