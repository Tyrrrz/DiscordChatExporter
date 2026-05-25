---
name: Recurring Discord scrape automation
last_updated: 2026-05-25
---

# Recurring Discord scrape automation Strategy

## Target problem

The user needs multiple Discord archives refreshed on a schedule without losing already downloaded history, but the upstream exporter overwrites files by default and Discord auth constraints make unattended scraping brittle. The hard part is keeping long-lived local archives safe while still making recurring updates practical across many custom archive roots.

## Our approach

Wrap the source-built CLI in a self-hosted Docker + cron workflow that defaults to safe recurring operation: append-only archive updates, conservative target resolution, human-readable first-write defaults inside each configured archive root, and fail-closed preflight before anything touches cron or existing exports. We win by treating data preservation and operator clarity as load-bearing, not optional polish.

## Who it's for

**Primary:** Self-hosting Discord archivists and maintainers of small-to-medium community portfolios - They're hiring this workflow to keep many Discord archives current on a recurring schedule without manually re-running exports or risking local history loss.

## Key metrics

- **Successful preflight rate** - Share of setup attempts that complete authenticated preflight cleanly; measured from `scripts/setup-cron.sh` output and cron-install outcomes.
- **Successful recurring scrape runs** - Share of scheduled runs that finish without target failures; measured from the cron log configured by `scripts/setup-cron.sh`.
- **Archive preservation incidents** - Count of runs that truncate, overwrite, or mismatch an existing archive; measured from wrapper error logs and manual archive validation. This should stay at zero.
- **Enabled target coverage** - Number of configured targets that can be resolved and authenticated with the current token; measured from `preflight` results against `config/scrape-targets.json`.

## Tracks

### Archive safety

Keep recurring updates append-only and recoverable so existing exports survive reruns, upstream deletions, and local edge cases.

_Why it serves the approach:_ The whole workflow fails if operators cannot trust it around existing archives.

### Target and auth resolution

Make target selection deterministic across explicit IDs, archive-seeded channel IDs, and bot-token limitations, with clear failure messages when Discord access is missing.

_Why it serves the approach:_ Conservative resolution is what turns a brittle export script into something safe enough for unattended cron.

### Operator runtime

Ship one source-built container + cron path with practical docs, smoke coverage, and environment defaults that match how self-hosted operators actually run it.

_Why it serves the approach:_ The workflow only helps if setup, reruns, and troubleshooting stay understandable outside a one-off terminal session.

## Not working on

- Rehydrating old edited messages, reactions, or other historical mutations that are not present in incremental exports.
- DM scraping with a bot token; that needs a different auth mode.
- Discovery for Discord targets the current token cannot access.

## Marketing

**One-liner:** A self-hosted recurring Discord archive runner that updates existing exports without destroying history.

**Key message:** Build the CLI from source, schedule it once, and let it refresh the archives you already trust. When auth or target resolution is wrong, it fails closed instead of silently corrupting your local archive set.
