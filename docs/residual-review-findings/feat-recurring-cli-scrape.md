## Residual Review Findings

Source: ce-code-review autofix pass on `docs/plans/2026-05-29-011-feat-documents-recurring-scrape-verify-plan.md`

- **[P2][manual]** `scripts/run-discord-scrape.sh` (`last_message_id`) — `max_by(.id)` uses string comparison; mixed-length snowflakes can pick wrong cursor and re-fetch history. Suggested fix: `sort_by(.id) | last | .id` or padded numeric compare.
- **[P3][manual]** `scripts/run-discord-scrape.sh` (`load_guild_channel_cache`) — Channel listing failures exit without CLI stderr context. Mirror `load_guild_cache` error capture.
- **[P3][advisory]** `docker-compose.yml` — `DCE_ARCHIVE_ROOT` defaults to `/home/brunner56/Documents`; set explicitly on other hosts.
