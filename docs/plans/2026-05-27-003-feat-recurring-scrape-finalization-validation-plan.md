---
title: feat: Finalize and validate recurring Discord scrape automation
type: feat
status: completed
date: 2026-05-27
---

# feat: Finalize and validate recurring Discord scrape automation

## Summary

The feat/recurring-cli-scrape branch has implemented the core recurring scraper infrastructure (scripts, Docker build, cron setup, smoke tests, and fixtures). This plan focuses on **comprehensive validation and production hardening**: verifying append-only safety end-to-end, testing all failure paths, ensuring documentation completeness, validating cron idempotency under stress, and creating a deployment readiness checklist.

The implementation stays in the wrapper/script layer and does not require changes to the core C# exporter. The validation approach is practical and executable: smoke-test suite coverage, edge-case scenario validation, cross-environment testing, and live iteration proofs.

---

## Problem Frame

The recurring scraper is feature-complete but requires production-hardening before it can be trusted with real token + existing archive roots. The hard part is gaining confidence that:
- Append-only merge logic preserves existing history under all conditions (including partial failures, interrupted runs, conflicting local state)
- Error handling fails closed consistently across auth, config, target resolution, and archive-safety boundaries
- The cron installation mechanism stays idempotent across repeated setup runs with evolving target configurations
- Operator-facing documentation aligns with actual behavior, with clear setup, troubleshooting, and recovery paths
- The preflight validation path covers every safety requirement before unattended runs

---

## Assumptions

*This plan builds from the existing implementation, test fixtures, and smoke-test scaffolding already on the feat/recurring-cli-scrape branch. The items below represent validation-focused bets that should be confirmed during execution.*

- The scripts run-discord-scrape.sh, setup-cron.sh, and run-discord-scrape-host.sh are the authoritative recurring-scraper implementations; the CLI project itself is unchanged.
- Smoke tests are the primary validation vehicle; formal integration tests are deferred to a future repo test suite if it emerges.
- The append-only merge logic in run-discord-scrape.sh is the critical data-safety contract and warrants the deepest validation coverage.
- Host cron remains the scheduler of record and the focus for idempotency and lock validation.
- README.md will be updated to surface the recurring-scraper capability at the repo's entry point, not buried in sub-documentation.
- Preflight validation is run-time-sufficient rather than compile-time-guaranteed; the shell layer cannot prove static correctness, only demonstrate runtime success.

---

## Requirements

- R1. All append-only merge scenarios in the existing fixtures (append-existing.json, append-incremental.json, wrong-channel.json) pass automated validation with clear pass/fail signals.
- R2. Error handling paths cover: missing token, invalid config, unresolvable targets, mismatched channel identity, missing preflight, and failed docker operations—each tested with expected failure messages and no silent data loss.
- R3. Cron installation mechanism stays idempotent across repeated setup runs with different schedule and target selections; existing unrelated crontab entries are preserved.
- R4. Preflight validation exercises the full runtime path (source-built container startup, authenticated discovery, config/token visibility) and produces clear pass/fail output before cron is installed.
- R5. Documentation (README.md, .docs/Docker.md, .docs/Scheduling-Linux.md) describes the operator contract accurately: supported config keys, safety guarantees, failure modes, and recovery procedures.
- R6. Smoke-test suite runs reliably in CI and local environments; test fixtures remain deterministic and do not depend on external state (real Discord tokens, live servers, etc.).
- R7. The host-retry auth flow (added in commit 090884f) is validated: retry behavior is predictable, error messages are clear, and the retry logic does not mask underlying token/auth issues.

---

## Scope Boundaries

- **Implementation is frozen** on this plan; only validation, documentation updates, and smoke-test enhancements are in scope. No new features or architectural changes.
- No performance optimization or refactoring of script logic unless it directly supports a validation goal.
- No changes to the core C# exporter or CLI project; the wrapper layer remains the only target.
- No cross-platform scheduler support beyond the existing Linux cron focus; macOS/Windows scheduling deferred.

### Deferred to Follow-Up Work

- Full integration test suite in the repo's existing test infrastructure (if one emerges).
- Performance profiling or optimization of incremental export and merge logic.
- Cross-platform scheduler parity (Windows Task Scheduler, macOS launchd).
- Rehydrating edited messages or reactions on already-archived history.

---

## Context & Research

### Relevant Code and Patterns

- `scripts/run-discord-scrape.sh` — Core append-only merge and error handling logic.
- `scripts/setup-cron.sh` — Cron installation, idempotency, and preflight orchestration.
- `scripts/run-discord-scrape-host.sh` — Host-side lock and cron invocation wrapper.
- `scripts/tests/` — Existing smoke-test suite (container-smoke.sh, run-discord-scrape-smoke.sh, setup-cron-smoke.sh, run-discord-scrape-host-smoke.sh).
- `scripts/tests/test-fixtures/` — Fixture JSON files for append/merge validation.
- `config/scrape-targets.json` — Target configuration with guild_ids, channel_ids, output_dir, and schedule.
- `Dockerfile` and `docker-compose.yml` — Source-built container and compose configuration.
- `STRATEGY.md` — Product-level goals and tracks for the recurring scraper.
- `.docs/Docker.md` and `.docs/Scheduling-Linux.md` — Existing operator documentation (to be reviewed and updated).

### Institutional Learnings

- No prior institutional learnings found; this is a first-time recurring-scraper implementation.

### External References

- Bash best practices: error handling, set -e, trap handlers, fd locking.
- Docker build and compose best practices from existing repo patterns.
- cron idempotency patterns from Linux sysadmin practice.

---

## Key Technical Decisions

- **Validation-first approach**: Smoke tests and fixtures are the validation vehicle rather than formal unit tests; this keeps the barrier low for shell-based integration work.
- **Append-only safety is non-negotiable**: Every merge scenario in the fixtures must pass, and new edge cases discovered during validation trigger fixture additions.
- **Fail-closed by default**: Ambiguous or unsafe state stops the affected target and never silently overwrites archives; error messages are explicit about why.
- **Idempotency is enforced at the cron layer**: Repeated setup runs should converge to a stable state; this is testable with fixture crontabs.
- **Documentation drives trust**: README.md and .docs/ materials are updated to reflect actual behavior; discrepancies are resolved by updating implementation, not documentation.
- **Host cron is the authority**: The recurring workflow does not attempt to override host timezone, scheduling, or lock semantics; all of those are host responsibilities.

---

## Open Questions

### Resolved During Planning

- **What level of validation is sufficient before declaring the feature production-ready?** Pass all smoke tests, cover error paths, validate end-to-end preflight, update documentation.
- **Should new merge-logic edge cases discovered during validation add to the fixture set or remain one-off test runs?** Add to fixtures so they're part of the permanent regression suite.

### Deferred to Implementation

- **How should the smoke-test suite be invoked in CI/CD?** The implementer should decide whether to wire the tests into an existing repo test runner or keep them as standalone scripts for now.
- **Should the host-retry auth flow be validated with a real Discord token or purely with mocked responses?** Implementer choice; mocked responses are sufficient for validation, but real-token testing may catch subtle timeout/retry edge cases.

---

## High-Level Technical Design

> *This illustrates the intended validation approach and is directional guidance for review, not implementation specification. The implementing agent should treat it as context, not code to reproduce.*

### Validation Flow

```
┌─────────────────────────────────────────────────────────────┐
│ Validation Checklist (All items must pass before release)   │
├─────────────────────────────────────────────────────────────┤
│ 1. Append-Only Merge Validation                             │
│    ├─ All fixtures pass (append-existing, incremental, etc) │
│    ├─ Edge case: partial write + retry = correct merge      │
│    └─ Edge case: concurrent appends don't corrupt           │
│ 2. Error Handling Validation                                │
│    ├─ Missing token → clear error, no archive touch        │
│    ├─ Invalid config → setup stops before cron install     │
│    ├─ Unresolvable target → logs and continues next target │
│    └─ Channel mismatch → archive preserved, target skipped  │
│ 3. Cron Idempotency Validation                              │
│    ├─ Install, then reinstall → one managed block only      │
│    ├─ Update schedule → only managed block changes          │
│    └─ Remove → managed block gone, other entries survive    │
│ 4. Preflight Validation                                     │
│    ├─ Container builds from source                          │
│    ├─ Auth layer is reachable with token                    │
│    ├─ Config discovery works                                │
│    └─ Lock mechanism is functional                          │
│ 5. Documentation Validation                                 │
│    ├─ README.md mentions recurring-scraper capability       │
│    ├─ Setup instructions are clear and complete             │
│    ├─ Error modes are documented                            │
│    └─ Recovery procedures are provided                      │
│ 6. Smoke Test Reliability Validation                        │
│    ├─ All tests pass locally                                │
│    ├─ Tests pass in CI (if integrated)                      │
│    ├─ Tests are deterministic (no timing/state issues)      │
│    └─ Fixtures are self-contained (no external deps)        │
└─────────────────────────────────────────────────────────────┘
```

---

## Implementation Units

### U1. Deepen append-only merge test coverage

**Goal:** Validate that the merge logic preserves existing local history under all plausible edge cases and failure scenarios.

**Requirements:** R1, R6

**Dependencies:** None

**Files:**
- Modify: `scripts/tests/run-discord-scrape-smoke.sh`
- Modify: `scripts/tests/test-fixtures/append-existing.json`
- Create: `scripts/tests/test-fixtures/append-partial-write.json`
- Create: `scripts/tests/test-fixtures/append-concurrent-conflict.json`
- Create: `scripts/tests/validation-checklist.md`

**Approach:**
- Review the existing append-only merge logic in run-discord-scrape.sh and identify all paths where data could be lost or corrupted.
- Enhance the smoke-test suite with additional fixture scenarios: partial writes interrupted mid-merge, concurrent export attempts, timestamp edge cases, empty incremental exports.
- Add validation assertions to confirm that existing JSON structure and message count are preserved after each merge scenario.
- Document the test scenarios clearly so operators understand what safety guarantees they have.

**Execution note:** Start by running the existing fixtures and understanding the current merge logic flow, then identify edge cases and add fixture scenarios.

**Patterns to follow:**
- `scripts/tests/run-discord-scrape-smoke.sh` — existing test structure
- `scripts/tests/test-fixtures/append-*.json` — fixture naming and structure
- `scripts/run-discord-scrape.sh` — merge logic implementation to understand

**Test scenarios:**
- Happy path: existing archive + incremental new messages = merged archive with all messages, sorted by ID.
- Happy path: first export creates a new archive with correct structure and metadata.
- Edge case: incremental export with zero new messages leaves the existing archive unchanged (byte-for-byte).
- Edge case: overlapping message IDs between existing and incremental are deduplicated.
- Edge case: missing incremental file after export attempt leaves the existing archive unchanged.
- Error path: corrupted destination JSON fails that target without attempting merge.
- Error path: channel metadata mismatch (guildId, channelId mismatch) aborts merge and preserves existing archive.
- Integration: a fixture that removes older messages from the incremental export still produces a merged archive with original history intact.
- Integration: repeated merges of the same incremental file (simulating a retry) produce identical results (idempotent).

**Verification:**
- All fixture scenarios pass and produce deterministic, reproducible results.
- Error paths produce explicit failure messages and never silently replace archives.
- Smoke-test output clearly signals pass/fail for each scenario.

---

### U2. Validate error handling across all failure modes

**Goal:** Ensure that the recurring scraper fails safely and clearly when token is missing, config is invalid, targets cannot be resolved, or archive state is ambiguous.

**Requirements:** R2, R4

**Dependencies:** None

**Files:**
- Create: `scripts/tests/error-path-smoke.sh`
- Create: `scripts/tests/test-configs/invalid-output-dir.json`
- Create: `scripts/tests/test-configs/missing-guild.json`
- Create: `scripts/tests/test-configs/duplicate-output-dir.json`
- Modify: `scripts/tests/validation-checklist.md`

**Approach:**
- Map all error conditions from the plan (missing token, invalid config, unresolvable target, channel mismatch, etc.).
- Write a dedicated error-path smoke test that exercises each condition with expected failure messages.
- Verify that each error condition stops the affected target without silencing other targets or mutating crontab.
- Document the expected error messages so operators can troubleshoot.

**Patterns to follow:**
- `scripts/run-discord-scrape.sh` — error handling patterns (set -e, trap handlers, explicit error messages)
- `scripts/tests/run-discord-scrape-smoke.sh` — test structure for validation

**Test scenarios:**
- Error path: missing DISCORD_TOKEN env variable → setup fails with clear message before cron install.
- Error path: invalid output_dir (outside approved root) → config validation rejects it before any export.
- Error path: duplicate output_dir across targets → validation fails before setup.
- Error path: guild_id not found or not accessible → target is skipped with a clear log message.
- Error path: channel mismatch in existing archive → that target fails without archive replacement.
- Error path: docker compose build fails → setup stops before cron install.
- Error path: host lock already held (another run in progress) → cron command logs and exits gracefully.

**Verification:**
- Each error condition produces a clear, actionable error message.
- No silent data loss or archive corruption occurs.
- Unrelated targets are not affected by a single target's failure.

---

### U3. Test cron idempotency and lifecycle management

**Goal:** Verify that the cron installation mechanism stays stable and idempotent across repeated setup runs, schedule changes, and removals.

**Requirements:** R3, R4

**Dependencies:** None

**Files:**
- Create: `scripts/tests/cron-idempotency-smoke.sh`
- Create: `scripts/tests/test-crontabs/fixture-with-unrelated-entries.txt`
- Modify: `scripts/tests/validation-checklist.md`

**Approach:**
- Create a smoke test that exercises the full cron lifecycle: install, reinstall with new schedule, update targets, remove.
- Use fixture crontabs (text files representing a pre-existing user's crontab) to ensure unrelated entries are preserved.
- Verify that setup converges to a single managed block and is safe to re-run.
- Test the `--dry-run` and `--remove` paths to ensure they work as expected.

**Patterns to follow:**
- `scripts/setup-cron.sh` — cron lifecycle implementation
- Existing cron testing patterns in the branch

**Test scenarios:**
- Happy path: initial install creates one managed cron block with monthly default schedule.
- Happy path: rerunning setup with same config produces no changes (idempotent).
- Happy path: rerunning with new schedule replaces only the managed block and preserves unrelated entries.
- Happy path: `--dry-run` shows the intended managed block without touching the live crontab.
- Happy path: `--remove` deletes only the managed block and leaves unrelated entries intact.
- Edge case: pre-existing fixture crontab with many unrelated entries survives a full lifecycle (install → update → remove).
- Error path: failed preflight leaves crontab untouched.

**Verification:**
- Cron installation mechanism converges to a stable, idempotent state.
- Unrelated crontab entries are always preserved.
- Dry-run and remove operations work as expected.

---

### U4. Validate preflight and end-to-end setup path

**Goal:** Ensure the preflight validation covers all runtime requirements and proves the recurring scraper is ready before cron is installed.

**Requirements:** R4, R5, R7

**Dependencies:** U1, U2, U3

**Files:**
- Create: `scripts/tests/end-to-end-preflight-smoke.sh`
- Modify: `.docs/Scheduling-Linux.md` — preflight section
- Modify: `scripts/tests/validation-checklist.md`

**Approach:**
- Design and execute a smoke test that runs the full preflight path: container build, config visibility, auth token validation, discovery success.
- Verify that a successful preflight leads to cron install and a failed preflight leaves crontab untouched.
- Document the preflight path clearly for operators so they understand what's being validated.
- Test the host-retry auth flow (commit 090884f) to ensure retries are predictable and don't mask real auth failures.

**Patterns to follow:**
- `scripts/setup-cron.sh` — preflight orchestration
- `scripts/tests/container-smoke.sh` — container validation patterns

**Test scenarios:**
- Happy path: preflight succeeds with valid token and config → cron install proceeds.
- Happy path: preflight shows accessible targets and estimated schedule clearly.
- Error path: missing DISCORD_TOKEN → preflight fails before cron install.
- Error path: docker build fails → setup stops before cron install.
- Error path: config not visible or invalid → setup stops before cron install.
- Integration: full lifecycle (preflight → install → dry-run → remove) succeeds end-to-end.

**Verification:**
- Preflight validation is comprehensive and covers all safety requirements.
- Failed preflight prevents cron installation.
- Successful preflight gives operators clear confidence in the runtime setup.

---

### U5. Complete and align documentation with implementation

**Goal:** Ensure README.md and .docs/ materials describe the operator contract accurately: setup, configuration, failure modes, and recovery procedures.

**Requirements:** R5, R6

**Dependencies:** U1, U2, U3, U4

**Files:**
- Modify: `Readme.md`
- Modify: `.docs/Docker.md`
- Modify: `.docs/Scheduling-Linux.md`
- Create: `.docs/Recurring-Scrape-Setup.md`
- Create: `.docs/Recurring-Scrape-Troubleshooting.md`

**Approach:**
- Add a high-level section to README.md that mentions the recurring-scraper capability and links to detailed setup docs.
- Review .docs/Docker.md and .docs/Scheduling-Linux.md for accuracy against the current implementation; update descriptions, examples, and error messages to match behavior.
- Create two new documents: a quick-start setup guide (Recurring-Scrape-Setup.md) and a troubleshooting guide (Recurring-Scrape-Troubleshooting.md).
- Ensure all documented flags, defaults, and safety constraints match the implemented behavior.

**Patterns to follow:**
- `.docs/Docker.md` and `.docs/Scheduling-Linux.md` — existing documentation style and structure
- Readme.md — high-level feature descriptions

**Test scenarios:**
- Test expectation: none -- documentation-only unit. Review should confirm that documented flags, examples, and safety guarantees match the implemented behavior.

**Verification:**
- README.md surfaces the recurring-scraper feature prominently.
- .docs/Recurring-Scrape-Setup.md provides clear, step-by-step instructions for first-time setup.
- .docs/Recurring-Scrape-Troubleshooting.md covers the most common failure modes and recovery steps.
- All documented error messages, defaults, and config keys match the implementation.
- External readers can set up the recurring scraper from the documentation without needing to reverse-engineer the scripts.

---

### U6. Create production-readiness checklist and sign-off

**Goal:** Produce a clear, verifiable checklist that confirms the feature is production-ready for release.

**Requirements:** R1-R7

**Dependencies:** U1, U2, U3, U4, U5

**Files:**
- Create: `docs/recurring-scrape-production-checklist.md`
- Modify: `docs/plans/2026-05-27-003-feat-recurring-scrape-finalization-validation-plan.md` — add final sign-off section

**Approach:**
- Compile all validation results (smoke-test pass rates, edge-case coverage, error-handling validation, idempotency proof, documentation alignment) into a single production-readiness checklist.
- Include specific test commands and expected outcomes so future reviewers or maintainers can re-validate if needed.
- Document any known limitations or deferred follow-up work.
- Provide clear sign-off criteria: all tests pass, all error paths verified, all documentation updated and reviewed.

**Patterns to follow:**
- Existing validation-checklist.md sections from U1-U5

**Test scenarios:**
- Test expectation: none -- summary/attestation document. Review should confirm all prior units' validation results are captured and organized.

**Verification:**
- The checklist is comprehensive, specific, and verifiable.
- Future maintainers can reproduce the validation by following the checklist.
- Sign-off criteria are clear and leave no ambiguity about readiness.

---

## System-Wide Impact

- **Interaction graph:** Host cron, Docker Compose, wrapper scripts, CLI, and local archives form a tightly coupled system; validation must exercise the full stack.
- **Error propagation:** Config/setup failures stop before cron mutation; target-level failures stop that target without affecting others; clear error messages guide operator troubleshooting.
- **State lifecycle risks:** Fixture crontabs, temporary merge files, and existing archives must remain coherent across repeated validation runs and interruptions.
- **Integration coverage:** Smoke tests validate source-built container, authenticated discovery, append-only merge, cron idempotency, and preflight path—all together, not in isolation.
- **Documentation parity:** Operator docs must match implementation; discrepancies are resolved by updating implementation, not softening documentation claims.
- **Unchanged invariants:** The upstream CLI remains the exporter of record; this plan does not modify core C# behavior, only validates the wrapper layer's safety.

---

## Risks & Dependencies

| Risk | Mitigation |
|------|-----------|
| Append-only merge logic still has unidentified edge cases | Deepen fixture coverage (U1); add edge cases discovered during validation to permanent fixture set |
| Error messages are unclear or missing, leading to operator confusion | Validate all error paths (U2); review error messages for clarity and actionability |
| Cron installation drifts and produces duplicate blocks after repeated setup runs | Test idempotency thoroughly with fixture crontabs (U3); verify managed-block markers are stable |
| Preflight validation passes but runtime fails, leaving cron in broken state | Run end-to-end smoke test that covers full lifecycle (U4); test host-retry auth flow for robustness |
| Documentation describes old behavior or missing config keys | Review docs against implementation (U5); cross-check with actual script output and error messages |
| Smoke tests are unreliable or time-sensitive, causing false failures in CI | Keep fixtures deterministic and self-contained (U6); avoid real Discord tokens or external dependencies |

---

## Documentation Plan

- **README.md** — Add recurring-scraper overview and link to detailed docs.
- **.docs/Recurring-Scrape-Setup.md** — Step-by-step first-time setup guide.
- **.docs/Recurring-Scrape-Troubleshooting.md** — Common issues and recovery steps.
- **.docs/Docker.md** and **.docs/Scheduling-Linux.md** — Update for accuracy and alignment with implementation.
- **docs/recurring-scrape-production-checklist.md** — Final validation results and readiness sign-off.

---

## Operational & Rollout Notes

- The recurring scraper requires explicit operator action to install (via setup-cron.sh); no automatic deployment or background updates.
- Host cron is the scheduler of record; the operator owns the schedule, retention, and log rotation.
- The preflight validation path is designed to be safe for operators to run with real tokens and existing archives before committing to cron.
- Recovery from a failed run is manual (inspect logs, fix config, re-run setup or individual target exports).

---

## Sources & References

- Related code: `scripts/run-discord-scrape.sh`
- Related code: `scripts/setup-cron.sh`
- Related code: `scripts/run-discord-scrape-host.sh`
- Related code: `scripts/tests/` (smoke-test suite and fixtures)
- Related code: `Dockerfile` and `docker-compose.yml`
- Related docs: `STRATEGY.md`
- Related docs: `.docs/Docker.md`, `.docs/Scheduling-Linux.md`
- Existing plan: `docs/plans/2026-05-24-001-feat-recurring-cli-scrape-automation-plan.md`
