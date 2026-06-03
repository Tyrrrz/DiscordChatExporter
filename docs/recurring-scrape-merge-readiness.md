# Recurring scrape — merge readiness

## Branch status (2026-05-30)

| Gate | Status |
|------|--------|
| Offline smokes (`run-all-smokes.sh`) | 19/19 pass (includes abort exit 134 skip regression) |
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
| KotOR_discord_msgs | **scrape pass / audit pass*** | pass* | plan 045: audit excludes `.dce-temp` partials; yes_general catch-up in progress with preserved partial temps (~23–29 MiB) |

\* Audit failed before plan 045 because truncated partial exports under `.dce-temp/` were scanned as archives. After fix, audit passes while partial temps exist.

**Plan 047 (2026-06-04):** Treat SIGTERM (143) and SIGINT (130) export exits as skippable aborts — stopping validation no longer fails the whole target with `ERROR: Channel failed`. `.dce-scrape.lock` gitignored.

**Plan 046 (2026-06-04):** `run-discord-scrape-host.sh scrape` holds non-blocking `flock` on `.dce-scrape.lock` so overlapping manual/cron validation cannot spawn twin yes_general exports. Stop duplicate runs before restarting KotOR validation.

**Plan 045 (2026-06-04):** `audit-archive-json.sh` and `verify-documents-archives.sh` skip `*/.dce-temp/*` (in-progress partial exports). Salvage run 2026-06-03: 7 merged, 17 unchanged, 3 skipped (+5404 messages); yes_general OOM-skipped with partial temps preserved for next salvage.

**Plan 044 (2026-06-04):** Offline smoke asserts partial temp preserved on OOM skip (channel 134). Host wrapper prefers `DISCORD_TOKEN_FILE` over inherited shell tokens. `run-all-smokes.sh` → 19/19 pass.

**KotOR / yes_general (plan 040–043):** Incremental `--after` works for all channels; most return `UNCHANGED` in seconds. `yes_general` archive last message was **2021-01-17** — the first catch-up legitimately fetches years of history. Prior bug: OOM skip **deleted** partial temp exports, causing re-download loops. Plan 043 preserves partial temps and salvages on next run.

```bash
docker compose build   # or podman-compose build
DCE_MIN_FREE_MB=0 ./scripts/run-operator-validation.sh --target KotOR_discord_msgs
```

Large `yes_general` may still skip; export that channel separately with more container memory if needed.

**Disk:** ~65 GiB free on `/home` (2026-05-30); large channel merges still need headroom.

## CI note (fork PRs)

Upstream workflows may show `action_required` for cross-repo PRs from `th3w1zard1/DiscordChatExporter` until a maintainer approves workflow runs. Local `run-all-smokes.sh` is the authoritative offline gate.
