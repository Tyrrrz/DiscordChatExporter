---
title: fix: Harden recurring scrape scripts from code review residuals
type: fix
status: completed
date: 2026-05-27
origin: PR #1538 residual review findings
---

# fix: Harden recurring scrape scripts from code review residuals

## Summary

Address actionable security and correctness findings deferred from the finalization validation pass on `feat/recurring-cli-scrape`. Scope is limited to script-layer hardening and CI wiring—no C# exporter changes.

## Problem Frame

The recurring scrape automation is validated by smoke tests but still has review-flagged risks: arbitrary command execution via `eval`/`bash -lc`, unvalidated custom cron expressions, incorrect incremental cursor selection, and smoke tests not running in CI.

## Requirements

| ID | Requirement | Files |
|----|-------------|-------|
| U1 | Incremental exports must use the highest message snowflake ID, not the last array element | `scripts/run-discord-scrape.sh`, smoke fixture + assertion |
| U2 | `--cron` expressions must be validated (five fields, safe charset) before crontab install | `scripts/setup-cron.sh`, smoke test |
| U3 | Host runner and setup preflight must invoke compose without `eval` | `scripts/run-discord-scrape-host.sh`, `scripts/setup-cron.sh` |
| U4 | `DCE_REAUTH_COMMAND` must be an executable script under the repo root (no `bash -lc` arbitrary strings) | `scripts/run-discord-scrape-host.sh` |
| U5 | Recurring scrape smoke suite runs in GitHub Actions on PR/push | `.github/workflows/main.yml` |

## Implementation Units

### U1 — max message ID cursor

- Change `last_message_id` to `max_by(.id)` with empty-array guard.
- Add fixture where max ID is not the last message; extend fake CLI to log/assert `--after` value.

### U2 — cron validation

- Add `validate_cron_expression` called when `CRON_EXPRESSION` is set.
- Reject invalid field counts and shell metacharacters.

### U3 — remove eval

- Refactor `compose_run_command` to populate a bash array via nameref; execute with `"${args[@]}"`.
- Refactor `build_target_args` / `run_preflight` similarly.

### U4 — reauth script allowlist

- Resolve and require absolute path under `$REPO_ROOT`, executable file.
- Execute directly instead of `bash -lc`.

### U5 — CI smoke job

- Add `recurring-scrape-smoke` job: install `jq`, run all `scripts/tests/*-smoke.sh` scripts.

## Test Scenarios

- Smoke: unordered archive still passes `--after` with max ID.
- Smoke: invalid `--cron` exits non-zero before crontab mutation.
- CI: new job passes on clean branch.

## Out of Scope

- Changing upstream PR approval for fork workflows.
- Replacing `%q` cron job line construction (values are operator-controlled paths).

## Success Criteria

- All existing smoke scripts pass locally.
- Residual P0/P1 script findings from review are resolved or explicitly superseded by this plan.
