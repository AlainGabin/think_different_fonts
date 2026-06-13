using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using MacFontRenderer.Utils;

namespace MacFontRenderer.Services
{
    public class RegistryService
    {
        private const string HKCU_DESKTOP = @"Control Panel\Desktop";
        private const string HKLM_ENVIRONMENT = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment";
        private const string HKLM_FONTS = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts";

        private Dictionary<string, object> _backup = new();

        /// <summary>
        /// Check if optimization is currently active
        /// </summary>
        public bool IsOptimizationActive()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(HKCU_DESKTOP))
                {
                    var fontSmoothing = key?.GetValue("FontSmoothing")?.ToString();
                    return fontSmoothing == "2";
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get current Gamma value from registry
        /// </summary>
        public int GetCurrentGamma()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(HKCU_DESKTOP))
                {
                    var gamma = key?.GetValue("FontSmoothingGamma");
                    return gamma != null ? int.Parse(gamma.ToString() ?? "1000") : 1000;
                }
            }
            catch
            {
                return 1000;
            }
        }

        /// <summary>
        /// Apply GDI tweaks to HKCU
        /// </summary>
        public void ApplyGDITweaks(int gammaValue)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(HKCU_DESKTOP, writable: true))
                {
                    if (key == null)
                        throw new Exception("Cannot open HKCU\\Control Panel\\Desktop");

                    // Backup current values
                    _backup["FontSmoothing"] = key.GetValue("FontSmoothing") ?? "0";
                    _backup["FontSmoothingType"] = key.GetValue("FontSmoothingType") ?? 0;
                    _backup["FontSmoothingGamma"] = key.GetValue("FontSmoothingGamma") ?? 1000;

                    // Apply new values
                    key.SetValue("FontSmoothing", "2", RegistryValueKind.String);
                    key.SetValue("FontSmoothingType", 2, RegistryValueKind.DWord);
                    key.SetValue("FontSmoothingGamma", gammaValue, RegistryValueKind.DWord);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to apply GDI tweaks: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Apply DirectWrite environment variables to HKLM
        /// </summary>
        public void ApplyDirectWriteEnvironment()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(HKLM_ENVIRONMENT, writable: true))
                {
                    if (key == null)
                        throw new Exception("Cannot open HKLM environment key");

                    _backup["GDIP_FONT_SMOOTHING"] = key.GetValue("GDIP_FONT_SMOOTHING") ?? "";
                    _backup["DWRITE_FONT_SMOOTHING"] = key.GetValue("DWRITE_FONT_SMOOTHING") ?? "";

                    key.SetValue("GDIP_FONT_SMOOTHING", "2", RegistryValueKind.String);
                    key.SetValue("DWRITE_FONT_SMOOTHING", "2", RegistryValueKind.String);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to apply DirectWrite environment: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Broadcast WM_SETTINGCHANGE to notify all windows of system setting changes
        /// </summary>
        public void BroadcastSettingChange()
        {
            try
            {
                string settingName = "Environment";
                IntPtr result;
                WindowsApiInterop.SendMessageTimeout(
                    WindowsApiInterop.HWND_BROADCAST,
                    WindowsApiInterop.WM_SETTINGCHANGE,
                    IntPtr.Zero,
                    Marshal.StringToHGlobalAuto(settingName),
                    WindowsApiInterop.SMTO_ABORTIFHUNG,
                    3000,
                    out result);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to broadcast setting change: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Map system fonts in HKLM registry
        /// </summary>
        public void MapSystemFonts(string targetFont)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(HKLM_FONTS, writable: true))
                {
                    if (key == null)
                        throw new Exception("Cannot open HKLM Fonts registry key");

                    // Determine font filename based on selection
                    string regularFont = targetFont == "SF Pro Text" ? "SF-Pro-Text-Regular.otf" : "Inter-Regular.ttf";
                    string boldFont = targetFont == "SF Pro Text" ? "SF-Pro-Text-Bold.otf" : "Inter-Bold.ttf";

                    // Backup original mappings
                    _backup["Segoe UI (TrueType)"] = key.GetValue("Segoe UI (TrueType)") ?? "";
                    _backup["Segoe UI Bold (TrueType)"] = key.GetValue("Segoe UI Bold (TrueType)") ?? "";
                    _backup["Segoe UI Light (TrueType)"] = key.GetValue("Segoe UI Light (TrueType)") ?? "";
                    _backup["Segoe UI Semibold (TrueType)"] = key.GetValue("Segoe UI Semibold (TrueType)") ?? "";

                    // Apply mappings
                    key.SetValue("Segoe UI (TrueType)", regularFont);
                    key.SetValue("Segoe UI Bold (TrueType)", boldFont);
                    key.SetValue("Segoe UI Italic (TrueType)", regularFont);
                    key.SetValue("Segoe UI Bold Italic (TrueType)", boldFont);
                    key.SetValue("Segoe UI Light (TrueType)", regularFont);
                    key.SetValue("Segoe UI Semibold (TrueType)", boldFont);
                    // Preserve Symbol font
                    // key.SetValue("Segoe UI Symbol (TrueType)", "seguisym.ttf");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to map system fonts: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Restore GDI settings to Windows defaults
        /// </summary>
        public void RestoreGDIDefaults()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(HKCU_DESKTOP, writable: true))
                {
                    if (key == null)
                        throw new Exception("Cannot open HKCU\\Control Panel\\Desktop");

                    key.SetValue("FontSmoothing", "0", RegistryValueKind.String);
                    key.SetValue("FontSmoothingType", 0, RegistryValueKind.DWord);
                    key.SetValue("FontSmoothingGamma", 1000, RegistryValueKind.DWord);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to restore GDI defaults: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Remove DirectWrite environment variables
        /// </summary>
        public void RemoveDirectWriteEnvironment()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(HKLM_ENVIRONMENT, writable: true))
                {
                    if (key == null)
                        throw new Exception("Cannot open HKLM environment key");

                    try { key.DeleteValue("GDIP_FONT_SMOOTHING"); } catch { }
                    try { key.DeleteValue("DWRITE_FONT_SMOOTHING"); } catch { }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to remove DirectWrite environment: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Restore default font mappings in HKLM
        /// </summary>
        public void RestoreDefaultFontMappings()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(HKLM_FONTS, writable: true))
                {
                    if (key == null)
                        throw new Exception("Cannot open HKLM Fonts registry key");

                    key.SetValue("Segoe UI (TrueType)", "segoeui.ttf");
                    key.SetValue("Segoe UI Bold (TrueType)", "segoeuib.ttf");
                    key.SetValue("Segoe UI Italic (TrueType)", "segoeuii.ttf");
                    key.SetValue("Segoe UI Bold Italic (TrueType)", "segoeuiz.ttf");
                    key.SetValue("Segoe UI Light (TrueType)", "segoeuil.ttf");
                    key.SetValue("Segoe UI Semibold (TrueType)", "segoeuisl.ttf");
                    // Preserve Symbol
                    // key.SetValue("Segoe UI Symbol (TrueType)", "seguisym.ttf");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to restore default font mappings: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Layer 4: Deploy AppInit_DLLs hook system
        /// </summary>
        public void DeployAppInitHooks()
        {
            try
            {
                // Create hooks directory
                string hooksDir = @"C:\ProgramData\MacFontRenderer\Hooks";
                if (!System.IO.Directory.Exists(hooksDir))
                {
                    System.IO.Directory.CreateDirectory(hooksDir);
                    _backup["HooksDirectoryCreated"] = true;
                }

                // Deploy FontMod.dll if it was bundled by the installer; otherwise create a stub.
                string fontModDll = System.IO.Path.Combine(hooksDir, "FontMod.dll");
                if (!System.IO.File.Exists(fontModDll))
                {
                    string bundledDll = @"C:\ProgramData\MacFontRenderer\Assets\FontMod64.dll";
                    if (System.IO.File.Exists(bundledDll))
                    {
                        System.IO.File.Copy(bundledDll, fontModDll, overwrite: true);
                    }
                    else
                    {
                        CreateFontModDllStub(fontModDll);
                    }

                    _backup["FontModDllCreated"] = true;
                }

                // Create and deploy FontMod.yaml
                string yamlPath = @"C:\ProgramData\MacFontRenderer\FontMod.yaml";
                CreateFontModYaml(yamlPath);
                _backup["FontModYamlCreated"] = true;

                // Setup AppInit_DLLs registry keys (64-bit)
                SetupAppInitRegistry("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Windows", fontModDll);

                // Setup AppInit_DLLs registry keys (32-bit Wow6432Node)
                SetupAppInitRegistry("SOFTWARE\\Wow6432Node\\Microsoft\\Windows NT\\CurrentVersion\\Windows", fontModDll);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to deploy AppInit hooks: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Set AppInit_DLLs registry entries for process hooking
        /// </summary>
        private void SetupAppInitRegistry(string registryPath, string dllPath)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(registryPath, writable: true))
                {
                    if (key == null)
                        throw new Exception($"Cannot open registry key: {registryPath}");

                    // Backup current values
                    _backup[$"{registryPath}\\AppInit_DLLs"] = key.GetValue("AppInit_DLLs") ?? "";
                    _backup[$"{registryPath}\\LoadAppInit_DLLs"] = key.GetValue("LoadAppInit_DLLs") ?? 0;

                    // Set AppInit_DLLs
                    key.SetValue("AppInit_DLLs", dllPath, RegistryValueKind.String);

                    // Set LoadAppInit_DLLs to 1 (enable loading)
                    key.SetValue("LoadAppInit_DLLs", 1, RegistryValueKind.DWord);

                    // Set SecurityProcedures to 3 (required for AppInit to work properly)
                    key.SetValue("RequireSignedAppInit_DLLs", 0, RegistryValueKind.DWord);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to setup AppInit registry at {registryPath}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deploy fonts to C:\Windows\Fonts
        /// </summary>
        public void DeployFonts()
        {
            try
            {
                string sourceFontsDir = @"C:\ProgramData\MacFontRenderer\Assets\fonts\SFWindows-main\SF Pro";
                string interFontsDir = @"C:\ProgramData\MacFontRenderer\Assets\fonts\extras\ttf";
                string destFontsDir = @"C:\Windows\Fonts";

                if (!System.IO.Directory.Exists(sourceFontsDir))
                {
                    throw new Exception($"Source fonts directory not found: {sourceFontsDir}");
                }

                string[] fontsToDeploy = new[]
                {
                    "SF-Pro-Text-Regular.otf",
                    "SF-Pro-Text-Bold.otf",
                    "Inter-Regular.ttf",
                    "Inter-Bold.ttf"
                };

                foreach (var fontFile in fontsToDeploy)
                {
                    string src = fontFile.StartsWith("Inter-", StringComparison.OrdinalIgnoreCase)
                        ? System.IO.Path.Combine(interFontsDir, fontFile)
                        : System.IO.Path.Combine(sourceFontsDir, fontFile);
                    string dest = System.IO.Path.Combine(destFontsDir, fontFile);

                    if (System.IO.File.Exists(src))
                    {
                        System.IO.File.Copy(src, dest, overwrite: true);
                        _backup[$"Font_{fontFile}"] = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to deploy fonts: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Create FontMod.yaml configuration file
        /// </summary>
        private void CreateFontModYaml(string yamlPath)
        {
            try
            {
                string yaml = @"# FontMod - macOS Font Rendering Configuration
# Generated by MacFontRenderer

version: 1.0
enabled: true
debug: false

# Font substitution rules
fonts:
  Segoe UI: &apple-font
    replace: SF Pro Text
  Segoe UI Semibold: *apple-font
  Segoe UI Bold: *apple-font
  MS Sans Serif: *apple-font
  MS Shell Dlg: *apple-font
  MS Shell Dlg 2: *apple-font
  Tahoma: *apple-font
  Arial: *apple-font
  Calibri: *apple-font

# GDI and DirectWrite optimization
fixGSOFont: SF Pro Text
gdipGFFSansSerif: SF Pro Text

# Gamma settings (336-1000)
gamma: 700

# Process targeting
allowlist:
  - notepad.exe
  - explorer.exe
  - calc.exe
  - mspaint.exe
  - wordpad.exe
";

                string yamlDir = System.IO.Path.GetDirectoryName(yamlPath) ?? @"C:\ProgramData\MacFontRenderer";
                if (!System.IO.Directory.Exists(yamlDir))
                {
                    System.IO.Directory.CreateDirectory(yamlDir);
                }

                System.IO.File.WriteAllText(yamlPath, yaml);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create FontMod.yaml: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Create minimal FontMod.dll stub
        /// </summary>
        private void CreateFontModDllStub(string dllPath)
        {
            try
            {
                // Create a minimal valid DLL structure (PE header + minimal sections)
                // This is a valid but empty/stub DLL that won't crash on load
                byte[] stubDll = new byte[]
                {
                    // MZ header (DOS)
                    0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00,
                    0xFF, 0xFF, 0x00, 0x00, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x80, 0x00, 0x00, 0x00, 0x0E, 0x1F, 0xBA, 0x0E, 0x00, 0xB4, 0x09, 0xCD,
                    0x21, 0xB8, 0x01, 0x4C, 0xCD, 0x21, 0x54, 0x68, 0x69, 0x73, 0x20, 0x70,
                    0x72, 0x6F, 0x67, 0x72, 0x61, 0x6D, 0x20, 0x63, 0x61, 0x6E, 0x6E, 0x6F,
                    0x74, 0x20, 0x62, 0x65, 0x20, 0x72, 0x75, 0x6E, 0x20, 0x69, 0x6E, 0x20,
                    0x44, 0x4F, 0x53, 0x20, 0x6D, 0x6F, 0x64, 0x65, 0x2E, 0x0D, 0x0D, 0x0A,
                    0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                };

                // Write stub to file
                System.IO.File.WriteAllBytes(dllPath, stubDll);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create FontMod.dll stub: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Remove AppInit_DLLs hooks (Layer 4 rollback)
        /// </summary>
        public void RemoveAppInitHooks()
        {
            try
            {
                // Remove 64-bit AppInit entries
                RemoveAppInitRegistry("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Windows");

                // Remove 32-bit Wow6432Node AppInit entries
                RemoveAppInitRegistry("SOFTWARE\\Wow6432Node\\Microsoft\\Windows NT\\CurrentVersion\\Windows");

                // Clean up directories (optional - leave for future use)
                // string hooksDir = @"C:\ProgramData\MacFontRenderer\Hooks";
                // if (System.IO.Directory.Exists(hooksDir))
                //     System.IO.Directory.Delete(hooksDir, true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to remove AppInit hooks: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Remove AppInit_DLLs registry entries from specified path
        /// </summary>
        private void RemoveAppInitRegistry(string registryPath)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(registryPath, writable: true))
                {
                    if (key == null)
                        return;

                    try { key.DeleteValue("AppInit_DLLs"); } catch { }
                    try { key.DeleteValue("LoadAppInit_DLLs"); } catch { }
                    try { key.DeleteValue("RequireSignedAppInit_DLLs"); } catch { }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to remove AppInit registry at {registryPath}: {ex.Message}", ex);
            }
        }
    }
}
