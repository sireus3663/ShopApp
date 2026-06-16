using System;

namespace ShopProject.Services.Interfaces
{
    public interface ILoggerService
    {
        void Log(LogLevel level, string message, Exception? ex = null);
        void Info(string message);
        void Warning(string message);
        void Error(string message, Exception? ex = null);
    }
}