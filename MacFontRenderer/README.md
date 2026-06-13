# MacFontRenderer for Windows

A lightweight, native Windows WPF application that optimizes font rendering to match macOS grayscale antialiasing and typography thickness on 4K/High-DPI displays at 100% scale.

## Features

- **GDI Rendering Optimization**: Modifies Windows ClearType settings (FontSmoothing, FontSmoothingType, FontSmoothingGamma)
- **DirectWrite Environment Injection**: Sets system-level environment variables for modern apps
- **System-Wide Font Substitution**: Maps Segoe UI variants to SF Pro Text or Inter font
- **Dynamic Weight Control**: Gamma slider to adjust font thickness (336-1000)
- **Safe Rollback**: One-click restore to Windows defaults with backup protection
- **System Status Monitoring**: Live status indicator and detailed operation log

## Requirements

- **Windows 10 / 11** (64-bit recommended)
- **.NET 8.0 Runtime** (or bundled with installer)
- **Administrator Privileges** (required for registry modifications)

## Building from Source

### Prerequisites
- Visual Studio 2022 or VS Code with C# extension
- .NET 8.0 SDK
- Git (optional)

### Build Steps

1. **Clone or navigate to the project**:
   ```powershell
   cd MacFontRenderer
   ```

2. **Restore dependencies**:
   ```powershell
   dotnet restore
   ```

3. **Build the application**:
   ```powershell
   dotnet build -c Release
   ```

4. **Run the application** (must be Admin):
   ```powershell
   # Right-click PowerShell and select "Run as Administrator"
   cd MacFontRenderer\bin\Release\net8.0-windows
   .\MacFontRenderer.exe
   ```

### Publish as Self-Contained Executable

For a standalone executable that doesn't require .NET SDK:

```powershell
dotnet publish -c Release -r win-x64 --self-contained
```

Output will be in `bin\Release\net8.0-windows\publish\`.

## Usage

1. **Run as Administrator**: The application will check for admin privileges on startup.

2. **Check System State**: The app automatically detects installed fonts (SF Pro Text or Inter) and current optimization status.

3. **Select Target Font**: Choose between SF Pro Text (macOS default) or Inter (open-source alternative).

4. **Adjust Font Weight**: Use the Gamma slider:
   - **336** = Extra Heavy (macOS Big Sur style)
   - **1000** = Standard (Recommended)

5. **Apply macOS Style**: Click "Apply macOS Style" to:
   - Update GDI rendering engine settings
   - Inject DirectWrite environment variables
   - Broadcast system settings change
   - Map system fonts in registry

6. **Restore Defaults**: Click "Restore Defaults" to safely revert all changes.

## Architecture

### Phase 1: User-Mode Safe Implementation (Current)
- Registry tweaks (HKCU and HKLM)
- Environment variable injection
- System font mapping
- Full rollback capability
- No DLL injection or kernel-level changes

### Phase 2 (Future): Per-Process Injection
- Chromium/Electron app-specific overrides
- Opt-in per-application settings
- Signed service-based injection

### Phase 3 (Future): Advanced Options
- Kernel-mode font redirection (requires signed driver)
- Early-boot/login screen font changes
- Full system-level persistent hooks

## Technical Details

### Registry Keys Modified

**HKEY_CURRENT_USER:**
- `Control Panel\Desktop\FontSmoothing` → "2"
- `Control Panel\Desktop\FontSmoothingType` → 2
- `Control Panel\Desktop\FontSmoothingGamma` → 1000 (or selected value)

**HKEY_LOCAL_MACHINE:**
- `SYSTEM\CurrentControlSet\Control\Session Manager\Environment\GDIP_FONT_SMOOTHING` → "2"
- `SYSTEM\CurrentControlSet\Control\Session Manager\Environment\DWRITE_FONT_SMOOTHING` → "2"
- `SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts\Segoe UI (TrueType)` → target font filename

### Windows APIs Used
- `RegOpenKeyEx` / `RegSetValueEx` for registry access
- `SendMessageTimeout` for WM_SETTINGCHANGE broadcast
- `AddFontResourceEx` / `RemoveFontResourceEx` for font installation
- `IsUserAnAdmin` for privilege elevation check

## Safety & Rollback

- All registry modifications are wrapped in try/catch blocks
- Backup values stored before changes
- "Restore Defaults" button completely reverses all modifications
- System restart recommended but not required for user-mode changes
- No system-critical DLL injection (Phase 1)

## Troubleshooting

### "Admin Privileges Required"
- Right-click the executable and select "Run as administrator"
- Or launch from elevated PowerShell

### Font Changes Not Applied
- Ensure target font (SF Pro Text or Inter) is installed
- Restart the application after installation
- Full effects visible after system restart

### Restore Isn't Working
- Run "Restore Defaults" as administrator
- If issues persist, manually restore from Control Panel > Fonts

## Licensing

This utility is provided as-is for personal use on your own systems. Font licensing depends on the target font selected:
- **SF Pro Text**: Apple copyright; included in macOS
- **Inter**: Open License (OFL); free for personal and commercial use

## Legal Notes

- Modifying system registry can affect system stability
- Test in isolated VMs before wide deployment
- Create system backups before use
- Use at your own risk on non-critical machines
- Respect font licensing and usage terms

## Future Enhancements

- Automated font download and installation UI
- Per-application font override profiles
- Scheduled optimization profiles
- Integration with Windows Theme settings
- Batch configuration for enterprise deployments

---

**Version**: 1.0.0  
**Last Updated**: 2026-06-12  
**Status**: Phase 1 - Stable User-Mode Implementation
