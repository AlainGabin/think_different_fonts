using System;
using System.ServiceProcess;
using System.Diagnostics;
using System.IO;
using MacFontRenderer.Service.Engine;
using MacFontRenderer.Service.Configuration;
using MacFontRenderer.Service.Utils;

namespace MacFontRenderer.Service
{
    public class FontRenderingService : ServiceBase
    {
        private ProcessInjectionEngine? _injectionEngine;
        private ConfigurationManager? _configManager;
        private EventLogger? _logger;
        private const string SERVICE_NAME = "MacFontRendererService";
        private const string DISPLAY_NAME = "MacOS Font Rendering Service";
        private const string CONFIG_PATH = @"C:\ProgramData\MacFontRenderer\";

        public FontRenderingService()
        {
            ServiceName = SERVICE_NAME;
            CanStop = true;
            CanShutdown = true;
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _logger = new EventLogger(SERVICE_NAME);
                _logger.LogInformation("Service starting...");

                // Initialize configuration manager
                _configManager = new ConfigurationManager(CONFIG_PATH, _logger);
                _configManager.LoadConfiguration();
                _logger.LogInformation("Configuration loaded successfully");

                // Initialize injection engine
                _injectionEngine = new ProcessInjectionEngine(_configManager, _logger);
                _injectionEngine.Start();
                _logger.LogInformation($"{DISPLAY_NAME} started successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Service start failed: {ex.Message}", ex);
                throw;
            }
        }

        protected override void OnStop()
        {
            try
            {
                _logger?.LogInformation("Service stopping...");
                
                _injectionEngine?.Stop();
                _logger?.LogInformation("Injection engine stopped");
                
                _logger?.LogInformation("Service stopped successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Service stop failed: {ex.Message}", ex);
                throw;
            }
        }

        protected override void OnPause()
        {
            try
            {
                _logger?.LogInformation("Service pausing...");
                _injectionEngine?.Pause();
                _logger?.LogInformation("Service paused");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Service pause failed: {ex.Message}", ex);
                throw;
            }
        }

        protected override void OnContinue()
        {
            try
            {
                _logger?.LogInformation("Service resuming...");
                _injectionEngine?.Resume();
                _logger?.LogInformation("Service resumed");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Service resume failed: {ex.Message}", ex);
                throw;
            }
        }

        protected override void OnShutdown()
        {
            _logger?.LogInformation("Service shutdown initiated");
            OnStop();
        }

        /// <summary>
        /// Install the Windows Service
        /// </summary>
        public static void Install()
        {
            try
            {
                string servicePath = Environment.ProcessPath
                    ?? System.Reflection.Assembly.GetEntryAssembly()?.Location
                    ?? System.Reflection.Assembly.GetExecutingAssembly().Location;
                
                ProcessStartInfo psi = new ProcessStartInfo("sc.exe", $"create {SERVICE_NAME} binPath= \"{servicePath}\"")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    process?.WaitForExit();
                    Console.WriteLine($"Service '{SERVICE_NAME}' installed successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Installation failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Uninstall the Windows Service
        /// </summary>
        public static void Uninstall()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("sc.exe", $"delete {SERVICE_NAME}")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    process?.WaitForExit();
                    Console.WriteLine($"Service '{SERVICE_NAME}' uninstalled successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Uninstallation failed: {ex.Message}");
                throw;
            }
        }

        static void Main(string[] args)
        {
            // Handle command-line arguments
            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "install":
                        Install();
                        return;
                    case "uninstall":
                        Uninstall();
                        return;
                    case "debug":
                        // Run as console app for debugging
                        var service = new FontRenderingService();
                        service.OnStart(Array.Empty<string>());
                        Console.WriteLine("Service running in debug mode. Press Enter to stop...");
                        Console.ReadLine();
                        service.OnStop();
                        return;
                }
            }

            // Run as Windows Service
            ServiceBase.Run(new FontRenderingService());
        }
    }
}
