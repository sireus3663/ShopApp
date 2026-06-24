using System.Runtime.InteropServices;
using ShopProject.ConsoleCommands;
using ShopProject.ConsoleCommands.BaseCommands;
using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;

namespace ShopProject
{
    public static class ConsoleMode
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        public static void Run(string[] args)
        {
            AllocConsole();

            Console.Title = "Shop Admin Console";
            Console.Clear();

            string autoLoginEmail = null;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--auto-login" && i + 1 < args.Length)
                {
                    autoLoginEmail = args[i + 1];
                    break;
                }
            }

            var logger = new LoggerService();
            var configService = new AppConfigService();
            var startup = new StartupService(logger, configService);

            AppDbContext? context = startup.TryConnect();

            var userRepo = new UserRepository(context);
            var productRepo = new ProductRepository(context);
            var cartRepo = new CartRepository(context);
            var orderRepo = new OrderRepository(context);
            var favoriteRepo = new FavoriteRepository(context);
            var discountRepo = new DiscountRepository(context);

            var authService = new AuthService(context);
            var userService = new UserService(context, authService, logger);
            var productService = new ProductService(productRepo, authService);
            var discountService = new DiscountService(discountRepo, authService, productRepo);
            var cartService = new CartService(cartRepo, authService);
            var favoriteService = new FavoriteService(favoriteRepo, authService);
            var orderService = new OrderService(authService, cartService, orderRepo, productRepo, userRepo, discountService, context);
            var moderatorService = new ModeratorService(userRepo, authService);

            if (!string.IsNullOrEmpty(autoLoginEmail))
            {
                var user = userRepo.GetByEmail(autoLoginEmail);
                if (user != null)
                {
                    authService.LoginById(user);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[OK] Автоматический вход: {user.Name} (Роль: {user.Role})");
                    Console.ResetColor();
                    logger.Info($"Авто-вход пользователя {user.Email} из WinForms консоли");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[WARNING] Пользователь {autoLoginEmail} не найден");
                    Console.ResetColor();
                }
            }

            var registry = new CommandRegistry(logger);
            CommandInitializer.RegisterAll(registry, logger, authService, userService, cartService,
                favoriteService, productService, orderService, moderatorService, discountService,
                productRepo, orderRepo, userRepo, context);

            var menuCommand = new MenuCommand(authService, registry);

            while (true)
            {
                var user = authService.currentUser;
                Console.Write(user != null ? $"\n[{user.Name}]> " : "\n> ");

                var input = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(input)) continue;

                if (input.ToLower() == "menu")
                {
                    menuCommand.Execute(Array.Empty<string>());
                }
                else if (input.ToLower() == "help")
                {
                    registry.ShowHelp();
                }
                else if (input.ToLower() == "clear")
                {
                    Console.Clear();
                    Console.WriteLine("Экран очищен");
                }
                else if (input.ToLower() == "exit")
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("До свидания!");
                    Console.ResetColor();
                    break;
                }
                else
                {
                    var parts = input.Split(' ');
                    if (parts.Length >= 2)
                    {
                        var twoWordCommand = $"{parts[0]} {parts[1]}";
                        var command = registry.Get(twoWordCommand);
                        if (command != null)
                        {
                            var cmdArgs = parts.Skip(2).ToArray();
                            registry.Execute(command.Name, cmdArgs);
                            continue;
                        }
                    }
                    var cmdName = parts[0];
                    var cmdArgs2 = parts.Skip(1).ToArray();

                    var command2 = registry.Get(cmdName);
                    if (command2 != null)
                    {
                        registry.Execute(cmdName, cmdArgs2);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Команда '{cmdName}' не найдена. Введите 'help' для списка команд.");
                        Console.ResetColor();
                    }
                }
            }
        }
    }
}