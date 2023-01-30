# DiscordChatExporter Tests

This test suite runs against a real Discord server, specifically created to exercise different behaviors required by the test scenarios.
In order to run these tests locally, you need to join the test server and configure your authentication token.

1. [Join the test server](https://discord.gg/eRV8Vap5bm)
2. Locate your Discord authentication token
3. Add your token to user secrets: `dotnet user-secrets set DISCORD_TOKEN <token>`
4. Run the tests: `dotnet test`

> **Note**:
> If you want to add a new test case, please let me know and I will give you the required permissions on the server.