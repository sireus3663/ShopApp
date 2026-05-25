using ShopProject.ConsoleCommands;
using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;

// подключение к БД
var context = new AppDbContext();

// логгер
var logger = new LoggerService();

// создание репозиториев и сервисов
var userRepo = new UserRepository(context);
var authService = new AuthService(context);
var userService = new UserService(context, authService, logger);

// Регистрация команд
var registry = new CommandRegistry(logger);
CommandInitializer.RegisterAll(registry, logger, userService);

Console.WriteLine("=== ShopProject ====");
Console.WriteLine("Введите help для списка команд\n");
while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(input)) continue;

    var parts = input.Split(' ');
    var cmdName = parts[0];           // ← переименовано: commandName → cmdName
    var cmdArgs = parts.Skip(1).ToArray();  // ← переименовано: args → cmdArgs

    registry.Execute(cmdName, cmdArgs);  // ← выполнение комман
}
