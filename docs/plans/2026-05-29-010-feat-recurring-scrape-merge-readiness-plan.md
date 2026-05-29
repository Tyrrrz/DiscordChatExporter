---
title: feat: Recurring scrape merge readiness
type: feat
status: completed
date: 2026-05-29
origin: LFG — PR #1538 ready; CI smoke suite missing newer Documents workflow tests
---

# feat: Recurring scrape merge readiness

## Summary

PR #1538 lands the recurring Documents scrape workflow (verify, auth bootstrap, unified operator entrypoints, GUI token discovery). Local smoke coverage exists for the newer scripts, but `.github/workflows/main.yml` still runs only the original six smoke tests. Close that gap so CI exercises the Documents operator path before merge.

---

## Problem Frame

Operators rely on `run-documents-scrape.sh`, `verify-documents-archives.sh`, and `setup-scrape-auth.sh`. Smoke tests exist for each (`documents-scrape-smoke.sh`, `verify-documents-auth-smoke.sh`) but are not wired into CI. A regression in the unified workflow could merge undetected while the legacy scrape smokes stay green.

`container-smoke.sh` requires Docker build and host archive mounts — keep it local-only for now; do not block this pass on container CI unless trivial to add.

---

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | CI `recurring-scrape-smoke` job runs `documents-scrape-smoke.sh` and `verify-documents-auth-smoke.sh` |
| R2 | `.docs/Recurring-Scrape-Setup.md` lists the full smoke suite (local + CI) consistently |
| R3 | All smoke scripts pass locally after the CI change |

---

## Scope Boundaries

- No changes to core C# exporter or merge semantics.
- No attempt to unblock upstream fork `action_required` CI (maintainer approval).
- `container-smoke.sh` stays optional/local unless Docker is already available in the job without new infrastructure.

### Deferred to Follow-Up Work

- Add `container-smoke.sh` to CI with Docker-in-GitHub-Actions setup.
- Live-token grow-only proof on production archives (operator-run, not committed).

---

## Implementation Units

### U1. Expand CI recurring-scrape-smoke job

**Goal:** Run Documents workflow smoke tests in GitHub Actions.

**Requirements:** R1

**Files:**
- Modify: `.github/workflows/main.yml`

**Approach:** Append `./scripts/tests/documents-scrape-smoke.sh` and `./scripts/tests/verify-documents-auth-smoke.sh` to the existing `Run recurring scrape smoke tests` step after chmod.

**Test scenarios:**
- Workflow YAML invokes both new scripts (grep or dry-run review).
- Local run of both scripts exits 0.

**Verification:** `bash -n` on workflow not needed; local smoke pass + workflow file contains both script paths.

---

### U2. Align operator documentation

**Goal:** Document which smokes run in CI vs locally.

**Requirements:** R2

**Dependencies:** U1

**Files:**
- Modify: `.docs/Recurring-Scrape-Setup.md`

**Approach:** Add a short "Validation" subsection listing all smoke scripts; mark which run in CI vs local-only (`container-smoke.sh`).

**Test scenarios:**
- Doc mentions `documents-scrape-smoke.sh` and `verify-documents-auth-smoke.sh` under CI coverage.

**Verification:** Manual read of updated section.

---

### U3. Run full local smoke suite

**Goal:** Confirm no regressions before push.

**Requirements:** R3

**Dependencies:** U1, U2

**Files:** (none — validation only)

**Approach:** Run every `scripts/tests/*.sh` locally; fix any failures in scope.

**Test scenarios:**
- All ten smoke scripts exit 0.

**Verification:** Single shell loop over `scripts/tests/*.sh`.
