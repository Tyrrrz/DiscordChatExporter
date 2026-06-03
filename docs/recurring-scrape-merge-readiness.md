# Recurring scrape — merge readiness

## Branch status (2026-05-29)

| Gate | Status |
|------|--------|
| Offline smokes (`run-all-smokes.sh`) | 19/19 pass |
| Live proof (`run-operator-proof.sh --sync-gui --target eod_discord`) | Passed on maintainer host |
| Monthly cron (`setup-cron.sh`) | Installed (`00 04 1 * *`); dry-run preflight OK for all enabled targets |
| Upstream CI (fork PR) | `action_required` until Tyrrrz approves workflow runs |

**Merge-ready** for upstream review. Further feature work should use a new branch; avoid additional `/lfg` passes unless scope changes.

Fork branch `feat/recurring-cli-scrape` adds append-only, Docker-based incremental exports with optional monthly cron. Intended for personal archive trees under a configurable `archive_root` (for example `~/Documents/*`).

GUI zip users: [docs/gui-zip-recurring-scrape-bridge.md](gui-zip-recurring-scrape-bridge.md).

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
./scripts/operator-handoff.sh      # disk + verify + archive dry-run
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

Single-target live proof (handoff → scrape → grow-only check):

```bash
./scripts/run-operator-proof.sh --sync-gui --target eod_discord
./scripts/run-operator-proof.sh --dry-run   # handoff only
```

Full validation with log (GUI token sync + scrape + audit):

```bash
./scripts/run-operator-validation.sh --sync-gui
./scripts/run-operator-validation.sh --sync-gui --target eod_discord
./scripts/run-operator-validation.sh --sync-gui --per-target --continue-on-error
./scripts/run-operator-validation.sh --dry-run
```

Detail: [.docs/Recurring-Scrape-Setup.md](../.docs/Recurring-Scrape-Setup.md) · [operator checklist](recurring-scrape-operator-checklist.md) · [troubleshooting](../.docs/Recurring-Scrape-Troubleshooting.md)

## Disk space

Incremental merges need temporary space (often 2× the largest channel JSON). Before scraping:

```bash
df -h ~/Documents /home/brunner56/Downloads/DiscordChatExporter
./scripts/verify-operator-ready.sh   # fails below 1 GiB free by default
```

Override threshold: `DCE_MIN_FREE_MB=2048 ./scripts/verify-operator-ready.sh`  
Skip check (smokes only): `DCE_MIN_FREE_MB=0`  
Also enforced by `run-documents-scrape.sh`, `run-discord-scrape-host.sh` (cron), and `run-operator-validation.sh`.

**Podman hosts:** install `podman-compose` (`dnf install podman-compose`) when `docker compose` cannot reach the socket; scripts auto-prefer `podman-compose` when present.

## Host validation (2026-05-29 / 2026-05-30)

### Single-target proof (`eod_discord`)

```bash
./scripts/run-operator-proof.sh --sync-gui --target eod_discord
```

Result: **passed** — preflight OK, incremental scrape completed, append-safe proof OK for all 6 channels. Log: `logs/operator-proof-20260529T213341Z.log`.

### Full per-target validation (`--per-target --continue-on-error`)

```bash
DCE_MIN_FREE_MB=0 ./scripts/run-operator-validation.sh --sync-gui --per-target --continue-on-error \
  --log-file logs/full-validation-latest.log
```

**Combined 2026-05-30 validation** (`logs/full-validation-latest.log` + `logs/validation-resume-20260530.log`):

| Target | Scrape | Audit | Notes |
|--------|--------|-------|-------|
| ror_orig_discord | pass | pass | full-validation run |
| ror_new_discord | pass | pass | full-validation run |
| openkotor_discord_msgs | pass | pass | full-validation run |
| KotOR_Speedrun_Discord | pass | pass | 7 channels skipped (forbidden) |
| holocron_toolset_discord | pass | pass | validation-resume |
| expanded_kotor_discord | pass | pass | validation-resume |
| eod_discord | pass | pass | validation-resume |
| DS_Discord_msgs | pass | pass | validation-resume; some channels forbidden |
| KotOR_discord_msgs | **retry** | — | `yes_general` CLI abort (OOM); fixed in plan 040 to skip channel on exit 134/137/139 |

**KotOR remediation (plan 040):** `run-discord-scrape.sh` skips channels when export exits 134/137/139 (abort/OOM) or log matches disk/forbidden patterns. Re-run:

```bash
docker compose build   # or podman-compose build
DCE_MIN_FREE_MB=0 ./scripts/run-operator-validation.sh --target KotOR_discord_msgs
```

Large `yes_general` may still skip; export that channel separately with more container memory if needed.

**Disk:** ~22 GiB free on `/home` (2026-05-30); large channel merges still need headroom.

## CI note (fork PRs)

Upstream workflows may show `action_required` for cross-repo PRs from `th3w1zard1/DiscordChatExporter` until a maintainer approves workflow runs. Local `run-all-smokes.sh` is the authoritative offline gate.
