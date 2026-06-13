# Packaging MacFontRenderer

This project ships a Windows installer using Inno Setup.

## Build locally

Install prerequisites:

- .NET 8 SDK
- Inno Setup 6

Build the app, service, portable ZIP, and installer:

```powershell
.\scripts\package.ps1 -Version 1.0.0
```

Outputs:

- `dist\MacFontRenderer-portable-1.0.0-win-x64.zip`
- `dist\installer\MacFontRenderer-Setup-1.0.0.exe`

## Bundled assets

The installer bundles:

- `MacFontRenderer.exe`
- `MacFontRenderer.Service.exe`
- `fonts\`
- `fonts\FontMod64.dll` as `C:\ProgramData\MacFontRenderer\Hooks\FontMod.dll`
- `fonts\FontMod.yaml.txt` as `C:\ProgramData\MacFontRenderer\FontMod.yaml`

The bundled Apple SF/New York fonts and FontMod hook DLL require explicit licensing and security review before public distribution.

## Recommended release flow

1. Run `.\scripts\package.ps1 -Version x.y.z`.
2. Sign `MacFontRenderer.exe`, `MacFontRenderer.Service.exe`, and the installer.
3. Upload the installer and SHA256 hash to GitHub Releases.
4. Test install, service start, app launch, restore, and uninstall on a clean Windows VM.
