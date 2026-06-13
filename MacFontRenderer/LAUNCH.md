# MacFontRenderer - Launch Guide

## Quick Start

### Windows Explorer (Easiest)
1. Navigate to: `<repo-root>\MacFontRenderer`
2. Double-click **RUN.bat**
3. If prompted by UAC, click "Yes" to confirm admin access

### PowerShell (Advanced)
```powershell
# Option 1: Run launch script
cd '<repo-root>\MacFontRenderer'
.\RUN.ps1

# Option 2: Run directly
$exe = "<repo-root>\MacFontRenderer\bin\Release\net8.0-windows\MacFontRenderer.exe"
Start-Process $exe -Verb RunAs
```

### From Any Terminal (After Setup)
```powershell
# First time only: Add dotnet to PATH
cd '<repo-root>\MacFontRenderer'
.\Setup-DotNet.ps1

# Then from any terminal:
cd MacFontRenderer
dotnet run --project MacFontRenderer.csproj
```

## Rebuilding After Changes

```powershell
cd '<repo-root>\MacFontRenderer'

# Debug build
dotnet build

# Release build (optimized)
dotnet build -c Release

# Publish as standalone executable
dotnet publish -c Release -r win-x64 --self-contained
```

## Creating a Standalone Installer (Optional)

For distribution without requiring .NET SDK:

```powershell
# Create self-contained executable
dotnet publish -c Release -r win-x64 --self-contained

# Output in: bin/Release/net8.0-windows/publish/
# Copy all files to create installer

# Or use MSI Toolset:
# Install WiX Toolset from https://wixtoolset.org/
# Create MacFontRenderer.wxs (setup file) and build MSI
```

## Application Features (macOS Style UI)

### Design Elements
- **Clean Minimalism**: Modern typography with ample whitespace
- **Monochrome Palette**: Apple-inspired grays and blues (#1D1D1D, #86868B, #0071E3)
- **Rounded Corners**: 12px borders on cards, smooth transitions
- **Status Indicator**: Live green/gray dot showing optimization state
- **Blur Effect**: Transparent window with modern border styling

### UI Sections

#### Header
- **Title**: "Font Renderer" (24pt, 600 weight)
- **Subtitle**: Optimization description in secondary text color

#### Status Card
- Live indicator showing current optimization state
- "Optimization active" or "No optimization applied yet"
- Real-time system check on startup

#### Settings
- **Font Selection**: Dropdown for SF Pro Text or Inter
- **Font Weight Slider**: Gamma control (336-1000)
  - 336 = Heavy (macOS Big Sur style)
  - 1000 = Standard (Recommended)
- Real-time gamma value display

#### Action Buttons
- **Apply Changes**: Blue button (#0071E3) - applies all optimizations
- **Restore Defaults**: Red button (#FF3B30) - safely reverts changes
- Hover effects for visual feedback

#### Alerts
- **Warning Box**: Subtle yellow background with important info
- **Activity Log**: Monospace font, timestamped operations
- Real-time output of all system changes

## Understanding the Logs

Each operation is timestamped and shows:
- ✓ Successful operations
- ❌ Errors with descriptions
- ① ② ③ ④ Step indicators for multi-step operations

Example log output:
```
[14:32:15] Checking system state...
[14:32:15] ✓ SF Pro Text font detected
[14:32:16] System check complete.

[14:33:02] ━━━ Applying macOS Font Style ━━━
[14:33:02] Target: SF Pro Text | Gamma: 1000
[14:33:02] ...
[14:33:02] ① Applying GDI tweaks...
[14:33:02] ✓ GDI tweaks applied
[14:33:02] ✓ Optimization Complete
[14:33:02] Restart recommended for full effect.
```

## Troubleshooting

### "Admin Privileges Required"
- Right-click RUN.bat and select "Run as administrator"
- Or use RUN.ps1 with elevated PowerShell

### App Won't Launch
- Verify .NET 8.0 Runtime is installed
- Check file location: `bin/Release/net8.0-windows/MacFontRenderer.exe`
- Rebuild: `dotnet build -c Release`

### Changes Not Taking Effect
- Ensure system restart is performed
- Check Activity Log for error messages
- Verify target font is installed on system

### Fonts Not Detected
- Install SF Pro Text or Inter font files
- Restart application after installation
- Check Windows Fonts folder: `C:\Windows\Fonts`

## Development & Customization

### Modify UI Theme
Edit [Views/MainWindow.xaml](Views/MainWindow.xaml) - color brushes section:
```xaml
<SolidColorBrush x:Key="BackgroundBrush" Color="#F5F5F7"/>
<SolidColorBrush x:Key="AccentBlue" Color="#0071E3"/>
```

### Add New Fonts
Edit [Services/RegistryService.cs](Services/RegistryService.cs) method `MapSystemFonts()`:
```csharp
string regularFont = targetFont == "New Font" ? "NewFont-Regular.ttf" : "...";
```

### Change Default Gamma
Edit [Views/MainWindow.xaml](Views/MainWindow.xaml) slider Value:
```xaml
<Slider ... Value="1000" ... />
```

## Version Information
- **Application**: MacFontRenderer v1.0.0
- **.NET Target**: net8.0-windows
- **Architecture**: WPF (Windows Presentation Foundation)
- **Build System**: .NET CLI
- **Status**: Production-Ready (Phase 1)

---

For more details, see [README.md](README.md) and [BUILD.md](BUILD.md)
