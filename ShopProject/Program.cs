using ShopProject.ConsoleCommands;
using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;

var logger = new LoggerService();
var configService = new AppConfigService();
var startup = new StartupService(logger, configService);

bool isFirstLaunch = startup.IsFirstLaunch();
Console.WriteLine("=== ShopProject ===");

if (isFirstLaunch)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("Первый запуск программы. Подключение к базе данных");
    Console.ResetColor();
    logger.Info("Первый запуск приложения");
}
else
{
    logger.Info("Запуск приложения");
}

AppDbContext? context = startup.TryConnect();

if (context == null)
{
    context = startup.PromptPasswordUntilConnected();
    if (context == null)
    {
        logger.Warning("Пользователь отказался от подключения. Завершение.");
        Console.WriteLine("Выход из программы.");
        return;
    }
}

var userRepo = new UserRepository(context);
var authService = new AuthService(context, configService);
var userService = new UserService(context, authService, logger);

startup.RestoreSession(context, authService);

var registry = new CommandRegistry(logger);
CommandInitializer.RegisterAll(registry, logger, userService);

Console.WriteLine("Введите help для списка команд\n");

while (true)
{
    var user = authService.currentUser;
    Console.Write(user != null ? $"[{user.Name}]> " : "> ");
    var input = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(input)) continue;
    var parts = input.Split(' ');
    var cmdName = parts[0];
    var cmdArgs = parts.Skip(1).ToArray();
    registry.Execute(cmdName, cmdArgs);
}