using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace MacFontRenderer.Service.Utils
{
    public class EventLogger
    {
        private readonly string _serviceName;
        private readonly string _logPath;
        private readonly object _lockObject = new();
        private const int MAX_LOG_SIZE = 10 * 1024 * 1024; // 10 MB

        public EventLogger(string serviceName)
        {
            _serviceName = serviceName;
            _logPath = Path.Combine(@"C:\ProgramData", 
                                   "MacFontRenderer", "Logs");
            
            EnsureLogDirectory();
        }

        /// <summary>
        /// Log informational message
        /// </summary>
        public void LogInformation(string message)
        {
            LogEvent(EventLogEntryType.Information, message);
        }

        /// <summary>
        /// Log warning message
        /// </summary>
        public void LogWarning(string message)
        {
            LogEvent(EventLogEntryType.Warning, message);
        }

        /// <summary>
        /// Log error message with exception
        /// </summary>
        public void LogError(string message, Exception? ex = null)
        {
            string fullMessage = ex != null ? 
                $"{message}\n\nException: {ex.GetType().Name}\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}" :
                message;
            
            LogEvent(EventLogEntryType.Error, fullMessage);
        }

        private void LogEvent(EventLogEntryType type, string message)
        {
            lock (_lockObject)
            {
                try
                {
                    // Write to Event Log
                    try
                    {
                        if (!EventLog.SourceExists(_serviceName))
                        {
                            EventLog.CreateEventSource(_serviceName, "Application");
                        }

                        using (EventLog log = new EventLog("Application"))
                        {
                            log.Source = _serviceName;
                            log.WriteEntry(message, type, 1000);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Fallback if can't write to Event Log
                    }

                    // Write to file
                    WriteToFile(type, message);
                }
                catch (Exception ex)
                {
                    // Last resort: write to console
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{type}] {message}");
                    if (ex != null)
                    {
                        Console.WriteLine($"Logging error: {ex.Message}");
                    }
                }
            }
        }

        private void WriteToFile(EventLogEntryType type, string message)
        {
            try
            {
                string logFile = Path.Combine(_logPath, $"{DateTime.Now:yyyy-MM-dd}.log");
                
                // Check and rotate if needed
                if (File.Exists(logFile) && new FileInfo(logFile).Length > MAX_LOG_SIZE)
                {
                    string backupFile = Path.Combine(_logPath, $"{DateTime.Now:yyyy-MM-dd-HHmmss}_backup.log");
                    if (File.Exists(backupFile))
                        File.Delete(backupFile);
                    File.Move(logFile, backupFile);
                }

                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{type}] {message}\n";
                
                using (var writer = new StreamWriter(logFile, true, Encoding.UTF8))
                {
                    writer.Write(logEntry);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }

        private void EnsureLogDirectory()
        {
            try
            {
                if (!Directory.Exists(_logPath))
                {
                    Directory.CreateDirectory(_logPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create log directory: {ex.Message}");
            }
        }

        /// <summary>
        /// Get recent log entries
        /// </summary>
        public string[] GetRecentLogs(int lines = 100)
        {
            try
            {
                string logFile = Path.Combine(_logPath, $"{DateTime.Now:yyyy-MM-dd}.log");
                
                if (!File.Exists(logFile))
                    return Array.Empty<string>();

                var allLines = File.ReadAllLines(logFile);
                int start = Math.Max(0, allLines.Length - lines);
                
                return allLines.Skip(start).ToArray();
            }
            catch (Exception ex)
            {
                return new[] { $"Error reading logs: {ex.Message}" };
            }
        }
    }
}
