using System;
using System.IO;
using Microsoft.Win32;

namespace MacFontRenderer.Services
{
    public class FontService
    {
        private const string FONTS_REGISTRY_PATH = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts";
        private const string FONTS_SYSTEM_PATH = @"C:\Windows\Fonts";

        /// <summary>
        /// Check if a font is installed on the system
        /// </summary>
        public bool IsFontInstalled(string fontName)
        {
            try
            {
                // Check registry
                using (var key = Registry.LocalMachine.OpenSubKey(FONTS_REGISTRY_PATH))
                {
                    if (key != null)
                    {
                        foreach (var valueName in key.GetValueNames())
                        {
                            if (valueName.StartsWith(fontName, StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get the font filename for a given font name
        /// </summary>
        public string GetFontFileName(string fontName)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(FONTS_REGISTRY_PATH))
                {
                    if (key != null)
                    {
                        foreach (var valueName in key.GetValueNames())
                        {
                            if (valueName.StartsWith(fontName, StringComparison.OrdinalIgnoreCase) &&
                                valueName.Contains("Regular"))
                            {
                                var value = key.GetValue(valueName);
                                return value?.ToString() ?? "";
                            }
                        }
                    }
                }

                return "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Install a font file to the system
        /// </summary>
        public bool InstallFont(string fontFilePath)
        {
            try
            {
                if (!File.Exists(fontFilePath))
                    throw new FileNotFoundException($"Font file not found: {fontFilePath}");

                string fileName = Path.GetFileName(fontFilePath);
                string destPath = Path.Combine(FONTS_SYSTEM_PATH, fileName);

                // Copy to system fonts folder
                File.Copy(fontFilePath, destPath, overwrite: true);

                // Register in system
                using (var key = Registry.LocalMachine.OpenSubKey(FONTS_REGISTRY_PATH, writable: true))
                {
                    if (key != null)
                    {
                        string fontDisplayName = Path.GetFileNameWithoutExtension(fileName);
                        key.SetValue($"{fontDisplayName} (TrueType)", fileName);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Uninstall a font from the system
        /// </summary>
        public bool UninstallFont(string fontName)
        {
            try
            {
                string fontFile = GetFontFileName(fontName);
                if (string.IsNullOrEmpty(fontFile))
                    return false;

                string fontPath = Path.Combine(FONTS_SYSTEM_PATH, fontFile);

                // Remove from registry
                using (var key = Registry.LocalMachine.OpenSubKey(FONTS_REGISTRY_PATH, writable: true))
                {
                    if (key != null)
                    {
                        foreach (var valueName in key.GetValueNames())
                        {
                            if (valueName.StartsWith(fontName, StringComparison.OrdinalIgnoreCase))
                            {
                                try { key.DeleteValue(valueName); } catch { }
                            }
                        }
                    }
                }

                // Remove file
                if (File.Exists(fontPath))
                {
                    File.Delete(fontPath);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
