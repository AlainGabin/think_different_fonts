using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using MacFontRenderer.Service.Utils;

namespace MacFontRenderer.Service.Configuration
{
    public class FontMappingConfig
    {
        [YamlMember(Alias = "default", Order = 1)]
        public string DefaultFont { get; set; } = "SF Pro Text";

        [YamlMember(Alias = "rules", Order = 2)]
        public Dictionary<string, string> Rules { get; set; } = new();
    }

    public class ProcessConfig
    {
        [YamlMember(Alias = "name", Order = 1)]
        public string Name { get; set; } = "";

        [YamlMember(Alias = "enabled", Order = 2)]
        public bool Enabled { get; set; } = true;

        [YamlMember(Alias = "gamma", Order = 3)]
        public int Gamma { get; set; } = 1000;
    }

    public class RootConfig
    {
        [YamlMember(Alias = "version", Order = 1)]
        public string Version { get; set; } = "1.0";

        [YamlMember(Alias = "enabled", Order = 2)]
        public bool Enabled { get; set; } = true;

        [YamlMember(Alias = "logging", Order = 3)]
        public bool Logging { get; set; } = false;

        [YamlMember(Alias = "font_mapping", Order = 4)]
        public FontMappingConfig FontMapping { get; set; } = new();

        [YamlMember(Alias = "target_processes", Order = 5)]
        public List<ProcessConfig> TargetProcesses { get; set; } = new();

        [YamlMember(Alias = "allowlist", Order = 6)]
        public List<string> Allowlist { get; set; } = new();

        [YamlMember(Alias = "blocklist", Order = 7)]
        public List<string> Blocklist { get; set; } = new();
    }

    public class ConfigurationManager
    {
        private readonly string _configPath;
        private readonly EventLogger _logger;
        private RootConfig? _config;
        private readonly string _configFile;
        private DateTime _lastConfigModified;

        public ConfigurationManager(string configPath, EventLogger logger)
        {
            _configPath = configPath;
            _logger = logger;
            _configFile = Path.Combine(configPath, "FontMod.yaml");
            _lastConfigModified = DateTime.MinValue;
        }

        /// <summary>
        /// Load or create default configuration
        /// </summary>
        public void LoadConfiguration()
        {
            try
            {
                EnsureConfigDirectory();

                if (!File.Exists(_configFile))
                {
                    CreateDefaultConfiguration();
                }

                var deserializer = new DeserializerBuilder().Build();
                string yaml = File.ReadAllText(_configFile);
                _config = deserializer.Deserialize<RootConfig>(yaml) ?? new RootConfig();
                _lastConfigModified = File.GetLastWriteTime(_configFile);

                _logger.LogInformation($"Configuration loaded from {_configFile}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load configuration: {ex.Message}", ex);
                _config = new RootConfig();
            }
        }

        /// <summary>
        /// Reload configuration if file changed
        /// </summary>
        public void ReloadIfChanged()
        {
            try
            {
                if (!File.Exists(_configFile))
                    return;

                DateTime fileModified = File.GetLastWriteTime(_configFile);
                if (fileModified > _lastConfigModified)
                {
                    _logger.LogInformation("Configuration file changed, reloading...");
                    LoadConfiguration();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to check configuration: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Save current configuration to file
        /// </summary>
        public void SaveConfiguration()
        {
            try
            {
                if (_config == null)
                    return;

                EnsureConfigDirectory();

                var serializer = new SerializerBuilder()
                    .WithIndentedSequences()
                    .Build();

                string yaml = serializer.Serialize(_config);
                File.WriteAllText(_configFile, yaml);
                _lastConfigModified = File.GetLastWriteTime(_configFile);

                _logger.LogInformation("Configuration saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save configuration: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Check if process should be injected
        /// </summary>
        public bool ShouldInjectProcess(string processName)
        {
            try
            {
                if (_config == null || !_config.Enabled)
                    return false;

                string cleanName = Path.GetFileName(processName).ToLower();

                // Check blocklist first
                if (_config.Blocklist.Any(b => b.ToLower() == cleanName))
                    return false;

                // Check allowlist
                if (_config.Allowlist.Any(a => a.ToLower() == cleanName))
                    return true;

                // Check target processes
                var target = _config.TargetProcesses.FirstOrDefault(p => p.Name.ToLower() == cleanName);
                return target?.Enabled ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking process: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Get font mapping for a given font name
        /// </summary>
        public string MapFont(string fontName)
        {
            if (_config?.FontMapping.Rules.TryGetValue(fontName, out var mapped) == true)
                return mapped;

            return _config?.FontMapping.DefaultFont ?? "SF Pro Text";
        }

        /// <summary>
        /// Get gamma value for process
        /// </summary>
        public int GetProcessGamma(string processName)
        {
            string cleanName = Path.GetFileName(processName).ToLower();
            var target = _config?.TargetProcesses.FirstOrDefault(p => p.Name.ToLower() == cleanName);
            return target?.Gamma ?? 1000;
        }

        /// <summary>
        /// Get current configuration
        /// </summary>
        public RootConfig GetConfiguration() => _config ?? new RootConfig();

        /// <summary>
        /// Update configuration
        /// </summary>
        public void UpdateConfiguration(RootConfig newConfig)
        {
            _config = newConfig;
            SaveConfiguration();
        }

        private void EnsureConfigDirectory()
        {
            if (!Directory.Exists(_configPath))
            {
                Directory.CreateDirectory(_configPath);
            }
        }

        private void CreateDefaultConfiguration()
        {
            var config = new RootConfig
            {
                Version = "1.0",
                Enabled = true,
                Logging = false,
                FontMapping = new FontMappingConfig
                {
                    DefaultFont = "SF Pro Text",
                    Rules = new Dictionary<string, string>
                    {
                        { "Arial", "SF Pro Text" },
                        { "Tahoma", "SF Pro Text" },
                        { "MS Shell Dlg", "SF Pro Text" },
                        { "Calibri", "SF Pro Text" },
                        { "Segoe UI", "SF Pro Text" }
                    }
                },
                TargetProcesses = new List<ProcessConfig>
                {
                    new() { Name = "notepad.exe", Enabled = true, Gamma = 1000 },
                    new() { Name = "calc.exe", Enabled = false, Gamma = 1000 },
                    new() { Name = "explorer.exe", Enabled = true, Gamma = 1000 },
                    new() { Name = "mspaint.exe", Enabled = false, Gamma = 1000 }
                },
                Allowlist = new List<string> { "notepad.exe", "explorer.exe", "calc.exe" },
                Blocklist = new List<string> { "taskeng.exe", "svchost.exe", "system.exe" }
            };

            _config = config;
            SaveConfiguration();
        }
    }
}
