# Task 5.3: Final gate — full build + full test suite

Status: TODO
Phase: 5
Depends: 5.1, 5.2, 4.5, 4.6
Verify: `dotnet build src/Sonarr.sln -c Debug && dotnet test src/NzbDrone.Core.Test -c Debug`

## Goal
Everything green, whole repo. The last gate before handing off for review/PR (per AGENTS.md the
human opens the PR).

## Steps
1. `dotnet build src/Sonarr.sln -c Debug` — zero warnings introduced beyond baseline.
2. `dotnet test src/NzbDrone.Core.Test -c Debug` — full Core suite.
3. `dotnet test src/NzbDrone.Integration.Test -c Debug` (if feasible locally).
4. `yarn lint && yarn typecheck && yarn build`.
5. `git log --oneline` review: one commit per task, messages describe the change; squash-fix
   anything sloppy BEFORE handing to the human.
6. Write a short feature summary (what/why/how-to-test) in `## Result` for the human to base
   the PR description on — the human writes the actual PR text themselves (AGENTS.md).

## Acceptance Criteria
- [ ] All commands above pass.
- [ ] Feature summary written.

## Result
_(fill on completion)_
