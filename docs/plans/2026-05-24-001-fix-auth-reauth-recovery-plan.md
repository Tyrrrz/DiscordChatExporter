---
date: 2026-05-24
sequence: 001
plan_type: fix
title: Harden GitHub and Discord reauth recovery
status: completed
---

# fix: Harden GitHub and Discord reauth recovery

## Summary

Ensure this workflow can recover from expired/invalid auth context instead of stopping at blockers:
1) persist and verify GitHub CLI auth from `GITHUB_TOKEN` in `~/.bashrc`,
2) add a durable Discord token refresh/reauth path for recurring scrape runs,
3) document and test the new non-destructive recovery behavior.

---

## Problem Frame

Current execution fails hard on two recurring auth conditions:
- GitHub Actions approval for cross-repo PR checks can be attempted but must fail closed when repository-admin rights are unavailable.
- Discord scrape/preflight failures (`401`/`403`) currently stop the run without an explicit automated token reload + optional interactive reauth path.

The plan focuses on making those outcomes explicit, recoverable, and idempotent without changing append-only archive safety.

---

## Scope Boundaries

### In Scope
- Add a host-side auth-aware runner used by cron that can reload Discord token and retry once on auth failure.
- Add clear failure classification for GitHub approval attempts (permission/policy blockers vs transient CLI auth issues).
- Preserve existing append-only path guarantees and configured archive roots.
- Update docs/env examples and smoke tests for the new auth flow.

### Out of Scope
- Circumventing Discord access policies or bypassing permissions for channels/accounts.
- Forcing upstream repository admin approvals when the authenticated GitHub user lacks required rights.

### Deferred to Follow-Up Work
- Optional long-lived secure token broker/secret-store integration beyond env/file-based token refresh.

---

## Key Technical Decisions

- Use a **host-side wrapper script** for scheduled runs rather than embedding reauth logic only inside container runtime; this is the only place that can safely source `~/.bashrc`, invoke `gh`, and coordinate interactive browser auth when manually triggered.
- Treat Discord auth recovery as a **single bounded retry**: reload token source -> retry preflight/scrape once -> fail with explicit reason. Avoid infinite loops or silent retries.
- Keep GitHub approval behavior **truthful and explicit**: attempt via `gh api`, classify 403 admin-rights response as unresolved upstream permission blocker, and record durable status.

---

## Implementation Units

### U1. Add auth-aware host runner for recurring scrapes
**Goal:** Provide a single entrypoint cron/manual runs can call that handles Discord token reload and bounded retry behavior.

**Requirements:** Recoverable auth flow; idempotent scheduling behavior; preserve existing archive update semantics.

**Dependencies:** None.

**Files:**
- `scripts/run-discord-scrape-host.sh` (new)
- `scripts/setup-cron.sh`
- `docker-compose.yml`

**Approach:**
- Create a host runner that:
  - sources configured env file and optional token file,
  - calls compose preflight/scrape,
  - detects Discord auth failures from wrapper output,
  - triggers one token refresh path (`DISCORD_TOKEN_FILE` reread and optional reauth command),
  - retries once and exits non-zero with explicit reason if still blocked.
- Update cron job line to execute the host runner instead of raw `docker compose run ... scrape`.

**Patterns to follow:** Existing strict error handling and fail-closed style in `scripts/run-discord-scrape.sh` and `scripts/setup-cron.sh`.

**Test scenarios:**
- Happy path: valid token runs scrape once, no retry path invoked.
- Edge: missing token file while configured triggers explicit failure before scrape.
- Error path: first scrape returns auth failure, refreshed token succeeds on retry.
- Error path: auth failure persists after retry -> hard fail without data-path mutation.
- Integration: cron-generated command uses host runner and preserves target overrides.

**Verification:** Cron-managed runs execute through the new runner and show deterministic retry/failure logs.

### U2. Make GitHub auth/approval handling explicit and durable
**Goal:** Ensure GitHub auth bootstrap and approval attempts are standardized and clear about resolvable vs policy blockers.

**Requirements:** Reauth from `~/.bashrc` via `gh`; explicit classification for approval failures.

**Dependencies:** U1 not required.

**Files:**
- `scripts/gh-approve-pr-runs.sh` (new)
- `.docs/Docker.md`
- `.docs/Scheduling-Linux.md`

**Approach:**
- Add a helper script that:
  - sources `~/.bashrc`, validates `GITHUB_TOKEN`, performs non-interactive `gh auth login --with-token` if needed,
  - attempts approval endpoints for provided run IDs,
  - maps known API responses (e.g., `Must have admin rights`) to explicit unresolved-policy output and non-zero exit.
- Document expected outcomes so future runs do not misclassify policy blockers as transient auth failures.

**Patterns to follow:** Existing CLI-first operations and explicit error messages.

**Test scenarios:**
- Happy path: token present and `gh auth status` valid.
- Error path: missing `GITHUB_TOKEN` yields clear actionable failure.
- Error path: approval 403 admin-rights response is surfaced as upstream-policy blocker.

**Verification:** Script output distinguishes auth misconfiguration from insufficient repository permission.

### U3. Extend tests and docs for reauth and scheduling behavior
**Goal:** Keep regression coverage and operator docs aligned with the new auth-recovery slice.

**Requirements:** Vertical-slice parity across scripts/tests/docs.

**Dependencies:** U1, U2.

**Files:**
- `scripts/tests/setup-cron-smoke.sh`
- `scripts/tests/run-discord-scrape-smoke.sh`
- `.docs/Scheduling-Linux.md`
- `.docs/Docker.md`
- `scrape.env.example`

**Approach:**
- Add smoke coverage for cron line changes and host-runner invocation.
- Add smoke fixtures/modes for first-fail auth then successful retry and persistent auth failure.
- Document env knobs (`DISCORD_TOKEN_FILE`, optional reauth command) and operational expectations for non-interactive cron vs interactive manual recovery.

**Patterns to follow:** Existing smoke test style and doc conventions already used for recurring wrapper features.

**Test scenarios:**
- Happy path: cron setup remains idempotent with managed block replacement.
- Edge: dry-run preview includes host runner command and no crontab mutation.
- Error path: simulated auth failure triggers single retry only.
- Integration: docs/env example reflect actual script options and defaults.

**Verification:** Existing smoke suite passes with new auth cases and docs match runtime behavior.

---

## Risks and Mitigations

- **Risk:** Retry logic could accidentally mutate paths or overwrite archives.
  - **Mitigation:** Keep all archive merge/path logic in existing wrapper; host runner only orchestrates retries.
- **Risk:** Interactive reauth flow unusable in cron context.
  - **Mitigation:** Split non-interactive token-file refresh (cron-safe) from optional manual interactive reauth command.
- **Risk:** Users assume GitHub approvals are always automatable.
  - **Mitigation:** Explicitly document and emit admin-rights prerequisite when API returns policy 403.

---

## System-Wide Impact

- Scheduler path changes from direct compose invocation to host runner orchestration.
- Operator setup adds token-file/reauth options but keeps current defaults valid.
- No change to archive file format, append merge semantics, or configured root mappings.

---

## Deferred Implementation Unknowns

- Final naming of environment variables and helper script CLI flags may adjust for consistency with existing `DCE_*` naming.
- Exact stderr matching strategy for Discord auth failures may need to key off stable wrapper messages rather than raw upstream text.
