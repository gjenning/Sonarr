# Task 4.2: Settings page + route + nav entry

Status: TODO
Phase: 4
Depends: 4.1
Verify: `yarn typecheck && yarn build`

## Goal
"Custom Audio Formats" page under Settings, listing audio formats with add/edit/delete, mirroring
Settings → Custom Formats.

## Context
Copy structure from `frontend/src/Settings/CustomFormats/`:
- `CustomFormats/CustomFormats.tsx` (list page: table of name + specs summary + actions) →
  `frontend/src/Settings/AudioFormats/AudioFormats.tsx`.
- Route: `frontend/src/App/AppRoutes.tsx` — find the `settings/customformats` route entry and
  add `settings/audioformats` (lazy-loaded the same way).
- Nav: `frontend/src/Settings/Settings.tsx` — add the nav item next to "Custom Formats"
  (label "Custom Audio Formats"; check how `settingsOnly`/permissions are applied there).
- Page title/breadcrumbs: mirror what the CF page sets.
- The edit modal is task 4.3 — stub the "Add/Edit" buttons to a placeholder component or gate
  them behind the modal import so this task compiles standalone.

## Files
- Create: `frontend/src/Settings/AudioFormats/AudioFormats.tsx` (+ `index.ts` if CF has one)
- Modify: `frontend/src/App/AppRoutes.tsx`
- Modify: `frontend/src/Settings/Settings.tsx`

## Acceptance Criteria
- [ ] Page renders at `/settings/audioformats` with empty state; nav entry works.
- [ ] `yarn build` passes.

## Result
_(fill on completion)_
