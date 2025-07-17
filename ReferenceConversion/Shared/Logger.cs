using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceConversion.Shared
{
    public class Logger
    {
        private static readonly object _fileLock = new();
        private static readonly string _logDir = @"C:\Reference_Covert_Logs";
        private const long MaxLogSizeBytes = 5 * 1024 * 1024; // 5MB
        public static Action<string>? LogToUI { get; set; } = null;
        public static bool IsEnabled { get; set; } = false;


        static Logger()
        {
            TryCreateLogDirectory();
        }

        public static void LogDebug(string message) => Log("DEBUG", message, ConsoleColor.Gray);
        public static void LogInfo(string message) => Log("INFO", message, ConsoleColor.Cyan);
        public static void LogSuccess(string message) => Log("SUCCESS", message, ConsoleColor.Green);
        public static void LogWarning(string message) => Log("WARNING", message, ConsoleColor.Yellow);
        public static void LogError(string message, Exception? ex = null)
        {
            var fullMsg = ex == null ? message : $"{message}\n{ex}";
            Log("ERROR", fullMsg, ConsoleColor.Red);
        }

        private static void Log(string level, string message, ConsoleColor color)
        {
            if (!IsEnabled) return;

            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string fullMessage = $"[{time}] [{level}] {message}";

            // Visual Studio Output
            Debug.WriteLine(fullMessage);

            // Console（可選，如果你有 Console App）
            if (Environment.UserInteractive)
            {
                var originalColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine(fullMessage);
                Console.ForegroundColor = originalColor;
            }

            // File log
            try
            {
                TryCreateLogDirectory();
                string logPath = Path.Combine(_logDir, $"log_{DateTime.Now:yyyyMMdd}.txt");

                lock (_fileLock)
                {
                    if (File.Exists(logPath))
                    {
                        FileInfo fileInfo = new FileInfo(logPath);
                        if (fileInfo.Length >= MaxLogSizeBytes)
                        {
                            File.Delete(logPath);
                            Debug.WriteLine($"[Logger] Log file exceeded {MaxLogSizeBytes} bytes, deleted old file.");
                        }
                    }

                    File.AppendAllText(logPath, fullMessage + Environment.NewLine);
                }
            }
            catch (Exception fileEx)
            {
                Debug.WriteLine($"[Logger File Write Error] {fileEx.Message}");
            }

            LogToUI?.Invoke(fullMessage);
        }

        private static void TryCreateLogDirectory()
        {
            try
            {
                if (!Directory.Exists(_logDir))
                    Directory.CreateDirectory(_logDir);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Logger Directory Create Error] {ex.Message}");
            }
        }

        public static void LogSeparator()
        {
            if (!IsEnabled) return;

            string separator = new string('-', 50);

            // Visual Studio Output
            Debug.WriteLine(separator);

            // Console 輸出（如果是 Console App）
            if (Environment.UserInteractive)
            {
                Console.WriteLine(separator);
            }

            // File log（可省略，如果你不想讓分隔線寫入 log 檔）
            try
            {
                TryCreateLogDirectory();
                string logPath = Path.Combine(_logDir, $"log_{DateTime.Now:yyyyMMdd}.txt");

                lock (_fileLock)
                {
                    File.AppendAllText(logPath, separator + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Logger Separator Write Error] {ex.Message}");
            }
            LogToUI?.Invoke(separator);
        }

        public static string GetLogDirectory()
        {
            return _logDir;
        }

    }
}
