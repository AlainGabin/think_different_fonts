# Phase 2 Next Steps - Immediate Action Plan

**Status**: 🎯 Ready to Begin FontMod.dll Development  
**Priority**: HIGH (blocks Phase 2 completion)  
**Estimated Effort**: 20-30 hours  
**Complexity**: Medium-High (requires Windows API knowledge)  

---

## 🚀 What's Next?

The Windows Service framework is **complete and compiled**. The **only blocker** for Phase 2 functionality is the **FontMod.dll injection library**.

---

## 📋 Option A: Build FontMod.dll in C# (Recommended for Speed)

### Why C#?
- ✅ Faster to develop (already familiar with project)
- ✅ Leverage existing code patterns
- ✅ Direct integration with ProcessInjectionEngine
- ✅ 70% of the work done already

### Why Not C#?
- ❌ Slightly lower performance than C++
- ❌ Larger DLL size (~2-3 MB vs 500 KB)
- ❌ More memory per process

---

## 📋 Option B: Build FontMod.dll in C++ (Recommended for Production)

### Why C++?
- ✅ Best performance (native hooks)
- ✅ Smallest DLL size (~500 KB)
- ✅ Industry standard for API hooking
- ✅ Detours library support

### Why Not C++?
- ❌ Steeper learning curve
- ❌ Requires C++ build environment
- ❌ More debugging complexity
- ❌ Longer development time

---

## 🎯 Recommended Path: C# Implementation (Phase 2a)

### Step 1: Create FontMod.dll Project

```powershell
# Create new C# DLL project
cd '<repo-root>\'

# Option A: Use dotnet CLI
dotnet new classlib -n MacFontRenderer.Hooks -f net8.0-windows

# Option B: Manual project file
# Create MacFontRenderer.Hooks\MacFontRenderer.Hooks.csproj with content below
```

### Project File (MacFontRenderer.Hooks.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <AssemblyName>FontMod</AssemblyName>
    <RootNamespace>MacFontRenderer.Hooks</RootNamespace>
    <Nullable>enable</Nullable>
    <LangVersion>latest</LangVersion>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <PlatformTarget>x64</PlatformTarget>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Costura.Fody" Version="5.7.0" />
    <PackageReference Include="Fody" Version="6.8.1" />
  </ItemGroup>

</Project>
```

### Step 2: Implement GDI Font Hooks

**File**: `MacFontRenderer.Hooks/FontHooks.cs`

```csharp
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace MacFontRenderer.Hooks
{
    public static class FontHooks
    {
        // ===== API Declarations =====
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateFontA(
            int nHeight,
            int nWidth,
            int nEscapement,
            int nOrientation,
            int fnWeight,
            uint fdwItalic,
            uint fdwUnderline,
            uint fdwStrikeOut,
            uint fdwCharSet,
            uint fdwOutputPrecision,
            uint fdwClipPrecision,
            uint fdwQuality,
            uint fdwPitchAndFamily,
            [MarshalAs(UnmanagedType.LPStr)] string lpszFace);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateFontW(
            int nHeight,
            int nWidth,
            int nEscapement,
            int nOrientation,
            int fnWeight,
            uint fdwItalic,
            uint fdwUnderline,
            uint fdwStrikeOut,
            uint fdwCharSet,
            uint fdwOutputPrecision,
            uint fdwClipPrecision,
            uint fdwQuality,
            uint fdwPitchAndFamily,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszFace);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateFontIndirectA(
            ref LOGFONTA lf);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateFontIndirectW(
            ref LOGFONTW lf);

        // ===== Font Name Mapping =====
        private static readonly Dictionary<string, string> FontMapping = new()
        {
            { "Arial", "SF Pro Text" },
            { "arial", "SF Pro Text" },
            { "Tahoma", "SF Pro Text" },
            { "tahoma", "SF Pro Text" },
            { "MS Shell Dlg", "SF Pro Text" },
            { "ms shell dlg", "SF Pro Text" },
            { "Segoe UI", "SF Pro Text" },
            { "segoe ui", "SF Pro Text" },
            { "Verdana", "Inter" },
            { "verdana", "Inter" },
            { "Times New Roman", "Inter" },
            { "times new roman", "Inter" },
        };

        // ===== Mapping Method =====
        public static string MapFontName(string? fontName)
        {
            if (string.IsNullOrEmpty(fontName))
                return "SF Pro Text";

            if (FontMapping.TryGetValue(fontName, out var mapped))
                return mapped;

            return fontName;
        }

        // ===== Hooked API Implementations =====
        
        public static IntPtr HookedCreateFontA(
            int nHeight,
            int nWidth,
            int nEscapement,
            int nOrientation,
            int fnWeight,
            uint fdwItalic,
            uint fdwUnderline,
            uint fdwStrikeOut,
            uint fdwCharSet,
            uint fdwOutputPrecision,
            uint fdwClipPrecision,
            uint fdwQuality,
            uint fdwPitchAndFamily,
            string lpszFace)
        {
            // Map the font name
            string mappedFace = MapFontName(lpszFace);

            // Call original API with mapped name
            return CreateFontA(
                nHeight, nWidth, nEscapement, nOrientation,
                fnWeight, fdwItalic, fdwUnderline, fdwStrikeOut,
                fdwCharSet, fdwOutputPrecision, fdwClipPrecision,
                fdwQuality, fdwPitchAndFamily, mappedFace);
        }

        public static IntPtr HookedCreateFontW(
            int nHeight,
            int nWidth,
            int nEscapement,
            int nOrientation,
            int fnWeight,
            uint fdwItalic,
            uint fdwUnderline,
            uint fdwStrikeOut,
            uint fdwCharSet,
            uint fdwOutputPrecision,
            uint fdwClipPrecision,
            uint fdwQuality,
            uint fdwPitchAndFamily,
            string lpszFace)
        {
            // Map the font name
            string mappedFace = MapFontName(lpszFace);

            // Call original API with mapped name
            return CreateFontW(
                nHeight, nWidth, nEscapement, nOrientation,
                fnWeight, fdwItalic, fdwUnderline, fdwStrikeOut,
                fdwCharSet, fdwOutputPrecision, fdwClipPrecision,
                fdwQuality, fdwPitchAndFamily, mappedFace);
        }

        public static IntPtr HookedCreateFontIndirectA(ref LOGFONTA lf)
        {
            // Map the font name
            lf.lfFaceName = MapFontName(lf.lfFaceName);

            // Call original API with mapped name
            return CreateFontIndirectA(ref lf);
        }

        public static IntPtr HookedCreateFontIndirectW(ref LOGFONTW lf)
        {
            // Map the font name
            lf.lfFaceName = MapFontName(lf.lfFaceName);

            // Call original API with mapped name
            return CreateFontIndirectW(ref lf);
        }
    }

    // ===== Data Structures =====
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct LOGFONTA
    {
        public int lfHeight;
        public int lfWidth;
        public int lfEscapement;
        public int lfOrientation;
        public int lfWeight;
        public byte lfItalic;
        public byte lfUnderline;
        public byte lfStrikeOut;
        public byte lfCharSet;
        public byte lfOutPrecision;
        public byte lfClipPrecision;
        public byte lfQuality;
        public byte lfPitchAndFamily;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string lfFaceName;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct LOGFONTW
    {
        public int lfHeight;
        public int lfWidth;
        public int lfEscapement;
        public int lfOrientation;
        public int lfWeight;
        public byte lfItalic;
        public byte lfUnderline;
        public byte lfStrikeOut;
        public byte lfCharSet;
        public byte lfOutPrecision;
        public byte lfClipPrecision;
        public byte lfQuality;
        public byte lfPitchAndFamily;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string lfFaceName;
    }
}
```

### Step 3: Create Configuration Interface

**File**: `MacFontRenderer.Hooks/HookConfiguration.cs`

```csharp
using System;
using System.Collections.Generic;

namespace MacFontRenderer.Hooks
{
    public interface IHookConfiguration
    {
        string MapFontName(string fontName);
        bool IsHookingEnabled();
        void SetConfiguration(Dictionary<string, string> fontMap);
    }

    public class HookConfiguration : IHookConfiguration
    {
        private Dictionary<string, string> _fontMap;
        private bool _isEnabled = true;

        public HookConfiguration()
        {
            _fontMap = new()
            {
                { "Arial", "SF Pro Text" },
                { "Tahoma", "SF Pro Text" },
                { "MS Shell Dlg", "SF Pro Text" },
                { "Segoe UI", "SF Pro Text" },
            };
        }

        public string MapFontName(string fontName)
        {
            if (string.IsNullOrEmpty(fontName) || !_isEnabled)
                return fontName;

            return _fontMap.TryGetValue(fontName, out var mapped) 
                ? mapped 
                : fontName;
        }

        public bool IsHookingEnabled() => _isEnabled;

        public void SetConfiguration(Dictionary<string, string> fontMap)
        {
            _fontMap = fontMap ?? new();
        }

        public void Enable() => _isEnabled = true;
        public void Disable() => _isEnabled = false;
    }
}
```

### Step 4: Build the Project

```powershell
cd '<repo-root>\MacFontRenderer.Hooks'

# Build
dotnet build -c Release

# Output: bin/Release/net8.0-windows/FontMod.dll
```

### Step 5: Deploy DLL

```powershell
# Copy to hooks directory
Copy-Item -Path "bin/Release/net8.0-windows/FontMod.dll" `
          -Destination "C:\ProgramData\MacFontRenderer\Hooks\"

# Create directory if needed
New-Item -ItemType Directory -Path "C:\ProgramData\MacFontRenderer\Hooks" -Force
```

---

## 🔌 Inject the DLL into Processes

### Update ProcessInjectionEngine.cs

Replace the stub `InjectHookDll` method:

```csharp
[DllImport("kernel32.dll", SetLastError = true)]
private static extern IntPtr OpenProcess(
    uint dwDesiredAccess,
    bool bInheritHandle,
    uint dwProcessId);

[DllImport("kernel32.dll", SetLastError = true)]
private static extern IntPtr VirtualAllocEx(
    IntPtr hProcess,
    IntPtr lpAddress,
    uint dwSize,
    uint flAllocationType,
    uint flProtect);

[DllImport("kernel32.dll", SetLastError = true)]
private static extern bool WriteProcessMemory(
    IntPtr hProcess,
    IntPtr lpBaseAddress,
    byte[] lpBuffer,
    uint nSize,
    out IntPtr lpNumberOfBytesWritten);

[DllImport("kernel32.dll", SetLastError = true)]
private static extern IntPtr CreateRemoteThread(
    IntPtr hProcess,
    IntPtr lpThreadAttributes,
    uint dwStackSize,
    IntPtr lpStartAddress,
    IntPtr lpParameter,
    uint dwCreationFlags,
    out uint lpThreadId);

[DllImport("kernel32.dll")]
private static extern IntPtr GetProcAddress(
    IntPtr hModule,
    [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

[DllImport("kernel32.dll")]
private static extern IntPtr GetModuleHandle(
    [MarshalAs(UnmanagedType.LPStr)] string lpModuleName);

[DllImport("kernel32.dll", SetLastError = true)]
private static extern bool CloseHandle(IntPtr hObject);

private const uint PROCESS_ALL_ACCESS = 0x001F0FFF;
private const uint MEM_COMMIT = 0x1000;
private const uint PAGE_READWRITE = 0x0004;

private bool InjectHookDll(Process process)
{
    try
    {
        IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, (uint)process.Id);
        if (hProcess == IntPtr.Zero)
        {
            _logger?.LogError($"Failed to open process {process.ProcessName} ({process.Id})");
            return false;
        }

        // DLL path
        string dllPath = @"C:\ProgramData\MacFontRenderer\Hooks\FontMod.dll";
        byte[] dllBytes = System.Text.Encoding.ASCII.GetBytes(dllPath + '\0');

        // Allocate memory in target process
        IntPtr allocMem = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)dllBytes.Length, MEM_COMMIT, PAGE_READWRITE);
        if (allocMem == IntPtr.Zero)
        {
            CloseHandle(hProcess);
            _logger?.LogError($"Failed to allocate memory in process {process.ProcessName}");
            return false;
        }

        // Write DLL path to allocated memory
        if (!WriteProcessMemory(hProcess, allocMem, dllBytes, (uint)dllBytes.Length, out var _))
        {
            CloseHandle(hProcess);
            _logger?.LogError($"Failed to write to process memory {process.ProcessName}");
            return false;
        }

        // Get address of LoadLibraryA in kernel32.dll
        IntPtr hKernel32 = GetModuleHandle("kernel32.dll");
        IntPtr loadLibraryAddr = GetProcAddress(hKernel32, "LoadLibraryA");

        // Create remote thread to call LoadLibraryA
        if (CreateRemoteThread(hProcess, IntPtr.Zero, 0, loadLibraryAddr, allocMem, 0, out _) == IntPtr.Zero)
        {
            CloseHandle(hProcess);
            _logger?.LogError($"Failed to create remote thread in {process.ProcessName}");
            return false;
        }

        System.Threading.Thread.Sleep(500); // Give it time to load
        CloseHandle(hProcess);

        _logger?.LogInformation($"Successfully injected FontMod.dll into {process.ProcessName} ({process.Id})");
        return true;
    }
    catch (Exception ex)
    {
        _logger?.LogError($"Exception during injection of {process.ProcessName}: {ex.Message}", ex);
        return false;
    }
}
```

---

## 🧪 Testing Plan

### Unit Tests
```csharp
[TestFixture]
public class FontHooksTests
{
    [Test]
    public void MapFontName_Arial_ReturnsSFProText()
    {
        var result = FontHooks.MapFontName("Arial");
        Assert.AreEqual("SF Pro Text", result);
    }

    [Test]
    public void MapFontName_UnknownFont_ReturnsOriginal()
    {
        var result = FontHooks.MapFontName("CustomFont");
        Assert.AreEqual("CustomFont", result);
    }
}
```

### Integration Tests
1. **Injection Test**: Inject into Notepad, verify FontMod.dll loaded
2. **Hook Test**: Create font, verify correct name substituted
3. **Config Test**: Change config, verify new mappings applied
4. **Cleanup Test**: Unject DLL, verify removal

---

## 📊 Development Checklist

Phase 2a: FontMod.dll Implementation

- [ ] Create MacFontRenderer.Hooks project
- [ ] Implement FontHooks class
  - [ ] GDI API declarations
  - [ ] Font name mapping logic
  - [ ] Hooked CreateFontA
  - [ ] Hooked CreateFontW
  - [ ] Hooked CreateFontIndirectA
  - [ ] Hooked CreateFontIndirectW
- [ ] Implement HookConfiguration class
- [ ] Build and compile DLL successfully
- [ ] Deploy to C:\ProgramData\MacFontRenderer\Hooks\
- [ ] Update ProcessInjectionEngine injection code
- [ ] Implement DLL injection (RemoteThread + LoadLibrary)
- [ ] Test with Notepad (basic)
- [ ] Test with Calc (ANSI calls)
- [ ] Test with Explorer (Unicode calls)
- [ ] Test configuration reload
- [ ] Performance profiling

---

## 📈 Estimated Timeline

| Task | Hours | Status |
|------|-------|--------|
| Create project structure | 1 | Next |
| Implement FontHooks | 4 | Next |
| Implement Configuration | 2 | Next |
| Build & compile | 1 | Next |
| DLL injection code | 3 | Next |
| Basic testing | 4 | Next |
| Advanced testing | 5 | Next |
| Bug fixes & optimization | 3 | Next |
| **Total** | **23** | **In Progress** |

---

## 🎯 Success Criteria

When Phase 2a is complete:
1. ✅ FontMod.dll created and compiled
2. ✅ Injects successfully into processes
3. ✅ Font substitution verified in 3+ applications
4. ✅ No crashes or memory leaks
5. ✅ Configuration loading works
6. ✅ Event logging functional

---

## 🚀 Ready to Start?

The Phase 2 service framework is complete and waiting. Creating FontMod.dll is the final piece.

### Quick Start Commands

```powershell
# 1. Create project
cd '<repo-root>'
dotnet new classlib -n MacFontRenderer.Hooks -f net8.0-windows

# 2. Add font hooks code (use code from above)
# File: MacFontRenderer.Hooks/FontHooks.cs

# 3. Build
cd MacFontRenderer.Hooks
dotnet build -c Release

# 4. Deploy
mkdir "C:\ProgramData\MacFontRenderer\Hooks" -Force
Copy-Item -Path "bin/Release/net8.0-windows/FontMod.dll" -Destination "C:\ProgramData\MacFontRenderer\Hooks\"

# 5. Test
.\bin\Release\net8.0-windows\FontMod.dll
# (Should load without errors)
```

---

## 📞 Questions?

Refer to:
- `PHASE2_ARCHITECTURE.md` — System design
- `PHASE2_IMPLEMENTATION.md` — Technical details
- `ProcessInjectionEngine.cs` — Current injection stubs
- Windows API documentation — RemoteThread/LoadLibrary pattern

---

**Next Step**: Create MacFontRenderer.Hooks project and implement FontHooks.cs  
**Target**: Complete Phase 2a in 24-48 hours  
**Result**: Full working Phase 2 system with per-app font injection  

🚀 **Let's build FontMod.dll!**
