using ShopProject.ConsoleCommands;
using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;

// подключение к БД
var context = new AppDbContext();

// создание репозиториев и сервисов
var userRepo = new UserRepository(context);
var authService = new AuthService(context);
var userService = new UserService(context, authService);

var logger = new LoggerService();
logger.Info("Приложение запущено");

var registry = new CommandRegistry();

// Регистрация команд
registry.Register(new HelpCommand(registry));
registry.Register(new ExitCommand());
registry.Register(new EchoCommand());
registry.Register(new ShowLogsCommand(logger));
registry.Register(new TestErrorCommand(logger));

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

    var command = registry.Get(cmdName);  // ← изменено

    if (command == null)
    {
        Console.WriteLine($"Неизвестная команда: {cmdName}");
        logger.Warning($"Неизвестная команда: {cmdName}");
        continue;
    }

    try
    {
        command.Execute(cmdArgs);  // ← изменено
    }
    catch (Exception ex)
    {
        logger.Error("Ошибка выполнения команды", ex);
    }
}