# MacFontRenderer Phase 2 - Architecture & Design

**Status**: Planning & Implementation  
**Phase**: 2 - Process-Scoped Safer Injection  
**Date**: 2026-06-13

---

## Phase 2 Overview

Phase 1 handled **system-wide tweaks** (registry, environment variables, font mappings). 

Phase 2 adds **intelligent process-scoped injection** for applications that need deeper font customization:
- ✅ Per-application font override rules
- ✅ Opt-in selective injection (not system-wide)
- ✅ Windows Service for managed deployment
- ✅ Configuration-driven architecture
- ✅ Chromium/Electron app-specific handling
- ✅ Safe deployment with audit logging

### Why Phase 2?

**Problem**: Some legacy Win32 apps ignore registry changes and use hardcoded fonts (Tahoma, Arial, MS Shell Dlg).

**Phase 1 Solution**: Global font substitution (good but blunt)

**Phase 2 Solution**: Intelligent, opt-in injection per application (precise & safe)

---

## Architecture: Per-Process Injection System

```
MacFontRenderer.exe (Main UI)
    │
    ├─── Registry Tweaks (Phase 1) ✓
    │
    └─── Service Management (Phase 2) NEW
            │
            ├─── MacFontRenderer.Service.exe (Windows Service)
            │    │
            │    ├─── Injection Engine
            │    │    └─── Monitors target processes
            │    │
            │    ├─── Configuration Manager
            │    │    └─── Reads per-app rules
            │    │
            │    └─── Hook Deployment
            │         └─── Injects FontMod.dll into approved apps
            │
            ├─── FontMod.dll (Injection DLL)
            │    │
            │    ├─── Font Hook (SetFont interception)
            │    ├─── Name Mapping (Arial → SF Pro Text)
            │    └─── Cache & Performance
            │
            └─── Configuration System
                 ├─── FontMod.yaml (Global rules)
                 ├─── Per-App Overrides (app-specific settings)
                 └─── Allowlist / Blocklist
```

---

## Phase 2 Components to Build

### 1. **FontMod Service** (C# Windows Service)
**Purpose**: Managed, privilege-scoped injection engine

**Features**:
- Runs as Local System (elevated privileges)
- Monitors process creation events
- Injects FontMod.dll into matching processes
- Reads configuration files (YAML)
- Logs all injections & failures
- Can be enabled/disabled via Service Manager or main app

**Key Methods**:
```csharp
// Service lifecycle
OnStart()       // Start monitoring & injection
OnStop()        // Clean shutdown
OnPause()       // Pause injection
OnContinue()    // Resume injection

// Process management
InjectIntoProcess(Process process)
UnInjectFromProcess(int processId)
GetActiveInjections()
LogInjectionEvent(string appName, bool success)

// Configuration
LoadConfigurationRules()
ReloadOnConfigChange()
ValidateTargetProcess(Process p)
```

**Dependencies**:
- System.ServiceProcess (Windows Service framework)
- System.Diagnostics.EventLog (Event logging)
- YamlDotNet (YAML configuration parsing)
- System.Management (WMI process monitoring)

### 2. **FontMod.dll** (C++ or C# with native interop)
**Purpose**: The actual font hook injected into processes

**Hooks**:
```cpp
// Hook Windows API calls
HFONT WINAPI HookedCreateFontA(int, int, int, int, int, ...);
HFONT WINAPI HookedCreateFontW(int, int, int, int, int, ...);
BOOL WINAPI HookedCreateFontIndirectA(const LOGFONTA *);
BOOL WINAPI HookedCreateFontIndirectW(const LOGFONTW *);
BOOL WINAPI HookedSelectObject(HDC hdc, HGDIOBJ h);

// Hook GDI+ calls (for modern apps)
Gdiplus::Font* HookedGdipCreateFont(...);
```

**Mapping Logic**:
```
Arial           → SF Pro Text Regular
Arial Bold      → SF Pro Text Bold
Tahoma          → SF Pro Text Regular
MS Shell Dlg    → SF Pro Text Regular
Calibri         → SF Pro Text Regular
Segoe UI        → SF Pro Text (already mapped in Phase 1)
```

**Features**:
- Transparent font name substitution
- Cache for performance
- Logging (disable in production)
- Graceful fallback on errors
- Works with GDI & GDI+

### 3. **Configuration System** (YAML)

**FontMod.yaml** (Global rules):
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
  - name: "calc.exe"
    enabled: false
  - name: "explorer.exe"
    enabled: true

allowlist:
  - "notepad.exe"
  - "explorer.exe"
  - "calc.exe"
  - "mspaint.exe"

blocklist:
  - "taskeng.exe"
  - "svchost.exe"
  - "system.exe"
```

**Per-App Override** (`AppOverrides.yaml`):
```yaml
app_specific_rules:
  notepad.exe:
    gamma: 800
    font: "Inter"
  explorer.exe:
    gamma: 1000
    font: "SF Pro Text"
```

### 4. **Service Management** (Main App Enhancement)

New UI Section:
```
┌─ Per-Application Settings ──────────────────┐
│                                              │
│ ☐ Enable Process-Scoped Injection           │
│ (Service: Stopped / Running / Error)        │
│                                              │
│ Managed Applications:                        │
│ ┌────────────────────────────────────────┐  │
│ │ ☑ notepad.exe    | Gamma: 1000 ✕       │  │
│ │ ☑ explorer.exe   | Gamma: 1000 ✕       │  │
│ │ ☑ calc.exe       | Gamma: 900  ✕       │  │
│ │ ☐ mspaint.exe    | Gamma: 1000 ✕       │  │
│ │ ☐ chrome.exe     | Gamma: 1000 ✕       │  │
│ └────────────────────────────────────────┘  │
│                                              │
│ [+ Add App] [- Remove] [Export] [Import]   │
│                                              │
└──────────────────────────────────────────────┘
```

### 5. **Installation & Service Setup**

**First Run**:
1. Check if service installed
2. If not, register Windows Service
3. Create configuration directory: `C:\ProgramData\MacFontRenderer\`
4. Deploy FontMod.dll to `C:\ProgramData\MacFontRenderer\Hooks\`
5. Deploy FontMod.yaml with default rules
6. Start service (requires admin UAC)

**Uninstall**:
1. Stop service
2. Unregister from Windows Service Manager
3. Delete FontMod.dll, configuration files
4. Remove configuration directory

---

## Phase 2 Development Phases

### Phase 2a: Core Service & Injection (First)
1. ✓ Create `MacFontRenderer.Service` project
2. ✓ Implement process monitoring (WMI)
3. ✓ Deploy & load FontMod.dll into target processes
4. ✓ Configuration loading & reloading
5. ✓ Error handling & rollback
6. ✓ Service installation / uninstallation

### Phase 2b: FontMod.dll Injection Library
1. ✓ C++/C# hybrid: P/Invoke for GDI hooks
2. ✓ Font name mapping logic
3. ✓ Graceful error handling
4. ✓ Performance optimization (caching)
5. ✓ Logging (optional)

### Phase 2c: UI & Management
1. ✓ Add "Per-App Settings" tab to main window
2. ✓ Service status indicator
3. ✓ App allowlist editor
4. ✓ Per-app gamma override
5. ✓ Import/Export configuration
6. ✓ Install/Uninstall service buttons

### Phase 2d: Testing & Validation
1. ✓ Test with legacy apps (Notepad, Calc, Paint)
2. ✓ Test with modern apps (Chrome, Edge, VS Code)
3. ✓ Test service start/stop/restart
4. ✓ Test configuration reload
5. ✓ Test rollback & uninstall
6. ✓ Performance benchmarking

---

## Benefits Over AppInit_DLLs

| Feature | AppInit | Phase 2 Service |
|---------|---------|-----------------|
| **Scope** | Global (all processes) | Per-app (selective) |
| **Safety** | High risk (can brick system) | Low risk (isolated) |
| **Performance** | Slow (all apps affected) | Fast (only targets) |
| **Disabling** | Requires registry edit | Service → Off |
| **Audit Trail** | None | Event Log + file logging |
| **Rollback** | Difficult | One-click service stop |
| **Maintenance** | Deprecated | Modern & supported |
| **Enterprise** | Not recommended | Production-ready |

---

## Security & Compliance

✅ **Principle of Least Privilege**:
- Service runs as Local System (needed for injection)
- Main app runs as user (standard)
- Minimal DLL surface area

✅ **Audit Trail**:
- Event Log entries for all injections
- Configuration file versioning
- Per-process success/failure logging

✅ **Fail-Safe**:
- Service can be disabled instantly
- Injection errors don't crash host process
- Rollback to Phase 1 anytime

✅ **Code Signing**:
- FontMod.dll should be signed (Authenticode)
- Service executable signed
- Prevents tampering & malware spoofing

---

## Compatibility

**Supported Applications**:
- ✓ Legacy Win32 (Notepad, Calc, Paint)
- ✓ .NET Framework (older Windows Forms apps)
- ✓ Chromium/Electron (via app-specific rules)
- ✓ Console applications (limited, GDI-only)
- ✓ WPF applications (built-in DPI awareness)

**Not Supported** (by design):
- ✗ Kernel-mode applications
- ✗ Services running as SYSTEM (already privileged)
- ✗ Immersive apps (UWP sandboxed)
- ✗ DLL injectors / malware tools

---

## Roadmap

| Phase | Focus | Status |
|-------|-------|--------|
| Phase 1 | System-wide registry tweaks | ✅ Complete |
| Phase 2 | Per-app process injection | 🔄 In Progress |
| Phase 3 | Kernel-level early-boot | ⏳ Future (optional) |
| Phase 3+ | Enterprise deployment | ⏳ Future (optional) |

---

## Next Steps

1. Create `MacFontRenderer.Service` project (Windows Service)
2. Implement process monitoring engine
3. Build FontMod injection library (C++)
4. Add service management to main UI
5. Create comprehensive test suite
6. Validate on multiple Windows versions

See [PHASE2_IMPLEMENTATION.md](PHASE2_IMPLEMENTATION.md) for detailed technical specifications.

---

**Created**: 2026-06-13  
**Version**: Phase 2 Architecture v1.0  
**Status**: Ready for Implementation
