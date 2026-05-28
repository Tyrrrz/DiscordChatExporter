---
title: fix: Verify live archive path updates
type: fix
status: completed
date: 2026-05-24
---

# fix: Verify live archive path updates

## Summary

Verify that the recurring scrape wrapper updates the user's existing `~/Documents` archives in place, then tighten destination-path fallback behavior so new unmapped channels default to stable, human-readable archive names instead of `channels/<channel-id>.json`.

This plan keeps the append-only merge contract and custom `output_dir` roots intact while closing the gap between current archive layout expectations and the wrapper's fallback naming behavior.

---

## Problem Frame

The recurring wrapper already preserves existing JSON exports when it can map a channel ID back to the right file, but the current fallback path for an unmapped channel is `output_dir/channels/<channel-id>.json`. The user's real archive set under `~/Documents` uses custom per-target roots with human-readable filenames, so this path fallback must be verified against live data and corrected so first-time channel exports do not drift into a different directory structure.

---

## Assumptions

*This plan was authored without synchronous user confirmation. The items below are agent inferences that fill gaps in the input — un-validated bets that should be reviewed before implementation proceeds.*

- The existing archive files already present under `~/Documents/<target>` are the source of truth for where future updates should land for those channels.
- When a channel has no prior archive and no stored mapping, the preferred default is an intuitive human-readable filename that still includes the channel ID for stable remapping.
- Live verification should use the current checked-in custom target config rather than introducing a second temporary config shape.

---

## Requirements

- R1. Running the recurring scraper against the current `config/scrape-targets.json` must continue to target the user's custom `~/Documents/<target>` roots and must not redirect updates into unrelated directories.
- R2. Existing archives already present under those custom roots must be updated in place through the append-only merge path rather than overwritten from scratch or duplicated into a new fallback path.
- R3. When a channel has no prior archive or stored mapping, the wrapper's default destination naming must be stable and human-readable, aligned with the CLI's guild/category/channel naming conventions instead of `channels/<channel-id>.json`.
- R4. Destination-path resolution must remain fail-closed: ambiguous matches, invalid JSON, wrong-channel archives, or paths outside the configured target root must abort without mutating the existing archive.
- R5. The behavior must be covered by fixture-based smoke tests and documented so operators understand both custom output roots and default naming for newly discovered channels.

---

## Scope Boundaries

- No change to the top-level custom target roots already configured in `config/scrape-targets.json`.
- No change to the core C# exporter append semantics; archive preservation remains wrapper-layer behavior.
- No attempt to make inaccessible Discord targets succeed with the current token; auth/access blockers remain external runtime constraints.

---

## Context & Research

### Relevant Code and Patterns

- `scripts/run-discord-scrape.sh` already enforces append-only updates via `--after`, `merge_exports`, channel identity checks, and target-local temp files.
- `scripts/run-discord-scrape.sh` persists channel-to-path mappings in `output_dir/.dce-meta/channel-map.json` and currently falls back to `output_dir/channels/<channel-id>.json` for unmapped channels.
- `scripts/tests/run-discord-scrape-smoke.sh` is the established shell-level safety test for append, dedupe, and wrong-channel no-clobber behavior.
- `config/scrape-targets.json` is the checked-in custom-root contract for the user's `~/Documents` archive tree.
- `DiscordChatExporter.Core/Exporting/ExportRequest.cs` contains the upstream CLI's human-readable default output naming logic and is the best reference for new wrapper fallback naming.
- `.docs/Docker.md` and `.docs/Scheduling-Linux.md` are the operator-facing docs for the recurring wrapper.

### Institutional Learnings

- No `docs/solutions/` directory exists in this repo.
- Existing plan and wrapper behavior intentionally keep archive safety in the shell layer and treat fail-closed preflight and path validation as load-bearing safety guarantees.

### External References

- None used. Repo-local patterns are strong enough for this fix.

---

## Key Technical Decisions

- **Treat the current custom `output_dir` values as authoritative:** updates must remain under the configured `~/Documents/<target>` roots; fixes should improve filename resolution inside those roots rather than invent a new directory layout.
- **Reuse human-readable archive names for first-write defaults:** new unmapped channels should adopt a stable guild/category/channel-based filename that still embeds the channel ID, matching the repo's existing archive style and the upstream CLI's naming conventions.
- **Preserve channel-to-path mapping as the long-term source of stability:** once a channel is resolved to a destination file, future runs should continue updating that same file regardless of later naming changes elsewhere.
- **Prove path behavior with both fixture coverage and a real runtime pass:** shell smoke tests should lock the path-resolution contract, and implementation should also run the wrapper against the real `~/Documents` config to confirm in-place updates or to surface external blockers without writing to alternate paths.

---

## Open Questions

### Resolved During Planning

- **Should the custom roots be changed?** No. The existing per-target `~/Documents/<target>` directories remain the contract.
- **What should replace the `channels/<channel-id>.json` fallback?** A human-readable default filename derived from guild/category/channel naming, with the channel ID preserved for stable remapping.
- **What is the success condition for live verification?** The run must either update the existing archive file in place or fail before creating a parallel destination path outside the expected custom root layout.

### Deferred to Implementation

- **How much live verification can succeed with the current token?** The implementer must determine this at runtime; if access is blocked, the verification outcome should still prove that no alternate path was created.
- **Which exact naming helper shape is least duplicative?** Implementation should decide whether to shell out to CLI naming-friendly metadata already present in exports or mirror the upstream naming rules directly in the wrapper after inspecting the simplest safe reuse path.

---

## Implementation Units

### U1. Fix destination-path fallback and archive seeding

**Goal:** Ensure the wrapper resolves channel destinations to the existing custom archive files when present and uses intuitive human-readable defaults for first-time channels.

**Requirements:** R1, R2, R3, R4

**Dependencies:** None

**Files:**
- Modify: `scripts/run-discord-scrape.sh`
- Modify: `config/scrape-targets.json`
- Test: `scripts/tests/run-discord-scrape-smoke.sh`

**Approach:**
- Review and tighten `resolve_destination_path()` so it first prefers persisted channel mappings, then existing archive files under the configured `output_dir`, and only then falls back to a new default path.
- Replace the current `output_dir/channels/<channel-id>.json` fallback with a stable human-readable filename that matches the archive naming style already present under `~/Documents` and the upstream CLI's default naming semantics.
- Preserve the rule that every resolved destination stays inside the configured target root and that ambiguous matches hard-fail instead of guessing.

**Patterns to follow:**
- `scripts/run-discord-scrape.sh`
- `DiscordChatExporter.Core/Exporting/ExportRequest.cs`
- `config/scrape-targets.json`

**Test scenarios:**
- Happy path: an existing archive with a human-readable filename containing `[channel-id]` is discovered, mapped, and reused for the update.
- Happy path: an unmapped first-time channel resolves to a human-readable filename under the target root and records that mapping for later runs.
- Edge case: a target root containing multiple matching files for the same channel ID fails closed and does not guess.
- Error path: a mapped path outside the configured target root is rejected before export.
- Integration: a rerun after the new mapping is written updates the exact same file path rather than creating a second archive path.

**Verification:**
- Unmapped channels no longer default to `channels/<channel-id>.json`.
- Existing archives under the configured custom roots continue to resolve back to their current file paths.

---

### U2. Expand append-only and path-safety smoke coverage

**Goal:** Add fixture coverage that proves path resolution and append-only updates do not create parallel archives or overwrite unrelated files.

**Requirements:** R2, R3, R4, R5

**Dependencies:** U1

**Files:**
- Modify: `scripts/tests/run-discord-scrape-smoke.sh`
- Create: `scripts/tests/test-fixtures/path-existing.json`
- Create: `scripts/tests/test-fixtures/path-incremental.json`

**Approach:**
- Extend the existing smoke script with a case where a preexisting human-readable archive under a target root has no stored map yet and must still be updated in place.
- Add coverage for the first-run fallback path so the test can assert both the filename pattern and that the file lands directly under the configured target root.
- Keep the current wrong-channel and invalid-json hard-fail expectations as the guardrail against archive corruption.

**Execution note:** Start by adding/adjusting fixture coverage before changing the fallback logic so the path regression is pinned by tests.

**Patterns to follow:**
- `scripts/tests/run-discord-scrape-smoke.sh`
- `scripts/tests/test-fixtures/append-existing.json`
- `scripts/tests/test-fixtures/append-incremental.json`
- `scripts/tests/test-fixtures/wrong-channel.json`

**Test scenarios:**
- Happy path: an existing human-readable archive with no prior `channel-map.json` entry is updated in place and retains prior messages.
- Happy path: a first-time export creates one human-readable file directly under the target root and writes a matching channel-map entry.
- Edge case: an incremental export with zero new messages leaves the existing human-readable archive untouched and does not create a second file.
- Error path: wrong-channel incremental data fails without replacing the existing archive or writing a new fallback file.
- Integration: two consecutive runs against the same channel keep the same destination path and merge by message ID.

**Verification:**
- Smoke coverage fails on the old `channels/<channel-id>.json` fallback and passes with the new default naming behavior.
- Existing append-only protections still pass after the path-resolution changes.

---

### U3. Run real-config verification and document the contract

**Goal:** Validate the fixed wrapper against the user's checked-in `~/Documents` targets and document how custom roots and default naming interact.

**Requirements:** R1, R2, R5

**Dependencies:** U1, U2

**Files:**
- Modify: `.docs/Docker.md`
- Modify: `.docs/Scheduling-Linux.md`
- Modify: `STRATEGY.md`

**Approach:**
- Run the wrapper with the real checked-in target config and current token/runtime setup to confirm that accessible channels update their existing archive paths in place and that blocked targets fail without creating alternate destination paths.
- Capture the operator contract in docs: custom `output_dir` roots remain authoritative, existing archives are reused, and first-time channels adopt human-readable defaults inside the target root.
- Refresh strategy/docs language only where needed so the runtime promise matches the implemented destination behavior.

**Patterns to follow:**
- `.docs/Docker.md`
- `.docs/Scheduling-Linux.md`
- `STRATEGY.md`

**Test scenarios:**
- Test expectation: none -- this unit is runtime verification and documentation work backed by the fixture coverage in U2.

**Verification:**
- A real wrapper run against the configured `~/Documents` targets either updates an existing archive path in place or fails before creating a conflicting path.
- Operator docs explain both custom-root behavior and the default naming used for new channels within those roots.

---

## System-Wide Impact

- **Interaction graph:** destination-path changes affect archive seeding, channel-map persistence, append-only merge flow, Docker runtime expectations, and cron-driven recurring runs.
- **Error propagation:** path ambiguity or invalid archive state must still abort the affected channel/target before any destination replacement.
- **State lifecycle risks:** incorrect fallback naming could silently split one logical channel across two files; the plan explicitly tests and prevents that.
- **API surface parity:** the user-facing contract spans the shell wrapper, checked-in target config, and operator docs, so all three need to stay aligned.
- **Integration coverage:** fixture tests cover deterministic path and merge semantics; runtime verification covers interaction with the real `~/Documents` archive tree and current token access.

---

## Risks & Dependencies

| Risk | Mitigation |
|------|------------|
| Live token access is still blocked for some targets | Treat runtime verification as success only when it proves no alternate path was created; document blocked targets explicitly rather than guessing |
| New filename generation diverges from existing archive style | Follow the upstream CLI naming reference and add smoke coverage for first-write defaults |
| Path-fix logic accidentally stops recognizing existing archives | Extend seeded-archive tests before changing fallback behavior and keep channel-map persistence authoritative |

---

## Documentation / Operational Notes

- Keep the docs explicit that custom `output_dir` targets remain authoritative and that new default naming only applies within those roots when no prior archive path exists.
- Runtime verification should be performed through the source-built wrapper path, not the downloaded binary bundle, so the docs and behavior stay aligned.

---

## Sources & References

- Strategy: `STRATEGY.md`
- Prior plan: `docs/plans/2026-05-24-001-feat-recurring-cli-scrape-automation-plan.md`
- Related code: `scripts/run-discord-scrape.sh`
- Related tests: `scripts/tests/run-discord-scrape-smoke.sh`
- Related config: `config/scrape-targets.json`
- Naming reference: `DiscordChatExporter.Core/Exporting/ExportRequest.cs`
- Operator docs: `.docs/Docker.md`, `.docs/Scheduling-Linux.md`
