# Custom Audio Formats — Implementation Framework

Feature: score custom **audio** formats (matched against MediaInfo audio streams) on quality
profiles, and allow episode-file upgrades for better audio even when the video quality /
custom-format cutoff is already met. Full design: [`00-overview.md`](00-overview.md).

## Layout

```
.planning/
├── README.md            ← this file (protocol + commands)
├── 00-overview.md       ← architecture, decisions, phase map (read first, once)
├── STATUS.md            ← generated dashboard (scripts/status.sh)
├── HANDOFF.md           ← generated session handoff (scripts/handoff.sh)
├── scripts/
│   ├── status.sh
│   ├── handoff.sh
│   └── verify.sh        ← run one task's Verify command
└── phase-N-*/
    ├── plan.md          ← phase goal + task list (short)
    └── task-N-M-*.md    ← self-contained task spec (the unit of work)
```

## Task lifecycle (one task per session — keep context small)

1. `bash scripts/handoff.sh` → prints + writes `HANDOFF.md` pointing at the next task.
2. Open that task file. Set `Status: IN_PROGRESS`.
3. Implement **only** what the task file says. The task file is self-contained; you should not
   need to re-explore the codebase beyond the files it lists.
4. Run the task's **Verify** command(s). They must pass before closing.
5. Fill in the `## Result` section (what was done, deviations, commit hash). Set `Status: DONE`
   (or `BLOCKED` + reason).
6. Commit locally: `git add -A && git commit -m "feat(audio-formats): <task id> <short summary>"`.
7. `bash scripts/handoff.sh` to regenerate `HANDOFF.md` / `STATUS.md`; amend or commit them.
8. Stop or pick the next task. **Never start a task whose `Depends:` are not all `DONE`.**

Rules:
- One task = one focused change (≤ ~5 files). If a task balloons, note it in `## Result` and
  split it in a follow-up task file.
- No pushing, no PRs, no GitHub writes — local commits only (see repo AGENTS.md guardrails).
- If reality diverges from the plan (API changed, file moved), update the task file's Steps and
  record the deviation in `## Result` so the next session isn't misled.

## Commands

```bash
bash .planning/scripts/status.sh    # progress dashboard → STATUS.md
bash .planning/scripts/handoff.sh   # next-task handoff → HANDOFF.md
bash .planning/scripts/verify.sh 1.5  # run task 1.5's verify command (or current IN_PROGRESS task)

# Backend (repo root)
dotnet build src/Sonarr.sln -c Debug
dotnet test src/NzbDrone.Core.Test -c Debug --filter "FullyQualifiedName~AudioFormats"
dotnet test src/NzbDrone.Core.Test -c Debug --filter "FullyQualifiedName~UpgradeSpecification"

# Frontend (repo root)
yarn lint
yarn typecheck
yarn build
```

Each task file lists the *targeted* verify command for that task; run the full build + full
`NzbDrone.Core.Test` suite at phase boundaries and before merging (task 5.3).

## Status vocabulary

`TODO` → `IN_PROGRESS` → `DONE` (or `BLOCKED` with a reason in `## Result`).
Status is the line `Status: <VALUE>` near the top of each task file — scripts parse it.
