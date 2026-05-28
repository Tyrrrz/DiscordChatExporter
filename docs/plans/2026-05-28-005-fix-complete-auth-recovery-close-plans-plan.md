---
title: fix: Complete auth recovery and close recurring scrape plans
type: fix
status: completed
date: 2026-05-28
origin: Active plans 2026-05-24-* with remaining U2 from auth-reauth plan
---

# fix: Complete auth recovery and close recurring scrape plans

## Summary

The recurring scrape feature branch is functionally complete after validation (003) and hardening (004). One implementation unit remains from `docs/plans/2026-05-24-001-fix-auth-reauth-recovery-plan.md` (U2: GitHub approval helper), and several sibling plans should be marked completed to reflect landed work.

## Problem Frame

Cross-repo PRs to `Tyrrrz/DiscordChatExporter` can block on GitHub Actions approval. Operators need an explicit, fail-closed helper that bootstraps `gh` from `GITHUB_TOKEN` and attempts run approval while surfacing admin-rights policy blockers separately from transient auth failures.

## Requirements

| ID | Requirement | Files |
|----|-------------|-------|
| U1 | Add `scripts/gh-approve-pr-runs.sh` with token bootstrap, run approval attempts, and explicit 403 admin-rights classification | new script, smoke test |
| U2 | Document the helper in operator docs | `.docs/Scheduling-Linux.md`, `.docs/Docker.md` |
| U3 | Mark completed 2026-05-24 active plans as `status: completed` | `docs/plans/2026-05-24-*.md` |

## Out of Scope

- Circumventing upstream admin approval policy
- Core C# or archive-path changes (already landed)

## Test Scenarios

- Missing `GITHUB_TOKEN` → clear error, exit non-zero
- Valid token + mock `gh` → approval API invoked for each run ID
- Mock `gh` returning admin-rights 403 → explicit policy blocker message

## Success Criteria

- Smoke test passes
- All recurring-scrape plans through 004 marked completed
- PR #1538 documents gh-approve usage for fork CI unblock attempts
