# 🛠️ DiscordChatExporter - Windows Installer

This folder contains a **ready-to-use Inno Setup script** to build a native `.exe` installer for the **DiscordChatExporter CLI** tool on Windows.

---

## ⚙️ Requirements

- **Windows**
- [Inno Setup 6.x](https://jrsoftware.org/isdl.php) (Free and lightweight)

---

## 📦 Installer Features

- Installs all required CLI files to `Program Files\DiscordChatExporter`
- Optionally adds the CLI to your system `PATH`
- Adds Start Menu shortcuts
- Automatically runs after install (optional)

---

## 🚀 How to Build the Installer

1. **Install Inno Setup 6** if you haven't already.  
   ➤ https://jrsoftware.org/isdl.php

2. **Prepare your `App/` folder** next to this script.  
   This folder must contain all the `.exe`, `.dll`, and related files for the CLI.

   Example:
```

Installer/
├── DiscordChatExporterInstaller.iss
└── App/
├── DiscordChatExporter.Cli.exe
├── \*.dll
└── \*.json

```

3. **Open `DiscordChatExporterInstaller.iss`** in the Inno Setup GUI.

4. Click **Build ➜ Compile** ✅

5. You’ll get an installer like:
```

Output/DiscordChatExporterSetup.exe

````

---

## 🧪 Testing

- After install, open **CMD** and run:
```sh
DiscordChatExporter.Cli.exe
````

If it runs from any directory → you're golden 🏆

---

## 📝 Notes

* This installer is for **CLI only**, not the GUI version.
* You can modify the script to include custom icons, silent mode, or GUI launcher.
* Want to auto-build it with GitHub Actions? Ping the project maintainer 💬

---

**Enjoy the CLI like a boss.**

```




