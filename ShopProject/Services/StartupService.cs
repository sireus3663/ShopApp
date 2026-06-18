using Microsoft.EntityFrameworkCore;
using ShopProject.Db;
using ShopProject.Models;

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
        catch
        {
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

    public void InitializeOnFirstLaunch(AppDbContext context)
    {
        if (!IsFirstLaunch()) return;
        _logger.Info("Первый запуск приложения — создание тестовых пользователей");
        SeedTestUsers(context);
    }

    private void SeedTestUsers(AppDbContext context)
    {
        var userRepo = new UserRepository(context);
        var users = new[]
        {
            new { Email = "admin@shop.com", Password = "Admin123", Name = "Administrator", Balance = 999999M, Role = Role.Admin },
            new { Email = "moder@shop.com", Password = "Moder123", Name = "Moderator", Balance = 10000M, Role = Role.Moderator },
            new { Email = "seller@shop.com", Password = "Seller123", Name = "Seller", Balance = 50000M, Role = Role.Seller },
            new { Email = "buyer@shop.com", Password = "Buyer123", Name = "Buyer", Balance = 10000M, Role = Role.Buyer },
        };

        foreach (var u in users)
        {
            if (!userRepo.Exists(u.Email))
            {
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Name = u.Name,
                    Email = u.Email,
                    Balance = u.Balance,
                    Role = u.Role,
                    IsBlocked = false
                };
                user.SetPassword(u.Password);
                userRepo.Add(user);
                _logger.Info($"Создан тестовый пользователь: {u.Email}");
            }
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
