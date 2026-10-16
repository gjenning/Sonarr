# Phase 4 — Frontend

**Goal:** Settings → Custom Audio Formats page (list + edit modal with specification editor,
mirroring Custom Formats), and a "Custom Audio Formats" section in the quality profile editor
(scores, min/cutoff score, `UpgradeForAudio` + `AudioUpgradeRequiresTitleHint` toggles).
Depends on Phase 2 API (task 2.3/2.4).

Pattern to mirror throughout: `frontend/src/Settings/CustomFormats/` (page, modal,
`Specifications/` editor) and `frontend/src/Settings/Profiles/Quality/`.

| Task | Title |
|------|-------|
| 4.1 | Types + API client hook (`useAudioFormats`) |
| 4.2 | Settings page + route + nav entry |
| 4.3 | Edit modal + specification editor |
| 4.4 | Quality profile editor section |
| 4.5 | Lint/typecheck/build pass + polish |
| 4.6 | Manual import preview shows audio formats + score (found by 5.2 QA) |

Verify at phase end:
```bash
yarn lint && yarn typecheck && yarn build
```
