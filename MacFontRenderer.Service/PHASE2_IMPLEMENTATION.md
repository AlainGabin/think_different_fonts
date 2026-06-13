# Phase 2 Implementation Guide

**Status**: Framework Complete, DLL Injection Pending  
**Date**: 2026-06-13  
**Version**: 1.0

---

## What's Been Built (C# Components ✓)

### 1. **FontRenderingService.cs** ✓
The main Windows Service entry point
- Service lifecycle management (Start/Stop/Pause/Continue)
- Service installation/uninstallation via command line
- Debug mode for testing
- Graceful error handling

**To use**:
```powershell
# Install service
MacFontRenderer.Service.exe install

# Uninstall service
MacFontRenderer.Service.exe uninstall

# Run in debug mode (for testing)
MacFontRenderer.Service.exe debug
```

### 2. **ConfigurationManager.cs** ✓
YAML-based configuration system
- Reads/writes FontMod.yaml
- Reloads on file change
- Font name mapping rules
- Process allowlist/blocklist
- Per-process gamma settings

**Default Configuration** (`C:\ProgramData\MacFontRenderer\FontMod.yaml`):
```yaml
version: 1.0
enabled: true
logging: false

font_mapping:
  default: "SF Pro Text"
  rules:
    Arial: "SF Pro Text"
    Tahoma: "SF Pro Text"
    "MS Shell Dlg": "SF Pro Text"
    Calibri: "SF Pro Text"

target_processes:
  - name: "notepad.exe"
    enabled: true
    gamma: 1000
  - name: "calc.exe"
    enabled: false
    gamma: 1000

allowlist:
  - "notepad.exe"
  - "explorer.exe"
  - "calc.exe"

blocklist:
  - "taskeng.exe"
  - "svchost.exe"
```

### 3. **ProcessInjectionEngine.cs** ✓
Core injection orchestration
- Monitors process creation
- Decides which processes to inject into
- Tracks active injections
- Handles start/stop/pause/resume

**Features**:
- ✓ Process monitoring via WMI
- ✓ Allowlist/blocklist enforcement
- ✓ Injection tracking
- ✓ Per-process state management
- ⏳ Actual DLL injection (see below)

### 4. **EventLogger.cs** ✓
Comprehensive logging system
- Event Log integration
- File-based logging with rotation
- Fallback console output
- 10 MB log rotation

**Log Location**: `C:\ProgramData\MacFontRenderer\Logs\`

---

## What Still Needs to Be Done

### Critical: **FontMod.dll Injection Library** ⏳

This is the actual hook library that performs font substitution.

#### Two Approaches:

**Option A: Pure C# (Simpler, Recommended for Now)**
```csharp
// FontMod.cs (to be injected)
public static class FontHook
{
    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateFontA(int height, int width, ...);
    
    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateFontIndirectA(ref LOGFONTA lf);
    
    // Hook these via detours or similar
    public static IntPtr HookedCreateFontA(int h, int w, ...)
    {
        // Map font name before creating
        return CreateFontA(h, w, ...mapped_font...);
    }
}
```

**Option B: C++ (More Complex, Better Performance)**
```cpp
// FontMod.cpp
#include <windows.h>
#include <detours.h>

typedef HFONT (*CreateFontA_t)(int, int, int, int, int, DWORD, DWORD, DWORD, DWORD, DWORD, DWORD, DWORD, DWORD, LPCSTR);
CreateFontA_t pCreateFontA = CreateFontA;

HFONT WINAPI HookedCreateFontA(
    int height, int width, int escapement, int orientation, int weight,
    DWORD italic, DWORD underline, DWORD strikeout, DWORD charset,
    DWORD outprecision, DWORD clipprecision, DWORD quality, DWORD pitch,
    LPCSTR fontface)
{
    // Map font name
    LPCSTR mapped = MapFontName(fontface);
    
    // Create with mapped font
    return pCreateFontA(height, width, escapement, orientation, weight,
                       italic, underline, strikeout, charset,
                       outprecision, clipprecision, quality, pitch,
                       mapped);
}

// Detour installation
void InitializeHooks()
{
    DetourTransactionBegin();
    DetourAttach(&(PVOID&)pCreateFontA, HookedCreateFontA);
    // ... attach more hooks
    DetourTransactionCommit();
}
```

#### Required Hooks:

| API | Purpose | Criticality |
|-----|---------|-------------|
| `CreateFontA` | GDI font creation (ANSI) | High |
| `CreateFontW` | GDI font creation (Unicode) | High |
| `CreateFontIndirectA` | LOGFONT structure-based creation | High |
| `CreateFontIndirectW` | LOGFONT (Unicode) | High |
| `SelectObject` | Font selection into device context | Medium |
| `GdipCreateFont` | GDI+ font creation | Medium |
| `GetFontData` | Font data retrieval | Low |

#### Implementation Steps:

1. **Create FontMod DLL project** (C++ with Detours or C# P/Invoke)
2. **Implement font name mapping** (Arial → SF Pro Text, etc.)
3. **Export initialization function** (DLL entry point)
4. **Add configuration reading** (read from shared memory or config file)
5. **Build release version** (signed if possible)
6. **Place in** `C:\ProgramData\MacFontRenderer\Hooks\FontMod.dll`

---

## Actual DLL Injection Implementation ⏳

The `ProcessInjectionEngine` has placeholder methods that need implementation:

### `InjectHookDll(Process process)`

**Current Placeholder**:
```csharp
private void InjectHookDll(Process process)
{
    // TODO: Implement actual DLL injection
    _logger.LogInformation($"Hook DLL injection queued for {process.ProcessName}");
}
```

**Real Implementation Required**:

**Option 1: Using Remote Thread (Classic Method)**
```csharp
[DllImport("kernel32.dll", SetLastError = true)]
private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

[DllImport("kernel32.dll", SetLastError = true)]
private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

[DllImport("kernel32.dll", SetLastError = true)]
private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out uint lpThreadId);

private void InjectHookDll(Process process)
{
    // 1. Open process with all access
    IntPtr hProcess = OpenProcess(0x001F0FFF, false, process.Id);
    if (hProcess == IntPtr.Zero)
        throw new Exception("Could not open process");

    // 2. Allocate memory for DLL path
    IntPtr dllPath = Marshal.StringToHGlobalAnsi(HOOK_DLL);
    IntPtr hAllocated = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)HOOK_DLL.Length, 0x1000, 0x40);
    
    // 3. Write DLL path to allocated memory
    WriteProcessMemory(hProcess, hAllocated, Encoding.ASCII.GetBytes(HOOK_DLL),
                      (uint)HOOK_DLL.Length, out UIntPtr written);
    
    // 4. Get LoadLibraryA address
    IntPtr hKernel32 = GetModuleHandle("kernel32.dll");
    IntPtr hLoadLibraryA = GetProcAddress(hKernel32, "LoadLibraryA");
    
    // 5. Create remote thread to load DLL
    CreateRemoteThread(hProcess, IntPtr.Zero, 0, hLoadLibraryA, hAllocated, 0, out uint threadId);
    
    // 6. Cleanup
    Marshal.FreeHGlobal(dllPath);
    CloseHandle(hProcess);
}
```

**Option 2: Using Detours Library (Recommended for Injection)**
```cpp
// Requires: Detours library from Microsoft
// Download: https://github.com/microsoft/Detours

// Build FontMod.dll with Detours
// Then inject using Detours runtime functions
```

---

## Phase 2 UI Integration (Main App Changes) ⏳

Add new tab to `MainWindow.xaml`:

```xaml
<!-- New TabItem in TabControl -->
<TabItem Header="Process Injection" Padding="10">
    <StackPanel Orientation="Vertical" Margin="20">
        
        <!-- Service Status -->
        <Border Background="White" CornerRadius="12" Padding="16" BorderBrush="#E5E5EA" BorderThickness="1" Margin="0,0,0,20">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="Auto"/>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>
                
                <Ellipse Grid.Column="0" Width="12" Height="12" Fill="#FF3B30" x:Name="ServiceIndicator" VerticalAlignment="Center"/>
                <TextBlock Grid.Column="1" Margin="12,0,0,0" FontSize="13" FontWeight="500">Service Status</TextBlock>
                <TextBlock Grid.Column="2" FontSize="13" Foreground="#0071E3" x:Name="ServiceStatus">Stopped</TextBlock>
            </Grid>
        </Border>

        <!-- Managed Applications -->
        <TextBlock FontSize="12" FontWeight="500" Foreground="#1D1D1D" Margin="0,0,0,10">Managed Applications</TextBlock>
        <DataGrid x:Name="ProcessGrid" Height="250" AutoGenerateColumns="False" CanUserAddRows="False" Margin="0,0,0,20">
            <DataGrid.Columns>
                <DataGridCheckBoxColumn Header="Enabled" Binding="{Binding Enabled}" Width="70"/>
                <DataGridTextColumn Header="Process Name" Binding="{Binding ProcessName}" Width="*" IsReadOnly="True"/>
                <DataGridTextColumn Header="Gamma" Binding="{Binding Gamma}" Width="70"/>
                <DataGridTextColumn Header="Status" Binding="{Binding Status}" Width="100" IsReadOnly="True"/>
            </DataGrid.Columns>
        </DataGrid>

        <!-- Action Buttons -->
        <StackPanel Orientation="Horizontal" Spacing="10">
            <Button Name="InstallServiceButton" Padding="12,8" Background="#0071E3" Foreground="White" Click="OnInstallService">Install Service</Button>
            <Button Name="StartServiceButton" Padding="12,8" Background="#34C759" Foreground="White" Click="OnStartService">Start Service</Button>
            <Button Name="StopServiceButton" Padding="12,8" Background="#FF3B30" Foreground="White" Click="OnStopService">Stop Service</Button>
            <Button Name="UninstallServiceButton" Padding="12,8" Background="#CCCCCC" Foreground="#333" Click="OnUninstallService">Uninstall</Button>
        </StackPanel>
    </StackPanel>
</TabItem>
```

**Code-Behind Implementation**:
```csharp
private ServiceController? _serviceController;

private void OnInstallService(object sender, RoutedEventArgs e)
{
    // Launch FontRenderingService.exe install
    ProcessStartInfo psi = new ProcessStartInfo
    {
        FileName = "MacFontRenderer.Service.exe",
        Arguments = "install",
        UseShellExecute = true,
        Verb = "runas" // Admin
    };
    Process.Start(psi);
}

private void OnStartService(object sender, RoutedEventArgs e)
{
    _serviceController = new ServiceController("MacFontRendererService");
    _serviceController.Start();
    UpdateServiceStatus();
}

private void UpdateServiceStatus()
{
    try
    {
        _serviceController?.Refresh();
        ServiceIndicator.Fill = _serviceController?.Status == ServiceControllerStatus.Running 
            ? new SolidColorBrush(Color.FromRgb(52, 199, 89))
            : new SolidColorBrush(Color.FromRgb(255, 59, 48));
        ServiceStatus.Text = _serviceController?.Status.ToString() ?? "Unknown";
    }
    catch { }
}
```

---

## Testing Checklist ⏳

- [ ] Service installs correctly
- [ ] Service starts/stops without errors
- [ ] Processes are detected and tracked
- [ ] FontMod.dll injects successfully into allowed processes
- [ ] Font substitution works in target apps
- [ ] Configuration reloading works
- [ ] Pause/resume functionality works
- [ ] Uninstall removes service cleanly
- [ ] Event logging works
- [ ] Performance impact is minimal

---

## Next Build Steps

1. **Build C# Service**: `dotnet build MacFontRenderer.Service.csproj`
2. **Create FontMod.dll**: (C++ or advanced C# project)
3. **Update Main App UI**: Add Process Injection tab
4. **Comprehensive Testing**: Test all scenarios
5. **Code Signing**: Sign DLL and service EXE

---

## For Reference: Key Directories

```
C:\ProgramData\MacFontRenderer\
├── FontMod.yaml                    # Configuration
├── Hooks\
│   └── FontMod.dll                 # Hook library (to be created)
└── Logs\
    └── 2026-06-13.log              # Service logs
```

---

## Estimated Effort

| Component | Effort | Priority |
|-----------|--------|----------|
| Service Framework | ✓ Done | High |
| Configuration System | ✓ Done | High |
| FontMod.dll (C++) | 8-16 hours | High |
| DLL Injection | 4-8 hours | High |
| UI Integration | 2-4 hours | Medium |
| Testing | 4-8 hours | High |
| **Total Phase 2** | **~30-40 hours** | |

---

**Status**: Framework complete, ready for DLL development  
**Next Phase**: Build FontMod.dll injection library
