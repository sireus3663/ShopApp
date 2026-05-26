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
            Console.WriteLine("Введите пароль от базы данных (или 'exit' для выхода):");
            Console.Write("> ");
            var input = Console.ReadLine()?.Trim();

            if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            _configService.UpdateDbPassword(input ?? string.Empty);

            var context = TryConnect();
            if (context != null)
            {
                PrintSuccess("Подключение к базе данных было успешным");

                try
                {
                    context.Database.Migrate();
                }
                catch
                {
                }

                return context;
            }

            PrintError("Ошибка: Неверный пароль или ошибка соединения. Попробуйте ещё раз.");
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
