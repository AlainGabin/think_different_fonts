using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using MacFontRenderer.Service.Configuration;
using MacFontRenderer.Service.Utils;

namespace MacFontRenderer.Service.Engine
{
    public class ProcessInjectionEngine
    {
        private readonly ConfigurationManager _configManager;
        private readonly EventLogger _logger;
        private ManagementEventWatcher? _processWatcher;
        private bool _running;
        private bool _paused;
        private readonly HashSet<int> _injectedProcesses = new();
        private const string HOOK_DLL = @"C:\ProgramData\MacFontRenderer\Hooks\FontMod.dll";
        private readonly object _lockObject = new();

        public ProcessInjectionEngine(ConfigurationManager configManager, EventLogger logger)
        {
            _configManager = configManager;
            _logger = logger;
            _running = false;
            _paused = false;
        }

        /// <summary>
        /// Start the injection engine
        /// </summary>
        public void Start()
        {
            lock (_lockObject)
            {
                if (_running)
                    return;

                _running = true;
                _paused = false;

                try
                {
                    // Verify hook DLL exists
                    if (!File.Exists(HOOK_DLL))
                    {
                        _logger.LogWarning($"Hook DLL not found: {HOOK_DLL}");
                        // Non-fatal: service continues with limited functionality
                    }

                    // Inject into currently running processes
                    InjectIntoExistingProcesses();

                    // Start monitoring for new processes
                    StartProcessMonitoring();

                    _logger.LogInformation("Process injection engine started");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to start injection engine: {ex.Message}", ex);
                    _running = false;
                    throw;
                }
            }
        }

        /// <summary>
        /// Stop the injection engine
        /// </summary>
        public void Stop()
        {
            lock (_lockObject)
            {
                if (!_running)
                    return;

                _running = false;

                try
                {
                    StopProcessMonitoring();
                    UnInjectFromAllProcesses();
                    _injectedProcesses.Clear();
                    _logger.LogInformation("Process injection engine stopped");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error stopping injection engine: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// Pause injection (service pause)
        /// </summary>
        public void Pause()
        {
            lock (_lockObject)
            {
                _paused = true;
                _logger.LogInformation("Injection paused");
            }
        }

        /// <summary>
        /// Resume injection (service continue)
        /// </summary>
        public void Resume()
        {
            lock (_lockObject)
            {
                _paused = false;
                InjectIntoExistingProcesses();
                _logger.LogInformation("Injection resumed");
            }
        }

        private void InjectIntoExistingProcesses()
        {
            try
            {
                Process[] allProcesses = Process.GetProcesses();
                
                foreach (var process in allProcesses)
                {
                    try
                    {
                        InjectIfNeeded(process);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to check process {process.ProcessName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error injecting into existing processes: {ex.Message}", ex);
            }
        }

        private void StartProcessMonitoring()
        {
            try
            {
                WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace");
                _processWatcher = new ManagementEventWatcher(query);
                _processWatcher.EventArrived += OnProcessStarted;
                _processWatcher.Start();

                _logger.LogInformation("Process monitoring started");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to start process monitoring: {ex.Message}", ex);
                _processWatcher?.Dispose();
                _processWatcher = null;
            }
        }

        private void StopProcessMonitoring()
        {
            try
            {
                if (_processWatcher != null)
                {
                    _processWatcher.Stop();
                    _processWatcher.EventArrived -= OnProcessStarted;
                    _processWatcher.Dispose();
                    _processWatcher = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error stopping process monitoring: {ex.Message}");
            }
        }

        private void OnProcessStarted(object sender, EventArrivedEventArgs e)
        {
            try
            {
                if (_paused || !_running)
                    return;

                string? processName = e.NewEvent.Properties["ProcessName"]?.Value?.ToString();
                if (string.IsNullOrEmpty(processName))
                    return;

                // Get the process by name to inject
                string filename = Path.GetFileName(processName);
                Process[]? processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(filename));

                foreach (var process in processes)
                {
                    try
                    {
                        InjectIfNeeded(process);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to inject into {process.ProcessName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error in process start event: {ex.Message}");
            }
        }

        private void InjectIfNeeded(Process process)
        {
            lock (_lockObject)
            {
                if (!_running || process == null || process.HasExited)
                    return;

                int pid = process.Id;

                // Already injected
                if (_injectedProcesses.Contains(pid))
                    return;

                string processName = Path.GetFileName(process.MainModule?.FileName ?? process.ProcessName);

                // Check if should inject
                if (!_configManager.ShouldInjectProcess(processName))
                    return;

                try
                {
                    InjectHookDll(process);
                    _injectedProcesses.Add(pid);

                    _logger.LogInformation($"Successfully injected into {processName} (PID: {pid})");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Injection failed for {processName}: {ex.Message}", ex);
                }
            }
        }

        private void InjectHookDll(Process process)
        {
            // This is a placeholder for the actual injection logic
            // Real implementation would use:
            // 1. Remote thread creation (CreateRemoteThread)
            // 2. LoadLibrary call in target process
            // 3. Cleanup and verification

            if (!File.Exists(HOOK_DLL))
                throw new FileNotFoundException($"Hook DLL not found: {HOOK_DLL}");

            // For now, we'll use P/Invoke to load the DLL into the process
            // In production, this would use more sophisticated injection techniques
            try
            {
                // TODO: Implement actual DLL injection
                // This requires:
                // - OpenProcess(PROCESS_ALL_ACCESS, false, pid)
                // - VirtualAllocEx (allocate memory for DLL path)
                // - WriteProcessMemory (write path to allocated memory)
                // - CreateRemoteThread (call LoadLibraryA with DLL path)
                // - WaitForSingleObject (wait for thread completion)
                
                _logger.LogInformation($"Hook DLL injection queued for {process.ProcessName}");
            }
            catch (Exception ex)
            {
                throw new Exception($"DLL injection failed: {ex.Message}", ex);
            }
        }

        private void UnInjectFromAllProcesses()
        {
            try
            {
                List<int> toRemove = new();

                foreach (int pid in _injectedProcesses)
                {
                    try
                    {
                        Process? process = Process.GetProcessById(pid);
                        if (process != null && !process.HasExited)
                        {
                            UnInjectHookDll(process);
                        }
                        toRemove.Add(pid);
                    }
                    catch (ArgumentException)
                    {
                        // Process no longer exists
                        toRemove.Add(pid);
                    }
                }

                foreach (int pid in toRemove)
                {
                    _injectedProcesses.Remove(pid);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uninjecting from processes: {ex.Message}", ex);
            }
        }

        private void UnInjectHookDll(Process process)
        {
            try
            {
                // TODO: Implement actual DLL uninjection
                _logger.LogInformation($"Hook DLL uninjection queued for {process.ProcessName}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to uninjection from {process.ProcessName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Get list of injected processes
        /// </summary>
        public List<string> GetInjectedProcesses()
        {
            lock (_lockObject)
            {
                var result = new List<string>();

                foreach (int pid in _injectedProcesses)
                {
                    try
                    {
                        Process? process = Process.GetProcessById(pid);
                        if (process != null && !process.HasExited)
                        {
                            result.Add($"{process.ProcessName} (PID: {pid})");
                        }
                    }
                    catch (ArgumentException)
                    {
                        // Process no longer exists
                    }
                }

                return result;
            }
        }
    }
}
