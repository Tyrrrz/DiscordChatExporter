---
title: feat: Per-target full validation pass
type: feat
status: complete
date: 2026-05-29
origin: Repeated /lfg — run all 9 enabled Documents targets with resilient orchestration
---

# feat: Per-target full validation pass

## Summary

Add `--per-target` mode to `run-operator-validation.sh` so each enabled server is scraped and audited independently (one failure does not block the rest). Execute full host validation with GUI token sync.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `--per-target` runs scrape + audit per enabled target with summary |
| R2 | `--continue-on-error` keeps going when a target fails |
| R3 | Smoke covers per-target dry-run with two targets |
| R4 | Host run: `./scripts/run-operator-validation.sh --sync-gui --per-target --continue-on-error` |
| R5 | Update merge-readiness with per-target command |

## Verification

- `./scripts/tests/run-operator-validation-smoke.sh` (extend for --per-target)
- `./scripts/run-all-smokes.sh`
- Host full pass completes or logs per-target failures
