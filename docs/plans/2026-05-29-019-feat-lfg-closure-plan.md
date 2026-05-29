---
title: feat: LFG closure — prove smoke and workspace bridge
type: feat
status: complete
date: 2026-05-29
origin: Repeated /lfg — recurring scrape stack complete; close gaps for operators and CI
---

# feat: LFG closure — prove smoke and workspace bridge

## Summary

Recurring scrape is feature-complete on `feat/recurring-cli-scrape`. This slice adds an offline prove smoke test, documents audit/salvage in the GUI zip bridge, and refreshes the open PR summary.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `prove-incremental-append.sh` supports `--snapshot-only` for offline verification |
| R2 | `scripts/tests/prove-incremental-append-smoke.sh` validates invalid JSON skip + grow-only compare |
| R3 | CI `recurring-scrape-smoke` job runs prove smoke |
| R4 | `DiscordChatExporter.linux-x64/RECURRING-SCRAPE.md` mentions audit/salvage |
| R5 | PR #1538 body includes plan 018 audit/salvage summary |

## Implementation Units

### U1. Prove snapshot-only mode

**Files:** `scripts/prove-incremental-append.sh`

Add `--snapshot-only` that writes snapshot TSV and exits (no Discord scrape).

### U2. Prove smoke test

**Files:** `scripts/tests/prove-incremental-append-smoke.sh`

Fixture archives: valid JSON, invalid JSON (skipped), then simulate grow-only compare.

### U3. Workspace bridge

**Files:** `../DiscordChatExporter.linux-x64/RECURRING-SCRAPE.md` (sibling path from repo: document in plan as operator copy target — implement via `scripts/sync-workspace-bridge.sh` or direct edit if path exists)

Use repo-relative note: bridge file lives beside repo at `DiscordChatExporter.linux-x64/RECURRING-SCRAPE.md`.

### U4. PR body refresh

Update PR #1538 via `gh pr edit` with Latest section for `a2aeaaa` and plan 019.

## Verification

- `./scripts/tests/prove-incremental-append-smoke.sh`
- All existing `scripts/tests/*.sh` pass
