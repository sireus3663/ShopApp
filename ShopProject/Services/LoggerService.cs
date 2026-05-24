using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.Services
{
    public class LoggerService
    {
        private readonly string _logFilePath;

        public LoggerService(string logFilePath = "logs/app.log")
        {
            var directory = Path.GetDirectoryName(logFilePath);
            if(!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
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
            
            /*var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = level == LogLevel.Error ? ConsoleColor.Red : level == LogLevel.Warning ? ConsoleColor.Yellow : ConsoleColor.Gray;
            Console.WriteLine(logEntry);
            Console.ForegroundColor = originalColor;*/
        }

        public void Info(string message) => Log(LogLevel.Info, message);
        public void Warning(string message) => Log(LogLevel.Warning, message);
        public void Error(string message, Exception? ex = null) => Log(LogLevel.Error, message, ex);
    }
}
