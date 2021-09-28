# DiscordChatExporter Tests

This test suite runs against a real Discord server, specifically created to exercise different behaviors required by the test scenarios.
In order to run these tests locally, you need to join the test server and configure your authentication token.

1. Join the server: https://discord.gg/eRV8Vap5bm
2. Locate your Discord authentication token
3. Specify your token using a file or an environment variable:
    - **Using a file**: put your token in a new `DiscordToken.secret` file created in this directory
    - **Using an environment variable**: set `DISCORD_TOKEN` variable to your token
4. Run the tests: `dotnet test`

> If you want to have a new test case or a scenario added, please let me know in your pull request.
Currently, it's not possible to add them by yourself.