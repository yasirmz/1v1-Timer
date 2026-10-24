<p align="center">
  <img src="media/logo.png" alt="1v1 Timer" width="160">
</p>

<h3 align="center">为 1v1 对局准备的两个秒表，显示在游戏上方。</h3>

<p align="center">
  用一个按键、鼠标按键或手柄开始、停止和切换计时器，游戏仍会收到每一次按键。<br>
  免费、开源、离线可用。
</p>

<p align="center">
  <a href="https://github.com/yasirmz/1v1-Timer/releases/latest/download/1v1timer-setup.exe"><img src="https://img.shields.io/badge/Download-Windows%2010%20%7C%2011-B41A23?style=for-the-badge&logo=windows&logoColor=E4DECB&labelColor=170F10" alt="Download for Windows"></a>
  <img src="https://img.shields.io/badge/license-GPL--2.0-3A2A2D?style=for-the-badge&labelColor=170F10" alt="License: GPL-2.0">
  <img src="https://img.shields.io/badge/languages-7-3A2A2D?style=for-the-badge&labelColor=170F10" alt="7 languages">
</p>

<p align="center">
  <a href="../README.md">English</a> ·
  <a href="README.ar.md">العربية</a> ·
  <b>简体中文</b> ·
  <a href="README.ru.md">Русский</a> ·
  <a href="README.es.md">Español</a> ·
  <a href="README.pt-BR.md">Português (Brasil)</a> ·
  <a href="README.de.md">Deutsch</a>
</p>

## 为什么选择 1v1 Timer

- **两个计时器并排显示。** 计时器 1 在左，计时器 2 在右，始终显示在游戏上方。秒数带两位小数（12.34），满一分钟后显示分钟（1:33.45）。
- **快捷键不占用任何按键。** 计时器只是监听：你按下的每个键盘按键、鼠标按键和手柄按钮，游戏都照样收到。
- **键盘、鼠标或手柄。** 每个操作都有一个键盘或鼠标按键，以及一个独立的手柄按钮。
- **快到时一眼就知道。** 计时器 2 在达到计时器 1 的时间之前的最后 20 秒会变成橙色。
- **从不碍事。** 它从不抢走游戏的焦点。可以拖到任何位置，拖动边缘或角可放大或缩小。
- **注重隐私。** 无需账号，没有遥测。只在检查更新时联网。

## 下载

1. 从[最新版本](https://github.com/yasirmz/1v1-Timer/releases/latest)下载 **1v1timer-setup.exe**。
2. 运行并按步骤操作：选择语言和文件夹（无需管理员权限），以及是否创建桌面快捷方式、是否开机时启动（默认不勾选）。
3. 开始对局，按下面的快捷键。

Windows 可能会提示发布者未知（安装程序尚未签名）：请选择 **更多信息**，然后选择 **仍要运行**。
更新在应用内进行，在你点击“更新”时安装。卸载会保留你的设置。

**系统要求：** Windows 10（2004 或更高版本）或 Windows 11，64 位。

## 使用方法

| 操作 | 键盘 / 鼠标 | 手柄 |
|---|---|---|
| 开始 / 停止 / 归零 | 鼠标后退键 | A |
| 开始 / 停止 | H | B |
| 选择计时器 1 | T | 十字键左 |
| 选择计时器 2 | Y | 十字键右 |

- **开始 / 停止 / 归零** 作用于选中的计时器：正在运行时停止；已停止且有时间时归零到 0.00；在 0.00 时开始。
- **开始 / 停止** 停止选中的计时器，或从它的时间继续计时。
- **切换时开始计时**（在设置中）：切换到另一个计时器时，离开的计时器停止，新的计时器在同一时刻开始，就像棋钟一样。
- **颜色：** 计时器为绿色，选中的计时器为蓝色，最后 20 秒为橙色，背景为黑色。可在设置中选择任何颜色，计时器上会立即显示。
- **透明背景** 只显示数字，并带有细描边。**点击穿透** 让点击穿过计时器传到游戏；计时器每次启动时总是关闭的，可在托盘图标中关闭。
- 右键单击计时器或时钟旁的图标，可打开 **设置**、**点击穿透** 和 **退出**。再次启动应用也会打开设置。
- 所有设置立即生效，并与计时器的位置和大小一起保存在 `%AppData%\1v1 Timer\settings.json` 中。

## 游戏与全屏

- **请使用无边框或窗口模式。** 独占全屏的游戏占用整个屏幕，Windows 不会在它上面绘制其他窗口，因此任何悬浮窗都无法显示。无边框（或“全屏窗口化”）看起来一样，并且会显示计时器。
- **以管理员身份运行的游戏：** 如果游戏以管理员身份运行，Windows 可能不会把它的按键交给未以管理员身份运行的计时器。请也以管理员身份运行 1v1 Timer，或使用手柄——手柄在两种情况下都能用。
- **手柄：** Xbox 手柄以及任何像它一样工作的手柄（XInput）。PlayStation 手柄可通过 Steam Input 或 DS4Windows 使用。

## 隐私

- 没有遥测，没有统计，没有账号。
- 1v1 Timer 只在两种情况下联网：在 GitHub 上检查是否有新版本（每天一次，可以关闭），以及在你点击“更新”时下载它。
- 快捷键只在你的电脑上读取。你输入的内容不会被保存或发送到任何地方：计时器只会把每个按键与自己的快捷键进行比较。
- 发现安全问题？请私下报告：见 [SECURITY.md](../SECURITY.md)。

## 从源代码构建

<details>
<summary>开发者</summary>

1v1 Timer 是一个 .NET 10 应用（WPF 和 WinForms），使用 [Capture](https://github.com/yasirmz/Capture) 的外观、设置、颜色选择器、托盘菜单和语言。

```powershell
dotnet build src\OneVOneTimer              # 应用
dotnet test tests\OneVOneTimer.Tests       # 测试
.\tools\release.ps1                        # 在 releases\ 中生成安装程序、更新文件和源代码包（需要 vpk 1.2.161 和 Inno Setup 6）
```

版本由 **Release build** 工作流构建并发布（Actions → Run workflow）。

</details>

## 许可证

1v1 Timer 是自由软件，采用 [GNU 通用公共许可证 2.0 版或更高版本](../LICENSE)。每个版本都附带完整的源代码。第三方组件及其许可证列在 [THIRD-PARTY-NOTICES.md](../THIRD-PARTY-NOTICES.md) 中。

<sub>1v1 Timer 基于 BasilForge 的 [Kairos](https://github.com/BasilForge/Kairos)，采用相同的许可证。</sub>
