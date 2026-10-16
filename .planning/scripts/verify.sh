#!/usr/bin/env bash
# Run a task's Verify command. Usage:
#   bash scripts/verify.sh 1.5        # run verify for task 1.5
#   bash scripts/verify.sh            # run verify for the current IN_PROGRESS task
# The Verify line is a shell command (backticks stripped), run from the repo root.
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
REPO="$(cd "$ROOT/.." && pwd)"

id="${1:-}"
if [[ -z "$id" ]]; then
  for f in "$ROOT"/phase-*/task-*.md; do
    grep -q '^Status: IN_PROGRESS' "$f" && id="$(basename "$f" | sed -E 's/^task-([0-9]+)-([0-9]+).*$/\1.\2/')" && break
  done
fi
if [[ -z "$id" ]]; then
  echo "No task id given and no task is IN_PROGRESS." >&2
  exit 1
fi

file="$(ls "$ROOT"/phase-*/task-"${id/./-}"-*.md 2>/dev/null | head -1 || true)"
if [[ -z "$file" ]]; then
  echo "No task file matches id $id" >&2
  exit 1
fi

cmd="$(grep -m1 '^Verify:' "$file" | sed -E 's/^Verify:[[:space:]]*//; s/`//g')"
if [[ -z "$cmd" || "$cmd" == manual* ]]; then
  echo "Task $id has no automated verify command: '${cmd:-<none>}'"
  exit 0
fi

echo "Verifying task $id ($(basename "$file")):"
echo "  $cmd"
echo "---"
cd "$REPO"
eval "$cmd"
