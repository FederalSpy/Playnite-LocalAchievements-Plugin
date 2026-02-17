using System;
using System.IO;
using Playnite.SDK;

namespace LocalAchievements.Services
{
    public static class ExtendedLogger
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static string pluginDataPath;
        private static readonly object fileLock = new object();

        public static void Initialize(string path)
        {
            pluginDataPath = path;
            try
            {
                if (!string.IsNullOrEmpty(pluginDataPath))
                {
                    string logsDir = Path.Combine(pluginDataPath, "Logs");
                    if (!Directory.Exists(logsDir))
                    {
                        Directory.CreateDirectory(logsDir);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error inicializando ExtendedLogger");
            }
        }

        public static void Info(string message)
        {
            logger.Info(message);
            WriteToFile("INFO", message);
        }

        public static void Warn(string message)
        {
            logger.Warn(message);
            WriteToFile("WARN", message);
        }

        public static void Error(string message, Exception ex = null)
        {
            logger.Error(ex, message);
            string detail = ex != null ? $" | Exception: {ex.Message} {ex.StackTrace}" : "";
            WriteToFile("ERROR", message + detail);
        }

        public static void Debug(string message)
        {
            logger.Debug(message);
            WriteToFile("DEBUG", message);
        }

        private static void WriteToFile(string level, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(pluginDataPath)) return;

                // Usamos lock para evitar errores si varios hilos intentan escribir a la vez
                lock (fileLock)
                {
                    string logFile = Path.Combine(pluginDataPath, "Logs", $"plugin-{DateTime.Now:yyyyMMdd}.log");
                    string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}{Environment.NewLine}";

                    File.AppendAllText(logFile, logEntry);
                }
            }
            catch
            {
                // Si falla el log en archivo, no podemos hacer mucho más que ignorarlo 
                // para no tumbar la aplicación.
            }
        }
    }
}