## Residual Review Findings

Source: ce-code-review autofix passes on plans 011 and 012.

**Status (2026-05-29):** R1–R3 addressed in `25e1a7e`.

- ~~**[P2][manual]** `last_message_id` snowflake cursor~~ — fixed: padded `sort_by` on message ids.
- ~~**[P3][manual]** `load_guild_channel_cache` diagnostics~~ — fixed: capture CLI output on failure.
- ~~**[P3][advisory]** `DCE_ARCHIVE_ROOT` portability~~ — fixed: compose comment.

**Open:** Use a user token (not bot) for live incremental downloads when channels return 403.
