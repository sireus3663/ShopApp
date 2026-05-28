using ShopProject.Db;
using ShopProject.Models;
using Microsoft.EntityFrameworkCore;

namespace ShopProject.Services;
public class StartupService
{
    private readonly LoggerService _logger;
    private readonly AppConfigService _configService;
    private readonly string _logFilePath;

    public StartupService(LoggerService logger, AppConfigService configService,
                          string logFilePath = "logs/app.log")
    {
        _logger = logger;
        _configService = configService;
        _logFilePath = logFilePath;
    }
    public bool IsFirstLaunch()
    {
        if (!File.Exists(_logFilePath)) return true;
        return new FileInfo(_logFilePath).Length == 0;
    }
    public AppDbContext? TryConnect()
    {
        try
        {
            var context = new AppDbContext();
            context.Database.OpenConnection();
            context.Database.CloseConnection();
            context.Database.Migrate();

            return context;
        }
        catch (Exception ex)
        {
            _logger.Error("Ошибка подключения к базе данных", ex);
            return null;
        }
    }
    public AppDbContext? PromptPasswordUntilConnected()
    {
        while (true)
        {
            PrintError("Ошибка: Не удалось подключиться к базе данных.");
            Console.WriteLine("Введите данные подключения (или 'exit' для выхода):\n");

            Console.Write("  Host     (Enter = localhost): ");
            var host = Console.ReadLine()?.Trim();
            if (string.Equals(host, "exit", StringComparison.OrdinalIgnoreCase)) return null;
            if (string.IsNullOrEmpty(host)) host = "localhost";

            Console.Write("  Port     (Enter = 5432):      ");
            var port = Console.ReadLine()?.Trim();
            if (string.Equals(port, "exit", StringComparison.OrdinalIgnoreCase)) return null;
            if (string.IsNullOrEmpty(port)) port = "5432";

            Console.Write("  Database (Enter = marketplace): ");
            var database = Console.ReadLine()?.Trim();
            if (string.Equals(database, "exit", StringComparison.OrdinalIgnoreCase)) return null;
            if (string.IsNullOrEmpty(database)) database = "marketplace";

            Console.Write("  Username (Enter = postgres):  ");
            var username = Console.ReadLine()?.Trim();
            if (string.Equals(username, "exit", StringComparison.OrdinalIgnoreCase)) return null;
            if (string.IsNullOrEmpty(username)) username = "postgres";

            Console.Write("  Password:                     ");
            var password = Console.ReadLine()?.Trim() ?? string.Empty;
            if (string.Equals(password, "exit", StringComparison.OrdinalIgnoreCase)) return null;

            _configService.UpdateConnectionString(host, port, database, username, password);

            Console.WriteLine("\nПодключение");
            var context = TryConnect();

            if (context != null)
            {
                PrintSuccess("Подключение успешно. База данных обновлена. Миграции применены");
                return context;
            }

            PrintError("Ошибка: Не удалось подключиться. Проверьте данные и попробуйте снова.\n");
        }
    }

    public void RestoreSession(AppDbContext context, AuthService authService)
    {
        var userId = _configService.GetCurrentUserId();
        if (userId == null) return;

        try
        {
            var user = context.users.FirstOrDefault(u => u.Id == userId.Value);
            if (user == null)
            {
                _configService.SetCurrentUserId(null);
                return;
            }

            authService.LoginById(user);
            PrintSuccess($"Сессия восстановлена: {user.Name} ({user.Role})");
            _logger.Info($"Сессия восстановлена для пользователя {user.Email} (Id={user.Id})");
        }
        catch (Exception ex)
        {
            _logger.Error("Ошибка при восстановлении сессии", ex);
            _configService.SetCurrentUserId(null);
        }
    }

    private static void PrintError(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(msg);
        Console.ResetColor();
    }

    private static void PrintSuccess(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(msg);
        Console.ResetColor();
    }
}
