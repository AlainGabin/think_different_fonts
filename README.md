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

The project currently bundles Apple SF/New York font files and a `FontMod` hook DLL because the project owner explicitly accepted that licensing and security risk. Review those assets before redistributing publicly.

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
- Inno Setup 6, only required for building the installer

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

Do not commit `dist\`, `artifacts\`, `bin\`, or `obj\`. They are generated build outputs and are ignored by `.gitignore`.

## GitHub Release Process

Commit source and packaging files only:

```powershell
git add .gitignore README.md PACKAGING.md installer scripts MacFontRenderer MacFontRenderer.Service fonts
git commit -m "Add installer packaging and release README"
```

Then upload the generated files from `dist\` to a GitHub Release:

```text
MacFontRenderer-Setup-1.0.0.exe
MacFontRenderer-portable-1.0.0-win-x64.zip
SHA256SUMS.txt
```

## Security and Signing

The current installer is not code-signed. Windows SmartScreen may warn users.

For a public production release, sign:

- `MacFontRenderer.exe`
- `MacFontRenderer.Service.exe`
- `MacFontRenderer-Setup-1.0.0.exe`
- any DLLs shipped with the installer

Use an Authenticode code signing certificate and publish SHA256 checksums with each release.

## Repository Notes

The `.gitignore` file is intentionally committed. It prevents generated installers, build outputs, local configs, keys, logs, and temporary files from being added to Git history.

Generated release artifacts belong in GitHub Releases, not in the repository.

## License and Asset Notice

Code licensing should be finalized before public distribution.

Bundled assets may have separate licenses:

- Inter is distributed under the SIL Open Font License.
- Apple SF/New York fonts are proprietary Apple assets.
- `FontMod64.dll` should be reviewed, verified, and signed before broad distribution.

Do not redistribute assets unless you have the right to do so.
