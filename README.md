# MacFontRenderer for Windows

MacFontRenderer is a Windows desktop utility for tuning system font rendering and font substitution so Windows text can look closer to macOS-style typography on high-DPI displays.

The app provides a WPF control panel, a background service framework, bundled font assets, and an installer package for Windows x64.

> Important: this project changes Windows registry values related to font rendering and system font mappings. Run it only on systems you control, and create a restore point before testing on a primary machine.

## Download

For normal users, use the installer from GitHub Releases:

1. Open the latest release.
2. Download `MacFontRenderer-Setup-1.0.0.exe`.
3. Run the installer as administrator.
4. Launch MacFontRenderer from the Start menu.
5. Choose a font and apply the optimization from the app.

The portable ZIP is also provided for advanced users, but the installer is the recommended path.

## What It Does

MacFontRenderer can apply several Windows font rendering changes:

- Adjusts ClearType/GDI font smoothing values.
- Sets DirectWrite-related environment values.
- Maps Segoe UI system font entries to bundled replacement fonts.
- Deploys bundled font assets to a runtime data folder.
- Installs a background service framework for future per-process font handling.
- Provides a restore action to return common Windows font settings to defaults.

The installer prepares the app, service, and assets. The app itself performs the system changes when the user applies them.

## Safety Notes

This software requires administrator privileges because it writes to system registry locations and deploys files under `ProgramData`.

Before using it:

- Create a Windows restore point.
- Test on a VM or non-critical PC first.
- Keep the installer available so you can uninstall cleanly.
- Use the app's restore option before uninstalling if you applied font changes.


## System Requirements

- Windows 10 or Windows 11
- x64 processor
- Administrator account
- No separate .NET runtime required when using the installer

## Installed Locations

The installer uses these locations:

```text
C:\Program Files\MacFontRenderer\
C:\Program Files\MacFontRenderer\Service\
C:\ProgramData\MacFontRenderer\
C:\ProgramData\MacFontRenderer\Assets\
C:\ProgramData\MacFontRenderer\Hooks\
```

## Registry Areas Used

MacFontRenderer may write to:

```text
HKCU\Control Panel\Desktop
HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment
HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts
HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows
HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows NT\CurrentVersion\Windows
```

These locations control font smoothing, environment settings, font mappings, and AppInit hook behavior.

## Restore and Uninstall

Recommended restore flow:

1. Open MacFontRenderer as administrator.
2. Click the restore/defaults action in the app.
3. Reboot or sign out/sign in if font changes remain visible.
4. Uninstall MacFontRenderer from Windows Settings.

The uninstaller stops and removes the `MacFontRendererService` service.

## Build From Source

Prerequisites:

- .NET 8 SDK


Build the app and service:

```powershell
dotnet build .\MacFontRenderer\MacFontRenderer.csproj -c Release
dotnet build .\MacFontRenderer.Service\MacFontRenderer.Service.csproj -c Release
```

Build release packages:

```powershell
.\scripts\package.ps1 -Version 1.0.0
```

Outputs:

```text
dist\installer\MacFontRenderer-Setup-1.0.0.exe
dist\MacFontRenderer-portable-1.0.0-win-x64.zip
dist\SHA256SUMS.txt
```