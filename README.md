<p align="center">
  <img src="docs/media/logo.png" alt="1v1 Timer" width="160">
</p>

<h3 align="center">Two stopwatches for 1v1 matches, on top of your game.</h3>

<p align="center">
  Start, stop and switch timers with a key, a mouse button or your controller, while the game still gets every key.<br>
  Free, open source and offline.
</p>

<p align="center"><b>Built by Yasir 🇸🇦</b></p>

<p align="center">
  <a href="https://github.com/yasirmz/1v1-Timer/releases/latest/download/1v1timer-setup.exe"><img src="https://img.shields.io/badge/Download-Windows%2010%20%7C%2011-B41A23?style=for-the-badge&logo=windows&logoColor=E4DECB&labelColor=170F10" alt="Download for Windows"></a>
  <img src="https://img.shields.io/badge/license-GPL--2.0-3A2A2D?style=for-the-badge&labelColor=170F10" alt="License: GPL-2.0">
  <img src="https://img.shields.io/badge/languages-7-3A2A2D?style=for-the-badge&labelColor=170F10" alt="7 languages">
</p>

<p align="center">
  <b>English</b> ·
  <a href="docs/README.ar.md">العربية</a> ·
  <a href="docs/README.zh-Hans.md">简体中文</a> ·
  <a href="docs/README.ru.md">Русский</a> ·
  <a href="docs/README.es.md">Español</a> ·
  <a href="docs/README.pt-BR.md">Português (Brasil)</a> ·
  <a href="docs/README.de.md">Deutsch</a>
</p>

## Why 1v1 Timer

- **Two timers, side by side.** Timer 1 on the left, Timer 2 on the right, always on top of the game. Seconds with two decimals (12.34), and minutes from a minute on (1:33.45).
- **Hotkeys that take nothing.** The timer only listens: the game still gets every key, mouse button and controller button you press.
- **Keyboard, mouse or controller.** Each action has a key or mouse button, and a controller button of its own.
- **Know when it's close.** Timer 2 turns orange in the last 20 seconds before it reaches Timer 1's time.
- **Never in the way.** It never takes the focus from your game. Drag it anywhere, and drag its edges or corner to make it bigger or smaller.
- **Light on your game.** It never uses the graphics card, next to no CPU while the timers are stopped, and under 1% of the CPU while one runs.
- **Private.** No account, no telemetry. It goes online only to look for updates.

## Download

1. Download **1v1timer-setup.exe** from the [latest release](https://github.com/yasirmz/1v1-Timer/releases/latest).
2. Run it and follow the steps: pick your language and the folder (no administrator rights needed), and whether you want a desktop shortcut and the timer to start with Windows (off unless you tick it).
3. Start a match and press the hotkeys below.

Windows may warn about an unknown publisher (the installer is not signed yet): choose **More info**, then **Run anyway**.
Updates come from inside the app, when you press Update. Uninstalling keeps your settings.

**Needs:** Windows 10 (2004 or newer) or Windows 11, 64-bit.

## How it works

| Action | Keyboard / mouse | Controller |
|---|---|---|
| Start / Stop / Reset | Mouse Back | A |
| Start / Stop | H | B |
| Select Timer 1 | T | D-pad Left |
| Select Timer 2 | Y | D-pad Right |

- **Start / Stop / Reset** works on the selected timer: running, it stops; stopped with time on it, it goes back to 0.00; at 0.00, it starts.
- **Start / Stop** stops the selected timer, or starts it again from its time.
- **Start Timer On Change** (in Settings): switching to the other timer stops the one you leave and starts the new one at the same moment, like a chess clock.
- **Colors:** Timer green, Selected Timer blue, Last 20 Seconds orange, Background black. Pick any color in Settings and see it on the timer at once.
- **Transparent Background** shows only the numbers, with a thin outline. **Clickthrough** lets clicks pass through the timer to the game; it is always off when the timer starts, and the tray icon turns it off.
- Right-click the timer, or the icon by the clock, for **Settings**, **Clickthrough** and **Exit**. Starting the app again also opens Settings.
- Every setting applies at once and is kept in `%AppData%\1v1 Timer\settings.json`, with the timer's place and size.

## Games and fullscreen

- **Use Borderless or Windowed mode.** A game in exclusive fullscreen owns the whole screen, and Windows draws no other window over it, so no overlay can show there. Borderless (or "fullscreen windowed") looks the same and shows the timer.
- **Games run as administrator:** if a game runs as administrator, Windows may keep its keys from reaching a timer that does not. Start 1v1 Timer as administrator too, or use the controller, which works either way.
- **Controllers:** Xbox, PlayStation (DualSense, DualShock 4) and most other controllers, with no extra app. On a PlayStation controller, Cross is A, Circle B, Square X and Triangle Y.

## Anti-cheat (Vanguard, Easy Anti-Cheat, VAC)

**Can 1v1 Timer get my Valorant, Fortnite or Steam account banned?**

1v1 Timer does none of the things anti-cheat bans for. It is a program of its own with its own window, like Discord or OBS, and it never touches a game:

- It never opens, reads or changes a game's memory, and never puts any code into a game.
- It never presses a key, clicks or presses a controller button for you. It only listens to presses, as Windows lets any app, and the game still gets every one.
- It installs no driver and needs no administrator rights.
- It shows two stopwatches; it does nothing in the game for you.

No anti-cheat lets an app sign itself up as safe, so nobody but Riot, Epic or Valve can speak for them. But a stopwatch that never touches the game is not what they look for. This is checked on every change: the project's [anti-cheat tests](tests/OneVOneTimer.Tests/AntiCheatTests.cs) list every Windows function the timer uses and fail if one for reading another program's memory, putting code into it or faking input is ever added.

Download 1v1 Timer only from this page's [releases](https://github.com/yasirmz/1v1-Timer/releases): a copy from anywhere else could have been changed.

## Privacy

- No telemetry, no analytics, no account.
- 1v1 Timer goes online only to check GitHub for a newer version (once a day, and you can turn that off) and to download it when you press Update.
- The hotkeys are read on your PC only. Nothing you type is kept or sent anywhere: the timer only compares each key with its own hotkeys.
- Found a security problem? Please report it privately: see [SECURITY.md](SECURITY.md).

## Build from source

<details>
<summary>For developers</summary>

1v1 Timer is a .NET 10 app (WPF and WinForms), with the look, settings, colour picker, tray menu and languages of [Capture](https://github.com/yasirmz/Capture).

```powershell
dotnet build src\OneVOneTimer              # the app
dotnet test tests\OneVOneTimer.Tests       # the tests
.\tools\release.ps1                        # the Setup, update files and source zip in releases\ (needs vpk 1.2.161 and Inno Setup 6)
```

Releases are built and published by the **Release build** workflow (Actions → Run workflow).

</details>

## License

1v1 Timer is free software under the [GNU General Public License v2.0 or later](LICENSE). Every release includes its complete source code. Third-party components and their licenses are listed in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

<sub>1v1 Timer is based on [Kairos](https://github.com/BasilForge/Kairos) by BasilForge, under the same license.</sub>
