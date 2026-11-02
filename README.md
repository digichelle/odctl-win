# ODCtl
OneDrive command-line control for Windows, written in C#.

## Description

This program adds command-line control to the official OneDrive for Windows client. It requires the latest version of the official client to function.

It’s written in C# because ~~I love pain~~ it was a good excuse to learn the language. If that seems like overkill, that’s because it is. A simpler [batch file variant](https://github.com/digichelle/odctl-cmd) exists, and that may be all you need.

## Why This Exists

Storing development project files on OneDrive is probably a bad idea. The constant creation and deletion of build artifacts wreaks havoc on the service. If you do it anyway (as I do), I strongly suggest quitting OneDrive before working on a development project.

This little program helps with that. No need to go all the way to the system tray or the Start menu. Instead, you can stay in your comfy Command Prompt or PowerShell window. Run `odctl stop` to quit OneDrive and `odctl start` to start it. Easy!

Some non-developers may benefit from this as well. For example, [Obsidian](https://obsidian.md/) users may wish to quit OneDrive before using the app. The app’s auto-save functionality is a touch aggressive and can overwhelm OneDrive.

Unlike the batch file variant (and batch files in general), this program can execute anywhere, not just in Command Prompt or PowerShell. For example, it can run in [MSYS2](http://www.msys2.org/) environments.

## Requirements

### Build and Run

- Windows 10 or higher (.NET SDK 10.0) or Windows 11 (Visual Studio 2026)
- [OneDrive for Windows](https://www.microsoft.com/en-us/microsoft-365/onedrive/download)
- [.NET SDK 10.0](https://dotnet.microsoft.com/en-us/) or [Visual Studio 2026](https://visualstudio.microsoft.com) with the [.NET desktop development workload](https://learn.microsoft.com/en-us/visualstudio/install/modify-visual-studio?view=visualstudio)

### Run Only

- Windows 10 or higher
- OneDrive for Windows
- [.NET 10.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0/runtime)

## Build and Install

Open Command Prompt or PowerShell, `cd` to the project’s root directory if you’re not already there, and run the following command:

```
dotnet publish odctl-win.csproj /p:PublishProfile=<PROFILE>
```

Alternatively, if you use Visual Studio, you can open the `odctl-win.slnx` solution file. Once open, right-click the `odctl-win` project in Solution Explorer, then click Publish. Select your desired publish profile, then click the Publish button.

Possible profiles are `win-arm64`, `win-x64`, and `win-x86`. The compiled executable, `odctl.exe`, will be placed in `.\bin\Release\net10.0-windows\publish\<PROFILE>`.

To install, copy `odctl.exe` to a directory in your PATH.

## Usage

### Start OneDrive

```
odctl start
```

### Stop OneDrive

```
odctl stop
```

### Restart OneDrive

```
odctl restart
```

### Show OneDrive Process Status

```
odctl status
```

### Help

```
odctl /?
```

## License

### MIT License

Copyright © 2026 Michelle Catherine Marcó

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
