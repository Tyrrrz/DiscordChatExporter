---
title: feat: Live operator validation runner
type: feat
status: complete
date: 2026-05-29
origin: Repeated /lfg — execute end-to-end validation on host, not only offline smokes
---

# feat: Live operator validation runner

## Summary

Add `run-operator-validation.sh` to orchestrate GUI token sync, readiness checks, incremental scrape, and post-run JSON audit with a timestamped log.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `scripts/run-operator-validation.sh` supports `--dry-run`, `--target`, `--sync-gui`, `--skip-scrape` |
| R2 | Writes `logs/operator-validation-*.log` with step timestamps |
| R3 | Post-scrape runs `audit-archive-json.sh` per affected targets |
| R4 | Offline smoke for dry-run path |
| R5 | Document in merge-readiness as post-bootstrap validation |

## Verification

- `./scripts/tests/run-operator-validation-smoke.sh`
- `./scripts/run-all-smokes.sh`
- Host: `./scripts/run-operator-validation.sh --dry-run` exits 0
