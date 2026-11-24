# 埃夫拉温

**Diller / Languages:** [TR](README.md) · [EN](README.en.md) · [ES](README.es.md) · [RU](README.ru.md) · [DE](README.de.md) · [中文](README.zh.md) · [日本語](README.ja.md)

设置新的 Windows 计算机时，Efla Win 在单个窗口中收集应用程序下载、驱动程序控制和常用系统设置。使用 WinUI 3 开发。

<img src="Assets/efla.png" alt="Efla Win logosu" width="72" height="72">

## 开始

在 64 位 Windows 11 中打开 `Efla Win 1.0.0.exe` 文件并获得管理员批准。无需安装包。 .NET 和 Windows App SDK 组件包含在 EXE 中，并在首次运行时打开到本地缓存。下载和 Windows 更新扫描需要互联网连接。

### 下载并安装应用程序

1. 从**推荐**开始或在**所有应用程序**屏幕上选择类别。 **Ctrl+F** 聚焦搜索框。
2. 对于单个应用程序，请使用 **下载** 按钮。要排队几个，请选中徽标旁边的框，然后点击 **下载所选** 按钮。
3. 默认情况下，下载的文件将写入 Windows 下载文件夹中的 `Efla Win` 目录。您可以使用底部栏中的**更改**选择另一个文件夹，并使用**打开文件夹**查看文件。
4. 下载完成后，**安装** 按钮将打开安装程序。当 Windows 报告应用程序已安装时，会出现 **卸载** 按钮，确认后，应用程序的删除工具将打开。

卡右键菜单中的 **开源** 选项可打开目录中的分发页面。

### 跟踪下载和浏览

即使您切换页面，左侧菜单中的**可用操作**也会显示下载队列和驱动程序扫描。显示下载期间接收到的数据量。如果服务器报告总大小，也会显示出来。进程旁边的 ****** 按钮会停止该作业。

当窗口最小化时，它会转到通知区域。您可以从 Efla Win 图标的右键菜单中打开窗口、下载文件夹或内存屏幕。 **退出**关闭应用程序。

### 编辑窗口

| 屏幕 | 你能做什么？ |
| --- | --- |
| **Windows 设置** | 检查搜索、任务栏、隐私、性能和文件资源管理器选项。跳转到启动应用程序、存储、网络和代理页面。 |
| **游戏设置** | 查看游戏模式、后台录制和 Xbox 游戏栏选项。打开图形首选项和电源计划。 |
| **硬件** | 查看您的处理器、显卡、Windows 版本、RAM 使用情况和可用磁盘空间。 |
| **司机** | 重点介绍基于设备的 Intel、AMD 和 NVIDIA 工具。扫描 Windows 更新中提供的驱动程序，然后打开 **可选更新** 屏幕进行安装。 |
| **记忆** | 查看当前 RAM 使用情况。 **清除**以减少本次会话中符合条件的应用程序的工作集。 |
| **应用程序设置** | 系统调整浅色或深色主题、突出显示颜色、字体大小、粗体文本、对比度和运动减少。 |

如果设置旁边显示“**锁定**”，则信息按钮会显示 Windows 强制执行的策略和保存路径。 **不可用**表示此版本的 Windows 或计算机不支持该选项。某些设置仅在您再次登录时才会反映。

## 从源代码编译

.NET 10 SDK 和 Windows SDK 必须安装在 Windows 上。打开源存档并在包含 `EflaWin.csproj` 文件的文件夹中运行 PowerShell。

```powershell
dotnet publish .\EflaWin.csproj -c Release -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true -p:EnableCompressionInSingleFile=true
```

广播输出位于路径 `bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\EflaWin.exe` 中。该 EXE 足以用于分发。

## 反馈

报告错误时，请包括 Windows 版本、发生错误的屏幕以及要重复的步骤。如果是下载问题，还请提供应用程序名称和卡上显示的错误文本。 EFLAKEEP000Token

## 资源和许可

在准备 Windows 选项时，检查了 [Optimizer](https://github.com/hellzerg/optimizer)、[Sparkle](https://github.com/thedogecraft/sparkle)、[ReviOS Playbook](https://github.com/meetrevision/playbook)、[GTweak](https://github.com/Greedeks/GTweak) 和 [vacpp/windows11](https://github.com/vacpp/windows11)。通知区域菜单界面以[TranslucentTB](https://github.com/TranslucentTB/TranslucentTB)为例。

© 2026 BLGN 工作室。使用条款位于 [LICENSE](LICENSE) 文件中。目录中的品牌和应用程序属于其各自所有者。

由❤️ [yefeblgn](https://yefeblgn.net) 制作
由 BLGN 工作室提供支持
