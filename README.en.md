# Efla Win

**Diller / Languages:** [TR](README.md) · [EN](README.en.md) · [ES](README.es.md) · [RU](README.ru.md) · [DE](README.de.md) · [中文](README.zh.md) · [日本語](README.ja.md)

Efla Win collects application downloads, driver control and frequently used system settings in a single window when setting up a new Windows computer. Developed with WinUI 3.

<img src="Assets/efla.png" alt="Efla Win logosu" width="72" height="72">

## Beginning

Open the `Efla Win 1.0.0.exe` file in 64-bit Windows 11 and give administrator approval. No installation package required. .NET and Windows App SDK components are included in the EXE and are decompressed into the local cache on first run. Downloads and Windows Update scanning require an internet connection.

### Download and install apps

1. Start with **Recommended** or select the category on the **All apps** screen. **Ctrl+F** focuses the search box.
2. For a single app, use the **Download** button. To queue up a few, check the boxes next to the logo and hit the **Download selected** button.
3. By default, downloaded files are written to the `Efla Win` directory in Windows' Downloads folder. You can select another folder with **Change** in the bottom bar, and see the files with **Open folder**.
4. Once the download is complete, the **Install** button opens the installer. When Windows reports the application as installed, the **Uninstall** button appears and after its confirmation, the application's removal tool opens.

The **Open source** option in the right-click menu of a card opens the distribution page in the catalogue.

### Track downloads and browsing

**Available Actions** in the left menu shows the download queue and driver scan even if you switch pages. The amount of data received during downloading appears. If the server reports the total size, that will also be shown. The ****** button next to a process stops that job.

When the window is minimized it goes to the notification area. You can open the window, download folder or memory screen from the right-click menu of the Efla Win icon. **Exit** closes the application.

### edit windows

| Screen | What can you do? |
| --- | --- |
| **Windows settings** | Check search, taskbar, privacy, performance and File Explorer options. Jump to the startup apps, storage, network, and proxy pages. |
| **Game settings** | Check out Game Mode, background recording, and Xbox Game Bar options. Open graphics preferences and power plan. |
| **Hardware** | See your processor, graphics card, Windows version, RAM usage and free disk space. |
| **Drivers** | Highlights Intel, AMD and NVIDIA tools based on device. Scan for drivers offered in Windows Update and open the **Optional updates** screen for installation. |
| **Memory** | See current RAM usage. **Purge** to reduce the working sets of eligible applications in this session. |
| **Application settings** | System adjust light or dark theme, highlight color, font size, bold text, contrast and motion reduction. |

If a setting says **Locked** next to it, the information button shows the policy and save path that Windows enforces. **Unavailable** indicates that the option is not supported on this version of Windows or computer. Some settings are only reflected when you log in again.

## Compile from source

.NET 10 SDK and Windows SDK must be installed on Windows. Open the source archive and run PowerShell in the folder containing the `EflaWin.csproj` file.

```powershell
dotnet publish .\EflaWin.csproj -c Release -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true -p:EnableCompressionInSingleFile=true
```

The broadcast output is in the path `bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\EflaWin.exe`. This EXE is sufficient for distribution.

## Feedback

When reporting an error, include the Windows version, the screen where the error occurred, and the steps to repeat. If it's a download problem, also include the app name and the error text that appears on the card. [GitHub](https://github.com/yefeblgn/eflawin)

## Resources and licensing

While preparing Windows options, [Optimizer](https://github.com/hellzerg/optimizer), [Sparkle](https://github.com/thedogecraft/sparkle), [ReviOS Playbook](https://github.com/meetrevision/playbook), [GTweak](https://github.com/Greedeks/GTweak) and [vacpp/windows11](https://github.com/vacpp/windows11) were examined. [TranslucentTB](https://github.com/TranslucentTB/TranslucentTB) was taken as an example in the interface of the notification area menu.

© 2026 BLGN Studios. Terms of use are in the [LICENSE](LICENSE) file. Brands and applications in the catalog belong to their respective owners.

made by ❤️ [yefeblgn](https://yefeblgn.net)
Powered by BLGN Studios
