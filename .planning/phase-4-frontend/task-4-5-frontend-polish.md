# Task 4.5: Frontend validation pass + polish

Status: TODO
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
- [ ] `yarn lint && yarn typecheck && yarn build` all clean.
- [ ] No hardcoded English strings where CF pages use `translate()`.

## Result
_(fill on completion)_
