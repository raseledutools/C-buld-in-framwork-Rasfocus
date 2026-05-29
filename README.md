# 🎯 FocusBlocker v2

Modern teal & white focus app for Windows. Built with C# + .NET Framework 4.8.

## Features
- ⏱️ **Animated arc timer** — visual Pomodoro countdown
- 🚫 **Website blocker** — chip-style site list, system-wide hosts file blocking
- 📅 **Block Schedule** — auto-block sites on specific days & times
- 🔔 **System tray** — minimises to tray, balloon notifications
- 💾 Tiny EXE, no runtime install needed (Windows 7/8/10/11)

## Get the EXE from GitHub Actions

1. Push this repo to GitHub
2. Go to **Actions** tab → latest run
3. Download **FocusBlocker-EXE** artifact

## Run locally
```
msbuild FocusBlocker.sln /p:Configuration=Release
```

> ⚠️ Run as **Administrator** — required to edit the hosts file for blocking.
