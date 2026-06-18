using System;
using System.IO;

namespace ShopProject.Services
{
    public interface ILoggerService
    {
        void Log(LogLevel level, string message, Exception? ex = null);
        void Info(string message);
        void Warning(string message);
        void Error(string message, Exception? ex = null);
    }
}

namespace ShopProject.Services
{
    public class LoggerService : ILoggerService
    {
        private readonly string _logFilePath;

        public LoggerService(string logFilePath = "logs/app.log")
        {
            var directory = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            _logFilePath = logFilePath;
        }

        public void Log(LogLevel level, string message, Exception? ex = null)
        {
            var logEntry = $"[{DateTime.Now:dd.MM.yyyy HH:mm:ss}] [{level}] {message}";
            if (ex != null)
            {
                logEntry += $" | Exception: {ex.GetType().Name} - {ex.Message}";
            }
            File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
        }

        public void Info(string message) => Log(LogLevel.Info, message);
        public void Warning(string message) => Log(LogLevel.Warning, message);
        public void Error(string message, Exception? ex = null) => Log(LogLevel.Error, message, ex);
    }
}