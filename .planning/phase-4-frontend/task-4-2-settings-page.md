# Task 4.2: Settings page + route + nav entry

Status: DONE
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
- [x] Page renders at `/settings/audioformats` with empty state; nav entry works.
- [x] `yarn build` passes.

## Result
Done 2026-10-16.

- Created `frontend/src/Settings/AudioFormats/AudioFormatSettingsPage.tsx` (mirrors
  `CustomFormatSettingsPage.tsx` minus the Manage modal/toolbar, which is out of scope here).
- Created `frontend/src/Settings/AudioFormats/AudioFormats/AudioFormats.tsx` (card grid +
  AddCard, uses `useSortedAudioFormats` from task 4.1) and `AudioFormat.tsx` card (name,
  conditions count, spec labels, clone + delete with confirm modal via `useDeleteAudioFormat`).
- Created `frontend/src/Settings/AudioFormats/AudioFormats/EditAudioFormatModal.tsx` —
  placeholder modal (header + "coming soon" body + Close) to be replaced by task 4.3.
- Modified `frontend/src/App/AppRoutes.tsx`: route `/settings/audioformats`.
- Modified `frontend/src/Settings/Settings.tsx`: nav card after "Custom Formats".
- Extra small support changes (beyond the Files list): added `AUDIO_FORMAT` icon
  (lucide `AudioLines`) to `frontend/src/Helpers/Props/icons.ts`, and 10 new keys to
  `src/NzbDrone.Core/Localization/Core/en.json` (CustomAudioFormats*, Add/Clone/Delete/Edit
  CustomAudioFormat*, placeholder text).

Deviations from the task file:
- Routes in this repo are static imports, not lazy-loaded — followed repo convention.
- CF has no `index.ts`, so none created.
- Delete lives on the card (clone + trash actions) instead of inside the edit modal footer
  like CF, because the edit modal is stubbed; task 4.3 may move it into the modal footer.
- No Export action: `AudioFormatController` has no export/bulk endpoints.
- Rendering not verified in a live browser (no dev server run here); wired + built clean.
  Manual browser check deferred to task 5.2 QA.

Verify: `yarn typecheck` exit 0, `yarn build` exit 0 (via `npx yarn@1.22.22` — yarn not
globally installed); eslint clean on all touched frontend files; en.json valid JSON.
