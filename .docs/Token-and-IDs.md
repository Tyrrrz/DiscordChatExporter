# Obtaining Token and Channel IDs

> **Warning**:
> **Do not share your token!**
> A token gives full access to an account. To reset a user token, change your account password. To reset a bot token, click on [Regenerate](#how-to-get-a-bot-token) in the bot settings.

## How to get a User Token

**Caution:** [Automating user accounts violates Discord's terms of service](https://support.discord.com/hc/en-us/articles/115002192352-Automated-user-accounts-self-bots-) and may result in account termination. Use at your own risk.

### Through your web browser

Prerequisite step: Navigate to [discord.com](https://discord.com) and login.

#### In Chrome

##### Using the console

1. <img width="500" align="right" src="https://i.imgur.com/zdDwIT5.jpg" />Press <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>I</kbd> (<kbd>⌥</kbd>+<kbd>⌘</kbd>+<kbd>I</kbd> on macOS). Chrome's [DevTools](https://developer.chrome.com/docs/devtools/overview) tools will display.

<br clear="right" />
<br />

2. Click the `Console` tab. The [console](https://developer.chrome.com/docs/devtools/console/) will open.

3. Type

    ```console
    (webpackChunkdiscord_app.push([[''],{},e=>{m=[];for(let c in e.c)m.push(e.c[c])}]),m).find(m=>m?.exports?.default?.getToken!==void 0).exports.default.getToken()
    ```

    into the console and press <kbd>Enter</kbd>. The console will display your user token.

##### Using the network monitor

1. <img width="500" align="right" src="https://i.imgur.com/zdDwIT5.jpg" />Press <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>I</kbd> (<kbd>⌥</kbd>+<kbd>⌘</kbd>+<kbd>I</kbd> on macOS). Chrome's [DevTools](https://developer.chrome.com/docs/devtools/overview) tools will display.

<br clear="right" />
<br />

2. <img width="500" align="right" src="https://i.imgur.com/BDeG0zg.png" />Click the `Network` tab. The [network panel](https://developer.chrome.com/docs/devtools/overview/#network) will open

<br clear="right" />
<br />

3. <img width="500" align="right" src="https://i.imgur.com/0Lgj0vk.png" />Press <kbd>F5</kbd>. The page will reload, and the network log (the lower half of the network panel) will display several entries.

<br clear="right" />
<br />

4. <img width="500" align="right" src="https://i.imgur.com/rnZG8Id.png" />Click the text box labelled `Filter` and type `messages`. The entries will filter down to a single request named `messages`. If the request doesn't appear, switch to any other Discord channel to trigger it.

<br clear="right" />
<br />

5. <img width="500" align="right" src="https://i.imgur.com/29dE3fR.png" />Click the entry named `messages`. A panel will open to the right and display details about the entry. Click the `Headers` tab if it isn't already active.

<br clear="right" />
<br />

6. <img width="500" align="right" src="https://i.imgur.com/u7CxXAt.png" />Scroll through the contents of the `Headers` tab until you find an entry beginning with `authorization:`.

<br clear="right" />
<br />

7. <img width="500" align="right" src="https://i.imgur.com/dXcXzma.png" />Right-click the entry and click `copy value`.

<br clear="right" />
<br />

##### Using the storage inspector

1. <img width="500" align="right" src="https://i.imgur.com/zdDwIT5.jpg" />Press <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>I</kbd> (<kbd>⌥</kbd>+<kbd>⌘</kbd>+<kbd>I</kbd> on macOS). Chrome's [DevTools](https://developer.chrome.com/docs/devtools/overview/) will display.

<br clear="right" />
<br />

2. <img width="500" align="right" src="https://i.imgur.com/biAUIop.png" />Press <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>M</kbd> (<kbd>⌘</kbd>+<kbd>Shift</kbd>+<kbd>M</kbd>). Chrome will enter [Device Mode](https://developer.chrome.com/docs/devtools/device-mode/), and the webpage will display as if on a mobile device.

<br clear="right" />
<br />

3. <img width="500" align="right" src="https://i.imgur.com/oUDRZoy.png" />If necessary, click the `»` at the right end of the tab bar, and click `Application`. The [application panel](https://developer.chrome.com/docs/devtools/overview/#application) will display.

<br clear="right" />
<br />

4. <img width="500" align="right" src="https://i.imgur.com/sydNPia.png" />In the menu to the right, under `Storage`, expand `Local Storage` if necessary, then click `https://discord.com`. The pane to the right will display a list of key-value pairs.

<br clear="right" />
<br />

5. <img width="500" align="right" src="https://i.imgur.com/qKo0ny9.png" />In the text box marked `Filter`, type `token`. The entries will filter down to those containing the string `token`.

<br clear="right" />
<br />

6. <img width="500" align="right" src="https://i.imgur.com/caj3lQq.png" />Click the `token` entry. (Note: if the token doesn't display, try refreshing by pressing <kbd>F5</kbd> or <kbd>⌘</kbd>+<kbd>R</kbd> on macOS)

<br clear="right" />
<br />

7. <img width="500" align="right" src="https://i.imgur.com/SwWFIH4.png" />Click the text box at the bottom, press <kbd>Ctrl</kbd>+<kbd>A</kbd> (<kbd>⌘</kbd>+<kbd>A</kbd> on macOS) then <kbd>Ctrl</kbd>+<kbd>C</kbd> (<kbd>⌘</kbd>+<kbd>C</kbd> on macOS) to copy the value to your clipboard.

<br clear="right" />
<br />

#### In Firefox

##### Using the console

1. <img width="500" align="right" src="https://i.imgur.com/O34nwdG.png" />Press <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>K</kbd> (<kbd>⌥</kbd>+<kbd>⌘</kbd>+<kbd>K</kbd> on macOS). Firefox’s [web developer tools](https://firefox-source-docs.mozilla.org/devtools-user/) will display at the bottom of the window, and the [web console](https://firefox-source-docs.mozilla.org/devtools-user/console/index.html) will display.

<br clear="right" />
<br />

2. Click the `Console` tab. The [console](https://firefox-source-docs.mozilla.org/devtools-user/console/index.html) will open.

1. Type

    ```console
    (webpackChunkdiscord_app.push([[''],{},e=>{m=[];for(let c in e.c)m.push(e.c[c])}]),m).find(m=>m?.exports?.default?.getToken!==void 0).exports.default.getToken()
    ```

    into the console and press <kbd>Enter</kbd>. The console will display your user token.

##### Using the network monitor

1. <img width="500" align="right" src="https://i.imgur.com/O34nwdG.png" />Press <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>E</kbd> (<kbd>⌥</kbd>+<kbd>⌘</kbd>+<kbd>E</kbd> on macOS). Firefox’s [web developer tools](https://firefox-source-docs.mozilla.org/devtools-user/) will display at the bottom of the window, and the [network monitor](https://firefox-source-docs.mozilla.org/devtools-user/network_monitor/) will display.

<br clear="right" />
<br />

2. <img width="500" align="right" src="https://i.imgur.com/j00QzhU.png" />Press <kbd>F5</kbd>. The page will reload, and the [network request list](https://firefox-source-docs.mozilla.org/devtools-user/network_monitor/request_list/index.html) will populate with entries.

<br clear="right" />
<br />

3. <img width="500" align="right" src="https://i.imgur.com/efUCfBO.png" />Type `messages` into the filter. The network request list will filter out any entries not containing the string `messages`. If the request doesn't appear, switch to any other Discord channel to trigger it.

<br clear="right" />
<br />

4. <img width="500" align="right" src="https://i.imgur.com/cdJZ7Q1.png" />Click `messages`. The [network request details pane](https://firefox-source-docs.mozilla.org/devtools-user/network_monitor/request_details/index.html) will display. The [headers tab](https://firefox-source-docs.mozilla.org/devtools-user/network_monitor/request_details/index.html#network-monitor-request-details-headers-tab) should be active by default. If it isn’t, click it.

<br clear="right" />
<br />

5. <img width="500" align="right" src="https://i.imgur.com/zBmq1JW.png" />Type `authorization` into the text box labelled `Filter Headers`.

<br clear="right" />
<br />

6. <img width="500" align="right" src="https://i.imgur.com/O3blcIS.png" />Scroll down until you see an entry labeled [authorization](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Authorization) under `Request Headers`.

<br clear="right" />
<br />

7. <img width="500" align="right" src="https://i.imgur.com/zHYEYoZ.png" />Right-click the entry labeled `authorization` and select `copy value`.

<br clear="right" />
<br />

##### Using the storage inspector

1. <img width="500" align="right" src="https://i.imgur.com/A8jqpkm.png" />Press <kbd>Shift</kbd>+<kbd>F9</kbd>. Firefox’s [web developer tools](https://firefox-source-docs.mozilla.org/devtools-user/) will display at the bottom of the window, and the [storage](https://firefox-source-docs.mozilla.org/devtools-user/storage_inspector/index.html) panel will be selected.

<br clear="right" />
<br />

2. <img width="500" align="right" src="https://i.imgur.com/TGcbB7f.png" />Press <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>M</kbd> (<kbd>⌥</kbd>+<kbd>⌘</kbd>+<kbd>M</kbd> on macOS). Firefox will toggle [responsive design mode](https://firefox-source-docs.mozilla.org/devtools-user/responsive_design_mode/), and the web page will display as if on a mobile device. (Note: Discord may steal focus and respond to the command by toggling mute. If this happens, return focus to Firefox’s web developer tools by clicking somewhere in it, then try the command again.)

<br clear="right" />
<br />

3. <img width="500" align="right" src="https://i.imgur.com/2xWkep9.png" />In the [storage tree](https://firefox-source-docs.mozilla.org/devtools-user/storage_inspector/index.html#storage-inspector-storage-tree) (the list on the left side of the web developer tools panel), click [Local Storage](https://developer.mozilla.org/en-US/docs/Web/API/Window/localStorage). The entry will expand, and the entry `https://discord.com` will display beneath it.

<br clear="right" />
<br />

4. <img width="500" align="right" src="https://i.imgur.com/tGlGuOL.png" />In the storage tree, click `https://discord.com`. The [table widget](https://firefox-source-docs.mozilla.org/devtools-user/storage_inspector/index.html#storage-inspector-table-widget) to the right of the storage tree will display several key-value pairs.

<br clear="right" />
<br />

5. <img width="500" align="right" src="https://i.imgur.com/hDNsnZ5.png" />In the text box labelled `Filter items` at the top of the table widget, enter `token`. The table will now only display entries containing the string `token`.

<br clear="right" />
<br />

6. <img width="500" align="right" src="https://i.imgur.com/8fKId1W.png" />Click the entry `token`. The [sidebar](https://firefox-source-docs.mozilla.org/devtools-user/storage_inspector/index.html#storage-inspector-sidebar) will display. (Note: If the token doesn’t display, try refreshing by pressing <kbd>F5</kbd>.)

<br clear="right" />
<br />

7. <img width="500" align="right" src="https://i.imgur.com/yD1ZuR9.png" />Right-click the single entry in the sidebar and select `copy`.

<br clear="right" />
<br />

### Through the desktop app / enabling web developer tools

#### By editing the settings file

1. If Discord is running, exit the application by right-clicking the icon in your taskbar tray and clicking `Quit Discord`.

2. Open Discord's settings file in your preferred text editor. See the following table for help finding it:

   | OS      | Stable                                                | Canary                                                      | Public Test Build (PTB)                                  |
   | ------- | ----------------------------------------------------- | ----------------------------------------------------------- | -------------------------------------------------------- |
   | Windows | `%APPDATA%\discord\settings.json`                     | `%APPDATA%\discordcanary\settings.json`                     | `%APPDATA%\discordptb\settings.json`                     |
   | macOS   | `~/Library/Application Support/discord/settings.json` | `~/Library/Application Support/discordcanary/settings.json` | `~/Library/Application Support/discordptb/settings.json` |
   | Linux   | `~/.config/discord/settings.json`                     | `~/.config/discordcanary/settings.json`                     | `~/.config/discordptb/settings.json`                     |

   If you use BetterDiscord, use the following table instead:

   | OS      | Stable                                                                  | Canary                                                                  | Public Test Build (PTB)                                              |
   | ------- | ----------------------------------------------------------------------- | ----------------------------------------------------------------------- | -------------------------------------------------------------------- |
   | Windows | `%APPDATA%\BetterDiscord\data\stable\settings.json`                     | `%APPDATA%\BetterDiscord\data\canary\settings.json`                     | `%APPDATA%\BetterDiscord\data\ptb\settings.json`                     |
   | macOS   | `~/Library/Application Support/BetterDiscord/data/stable/settings.json` | `~/Library/Application Support/BetterDiscord/data/canary/settings.json` | `~/Library/Application Support/BetterDiscord/data/ptb/settings.json` |
   | Linux   | `~/.config/BetterDiscord/data/stable/settings.json`                     | `~/.config/BetterDiscord/data/canary/settings.json`                     | `~/.config/BetterDiscord/data/ptb/settings.json`                     |

3. Insert a blank line after the first curly bracket (`{`), add the text `"DANGEROUS_ENABLE_DEVTOOLS_ONLY_ENABLE_IF_YOU_KNOW_WHAT_YOURE_DOING": true,` to it, and save the file. Your file should resemble the following:

```json
{
  "DANGEROUS_ENABLE_DEVTOOLS_ONLY_ENABLE_IF_YOU_KNOW_WHAT_YOURE_DOING": true,
  "BACKGROUND_COLOR": "#202225",
  "IS_MAXIMIZED": true
}
```

4. Launch Discord.

5. To find your user token, continue [here](#in-chrome).

#### Via settings menu (BetterDiscord only)

1. <img width="500" align="right" src="https://i.imgur.com/mu1g4OF.png" />Click the User Settings button (the gear icon to the right of your username). Discord’s settings page will open.

<br clear="right" />
<br />

2. <img width="500" align="right" src="https://i.imgur.com/qFrIKON.png" />In the sidebar to the left, click `Settings` under the `BetterDiscord` group. BetterDiscord’s settings page will display.

<br clear="right" />
<br />

3. <img width="500" align="right" src="https://i.imgur.com/48Re5kj.png" />In the main panel to the right, expand the `Developer Settings` group if necessary, and toggle `DevTools` to enabled.

<br clear="right" />
<br />

4. Press <kbd>Esc</kbd>. The settings page will close.
5. To find your user token, continue [here](#in-chrome).

## How to get a Bot Token

1. Go to [Discord developer portal](https://discord.com/developers/applications)
2. Open your Application's settings
3. Navigate to the **Bot** section on the left
4. Under **Token** click **Copy**

> **Warning**:
> Your bot needs to have **Message Content Intent** enabled for it to be able to read messages!

![https://discord.com/developers/applications/](https://i.imgur.com/BdrrxlY.png)

---

## How to get a Server ID or a Channel ID

1. Open Discord Settings
2. Go to the **Advanced** section
3. Enable **Developer Mode**
4. Right-click on the desired server or channel and click **Copy Server ID** or **Copy Channel ID**
