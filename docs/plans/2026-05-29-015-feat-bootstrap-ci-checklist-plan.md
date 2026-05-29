---
title: feat: Bootstrap CI smoke and operator checklist
type: feat
status: completed
date: 2026-05-29
origin: LFG — lock operator path; CI covers bootstrap; live bootstrap verification
---

# feat: Bootstrap CI smoke and operator checklist

## Summary

Add CI smoke for `bootstrap-recurring-scrape.sh`, a short operator checklist, align docs on bootstrap-first workflow, and verify live bootstrap against `scrape.env`.

## Requirements

| ID | Requirement |
|----|-------------|
| R1 | `bootstrap-recurring-scrape-smoke.sh` exercises `--help` and `--dry-run` |
| R2 | CI `recurring-scrape-smoke` job runs bootstrap smoke |
| R3 | `docs/recurring-scrape-operator-checklist.md` lists end-to-end steps |
| R4 | Recurring setup doc references bootstrap as primary entry |
| R5 | Live `./scripts/bootstrap-recurring-scrape.sh --skip-build` succeeds with existing `scrape.env` |

## Implementation Units

### U1. Bootstrap smoke + CI

**Files:** `scripts/tests/bootstrap-recurring-scrape-smoke.sh`, `.github/workflows/main.yml`

### U2. Operator checklist + docs

**Files:** `docs/recurring-scrape-operator-checklist.md`, `.docs/Recurring-Scrape-Setup.md`

## Verification

- All `scripts/tests/*.sh`
- Live bootstrap (skip-build) on one target
