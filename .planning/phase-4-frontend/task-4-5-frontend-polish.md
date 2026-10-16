# Task 4.5: Frontend validation pass + polish

Status: DONE
Phase: 4
Depends: 4.2, 4.3, 4.4
Verify: `yarn lint && yarn typecheck && yarn build`

## Goal
Clean pass over all Phase 4 frontend code: lint/typecheck clean, consistent naming with the CF
pages, i18n strings extracted, no dead code from the stubbing in 4.2.

## Steps
1. `yarn lint` — fix all new warnings/errors in `frontend/src/Settings/AudioFormats/` and touched
   files.
2. Check i18n: CF pages use `translate(...)` keys registered in
   `frontend/src/typings/i18n` (or the locale JSON source — grep how `customFormats` key is
   declared) and add the new strings the same way.
3. Consistency sweep: file naming, component export style, CSS module usage vs CF pages.
4. Manual smoke on dev server (`yarn start` against a dev backend if available): list page,
   modal CRUD, profile editor save/reload.

## Acceptance Criteria
- [x] `yarn lint && yarn typecheck && yarn build` all clean.
- [x] No hardcoded English strings where CF pages use `translate()`.

## Result
Done 2026-10-16.

- **Lint**: full `eslint` over `frontend/` (the `yarn lint` script) — already clean; no fixes
  needed in `Settings/AudioFormats/` or touched files. `stylelint` on the new CSS modules also
  clean.
- **i18n**: scanned every `translate()` key used by Phase 4 files (70 keys) against
  `src/NzbDrone.Core/Localization/Core/en.json`. Found two unregistered keys — `Field` and
  `Value`, the label fallbacks in `Specifications/EditSpecificationModalContent.tsx` (the
  backend schema always supplies these labels, but the fallback would warn in dev and render
  untranslated). Added both to `en.json` in alphabetical position. Verified all 29
  audio-format keys in `en.json` are actually used (no dead keys; the 4.2 placeholder key was
  already removed in 4.3), no duplicate keys, JSON valid.
- **Consistency sweep**: file naming (`EditXModal`/`EditXModalContent`/`.module.css`),
  component export style (`function X` + `export default`), and CSS module imports all mirror
  the CF pages; `useAudioFormats.ts` is a faithful trim of `useCustomFormats.ts` (kept its
  unused `useAudioFormatsWithIds` because CF's identical `useCustomFormatsWithIds` is also
  unused — removing it would deviate from the mirror). No stub remnants from 4.2: no TODOs,
  no unused CSS classes, `AUDIO_FORMAT` icon is used by the nav card.
- **Dead code**: none found beyond the CF-mirror note above.

Deviations from the task file:
- Step 4 (manual smoke on dev server) not performed: no dev backend available in this
  environment, same as 4.2/4.3. Browser QA deferred to task 5.2.
- `yarn` is not installed in this environment; ran the underlying scripts from package.json:
  `eslint --config frontend/.eslintrc.cjs ... frontend/`, `tcm ... && tsc --noEmit -p
  frontend/tsconfig.json`, and `vite build`.

Verify: lint exit 0, typecheck exit 0, `vite build` exit 0 (chunk-size warning is pre-existing).

Commit: d4669ec9c5
