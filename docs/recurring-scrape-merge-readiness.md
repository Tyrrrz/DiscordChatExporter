# Recurring scrape — merge readiness

## Branch status (2026-06-04)

| Gate | Status |
|------|--------|
| Offline smokes (`run-all-smokes.sh`) | 23/23 pass |
| Live proof (`run-operator-proof.sh --sync-gui --target eod_discord`) | Passed on maintainer host |
| Monthly cron (`setup-cron.sh`) | Installed (`00 04 1 * *`); dry-run preflight OK for all enabled targets |
| Upstream CI (fork PR) | `action_required` until Tyrrrz approves workflow runs |

Fork branch `feat/recurring-cli-scrape` adds append-only, Docker-based incremental exports with optional monthly cron. Intended for personal archive trees under a configurable `archive_root` (for example `~/Documents/*`).

**Recent operator tooling (plans 054–059):** `salvage` subcommand, archive-root scrape lock + `scrape-lock-status.sh`, `--salvage-only` / `--salvage-before-scrape` on validation/documents/handoff/proof, lock gate before scrape, `--reclaim-stale` for dead holders.

GUI zip users: [docs/gui-zip-recurring-scrape-bridge.md](gui-zip-recurring-scrape-bridge.md).

## What ships

- **Config:** `config/scrape-targets.json` — per-server `output_dir`, optional `channel_ids`, `enabled` flags
- **Core:** `scripts/run-discord-scrape.sh` — incremental `--after`, merge-by-id, fail-closed path safety
- **Host:** `scripts/run-discord-scrape-host.sh`, `scripts/run-documents-scrape.sh`, `scripts/bootstrap-recurring-scrape.sh`
- **Auth:** `scrape.env`, `scripts/setup-scrape-auth.sh`, `scripts/sync-token-from-gui.sh`
- **Cron:** `scripts/setup-cron.sh` (`--interval monthly` default)
- **Integrity:** `scripts/audit-archive-json.sh`, `scripts/salvage-truncated-export.sh`, `scripts/prove-incremental-append.sh` (use `--channel ID` for channel-scoped grow-only proof), `scripts/scrape-lock-status.sh`
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
./scripts/run-operator-validation.sh --salvage-only --target KotOR_discord_msgs --channel 221726893064454144
./scripts/run-operator-validation.sh --salvage-before-scrape --target KotOR_discord_msgs --channel 221726893064454144
```

Lock and salvage helpers:

```bash
./scripts/scrape-lock-status.sh
./scripts/scrape-lock-status.sh --reclaim-stale
./scripts/run-documents-scrape.sh --salvage-before-scrape --target NAME [--channel ID]
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

**Plan 044 (2026-06-04):** Offline smoke asserts partial temp preserved on OOM skip (channel 134). Host wrapper prefers `DISCORD_TOKEN_FILE` over inherited shell tokens.

**Plan 061 (2026-06-04):** Shared `scripts/lib/scrape-lock.sh` — lock path, held check, holder formatting, and reclaim helpers sourced by host runner, lock status, and operator wrappers. `run-all-smokes.sh` → 21/21 pass.

**Plans 054–059 (2026-06-04):** Salvage-only subcommand; archive-root lock with meta sidecar; operator validation/proof/handoff salvage flags; `scrape-lock-status.sh` + `--reclaim-stale`; documents scrape lock gate + `--salvage-before-scrape`. `run-all-smokes.sh` → 21/21 pass.

**KotOR / yes_general (plan 040–043):** Incremental `--after` works for all channels; most return `UNCHANGED` in seconds. `yes_general` archive last message was **2021-01-17** — the first catch-up legitimately fetches years of history. Prior bug: OOM skip **deleted** partial temp exports, causing re-download loops. Plan 043 preserves partial temps and salvages on next run.

```bash
docker compose build   # or podman-compose build
DCE_MIN_FREE_MB=0 ./scripts/run-operator-validation.sh --target KotOR_discord_msgs
```

Large `yes_general` may still skip without a higher container cap; `KotOR_discord_msgs` sets `container_memory: "8g"` in `scrape-targets.json` for single-target runs (override globally with `DCE_CONTAINER_MEMORY` in `scrape.env`):

```bash
DCE_MIN_FREE_MB=0 ./scripts/run-operator-validation.sh \
  --salvage-before-scrape --target KotOR_discord_msgs \
  --channel 221726893064454144 \
  --log-file logs/kotor-yes-general.log
# Also writes logs/kotor-yes-general.summary.json (or recovers from log if file write fails)
```

**Plan 063 (2026-06-04):** Optional `DCE_CONTAINER_MEMORY` compose `mem_limit` for large channel catch-up (default 0 = unlimited).

**Plan 064 (2026-06-04):** OOM, scrape-lock, and partial-temp salvage runbooks in `.docs/Recurring-Scrape-Troubleshooting.md`; GUI bridge notes `DCE_CONTAINER_MEMORY` for yes_general.

**Plan 065 (2026-06-04):** Scrape summary labels OOM skips as `SKIPPED (OOM/aborted)` with operator hint; `verify-operator-ready` prints configured container memory.

**Plan 066 (2026-06-04):** `prove-incremental-append --channel` filters snapshots and grow-only comparison to selected channels.

**Plan 067 (2026-06-04):** Optional per-target `container_memory` in `scrape-targets.json` (single `--target` runs); `KotOR_discord_msgs` defaults to `8g`.

**Plan 068 (2026-06-04):** `verify-documents-archives` MEM column and `verify-operator-ready` target memory hints when global cap unset.

**Plan 069 (2026-06-04):** Optional JSON scrape run summary via `DCE_RUN_SUMMARY_JSON` / `DCE_RUN_SUMMARY_FILE`.

**Plan 070 (2026-06-04):** Compose mounts `logs/` at `/logs`; host runner passthrough; operator-validation auto-writes `*.summary.json` beside `--log-file`.

**Plan 071 (2026-06-04):** When summary file write fails, operator validation recovers JSON from the last `DCE_JSON_SUMMARY:` line in the teed log.

**Plan 072 (2026-06-04):** Host runner recovers JSON summary from the captured compose run log before deleting the temp file.

**Plan 073 (2026-06-04):** Operator proof auto-writes `*.summary.json` beside proof log with tee-log recovery (parity with validation).

**Plan 074 (2026-06-04):** `print-scrape-summary.sh` pretty-prints `*.summary.json` (`--json`, `--oom-only`, stdin `-`).

**Plan 075 (2026-06-04):** `run-documents-scrape.sh` auto-writes `logs/documents-scrape-<UTC>.summary.json` on live scrapes.

**Plan 076 (2026-06-04):** Multi-target validation (`--per-target`) and proof loops write separate `logs/operator-*-<target>-<UTC>.summary.json` per scrape.

**Plan 077 (2026-06-04):** Setup doc + merge-readiness smoke inventory synced to 23 offline tests (includes `print-scrape-summary-smoke`, `scrape-summary-json-smoke`).

**Plan 078 (2026-06-04):** `run-documents-scrape.sh` `--log-file` with auto tee on live scrapes; summary pairs with log basename.

**Plan 079 (2026-06-04):** `setup-cron.sh` installs `run-documents-scrape.sh --log-file` (unified workflow + JSON summary) instead of bare host scrape redirect.

**Disk:** ~65 GiB free on `/home` (2026-05-30); large channel merges still need headroom.

## CI note (fork PRs)

Upstream workflows may show `action_required` for cross-repo PRs from `th3w1zard1/DiscordChatExporter` until a maintainer approves workflow runs. Local `run-all-smokes.sh` is the authoritative offline gate.
