# Recurring scrape — merge readiness

Fork branch `feat/recurring-cli-scrape` adds append-only, Docker-based incremental exports with optional monthly cron. Intended for personal archive trees under a configurable `archive_root` (for example `~/Documents/*`).

## What ships

- **Config:** `config/scrape-targets.json` — per-server `output_dir`, optional `channel_ids`, `enabled` flags
- **Core:** `scripts/run-discord-scrape.sh` — incremental `--after`, merge-by-id, fail-closed path safety
- **Host:** `scripts/run-discord-scrape-host.sh`, `scripts/run-documents-scrape.sh`, `scripts/bootstrap-recurring-scrape.sh`
- **Auth:** `scrape.env`, `scripts/setup-scrape-auth.sh`, `scripts/sync-token-from-gui.sh`
- **Cron:** `scripts/setup-cron.sh` (`--interval monthly` default)
- **Integrity:** `scripts/audit-archive-json.sh`, `scripts/salvage-truncated-export.sh`, `scripts/prove-incremental-append.sh`
- **CI:** `.github/workflows/main.yml` job `recurring-scrape-smoke` runs `./scripts/run-all-smokes.sh`

## Validate before merge

```bash
./scripts/run-all-smokes.sh
./scripts/run-all-smokes.sh --include-container   # optional; needs Docker/Podman
```

## Operator quick path

```bash
./scripts/verify-operator-ready.sh
cp scrape.env.example scrape.env   # or ./scripts/sync-token-from-gui.sh --force
./scripts/bootstrap-recurring-scrape.sh
./scripts/run-documents-scrape.sh
./scripts/setup-cron.sh --dry-run
```

Optional Discord probe for one target:

```bash
./scripts/verify-operator-ready.sh --preflight KotOR_discord_msgs
```

Full validation with log (GUI token sync + scrape + audit):

```bash
./scripts/run-operator-validation.sh --sync-gui
./scripts/run-operator-validation.sh --sync-gui --target eod_discord
./scripts/run-operator-validation.sh --sync-gui --per-target --continue-on-error
./scripts/run-operator-validation.sh --dry-run
```

Detail: [.docs/Recurring-Scrape-Setup.md](../.docs/Recurring-Scrape-Setup.md) · [operator checklist](recurring-scrape-operator-checklist.md) · [troubleshooting](../.docs/Recurring-Scrape-Troubleshooting.md)

## CI note (fork PRs)

Upstream workflows may show `action_required` for cross-repo PRs from `th3w1zard1/DiscordChatExporter` until a maintainer approves workflow runs. Local `run-all-smokes.sh` is the authoritative offline gate.
