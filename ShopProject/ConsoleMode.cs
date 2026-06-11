using ShopProject.ConsoleCommands;
using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;

namespace ShopProject
{
    public static class ConsoleMode
    {
        public static void Run()
        {
            var logger = new LoggerService();
            var configService = new AppConfigService();
            var startup = new StartupService(logger, configService);

            if (startup.IsFirstLaunch())
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
                    logger.Warning("Пользователь отказался от подключения к БД. Завершение.");
                    Console.WriteLine("Выход из программы.");
                    return;
                }
                logger.Info("Подключение к БД установлено после ввода пароля");
            }
            else
            {
                logger.Info("Подключение к БД установлено");
            }

            var userRepo = new UserRepository(context);
            var productRepo = new ProductRepository(context);
            var cartRepo = new CartRepository(context);
            var orderRepo = new OrderRepository(context);
            var favoriteRepo = new FavoriteRepository(context);
            var discountRepo = new DiscountRepository(context);

            var authService = new AuthService(context, configService);
            var userService = new UserService(context, authService, logger);
            var productService = new ProductService(productRepo, authService);
            var discountService = new DiscountService(discountRepo, authService, productRepo);
            var cartService = new CartService(cartRepo, authService);
            var favoriteService = new FavoriteService(favoriteRepo, authService);
            var orderService = new OrderService(authService, cartService, orderRepo, productRepo, userRepo, discountService, context);
            var moderatorService = new ModeratorService(userRepo, authService);

            string adminEmail = "admin@shop.com";
            string adminPassword = "Admin123";
            if (!userRepo.Exists(adminEmail))
            {
                var admin = new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Administrator",
                    Email = adminEmail,
                    Balance = 999999,
                    Role = Role.Admin,
                    IsBlocked = false
                };
                admin.SetPassword(adminPassword); 
                userRepo.Add(admin);
                Console.WriteLine($"[OK] Создан администратор: {adminEmail} / {adminPassword}");
            }
            else { Console.WriteLine($"[i] Администратор уже существует: {adminEmail}"); }

            string moderatorEmail = "moder@shop.com";
            string moderatorPassword = "Moder123";
            if (!userRepo.Exists(moderatorEmail))
            {
                var moderator = new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Moderator",
                    Email = moderatorEmail,
                    Balance = 10000,
                    Role = Role.Moderator,
                    IsBlocked = false
                };
                moderator.SetPassword(moderatorPassword);
                userRepo.Add(moderator);
                Console.WriteLine($"[OK] Создан модератор: {moderatorEmail} / {moderatorPassword}");
            }
            else { Console.WriteLine($"[i] Модератор уже существует: {moderatorEmail}"); }

            string sellerEmail = "seller@shop.com";
            string sellerPassword = "Seller123";
            if (!userRepo.Exists(sellerEmail))
            {
                var seller = new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Seller",
                    Email = sellerEmail,
                    Balance = 50000,
                    Role = Role.Seller,
                    IsBlocked = false
                };
                seller.SetPassword(sellerPassword);
                userRepo.Add(seller);
                Console.WriteLine($"[OK] Создан продавец: {sellerEmail} / {sellerPassword}");
            }
            else { Console.WriteLine($"[i] Продавец уже существует: {sellerEmail}"); }

            string buyerEmail = "buyer@shop.com";
            string buyerPassword = "Buyer123";
            if (!userRepo.Exists(buyerEmail))
            {
                var buyer = new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Buyer",
                    Email = buyerEmail,
                    Balance = 10000,
                    Role = Role.Buyer,
                    IsBlocked = false
                };
                buyer.SetPassword(buyerPassword);
                userRepo.Add(buyer);
                Console.WriteLine($"[OK] Создан покупатель: {buyerEmail} / {buyerPassword}");
            }
            else { Console.WriteLine($"[i] Покупатель уже существует: {buyerEmail}"); }

            var registry = new CommandRegistry(logger);
            CommandInitializer.RegisterAll(registry, logger, authService, userService, cartService,
                favoriteService, productService, orderService, moderatorService, discountService,
                productRepo, orderRepo, userRepo, context);

            Console.WriteLine("\n=== ShopProject ====");
            Console.WriteLine("Тестовые пользователи:");
            Console.WriteLine($"  Админ:      {adminEmail} / {adminPassword}");
            Console.WriteLine($"  Модератор:  {moderatorEmail} / {moderatorPassword}");
            Console.WriteLine($"  Продавец:   {sellerEmail} / {sellerPassword}");
            Console.WriteLine($"  Покупатель: {buyerEmail} / {buyerPassword}");
            Console.WriteLine();
            Console.WriteLine("Доступные команды:");
            Console.WriteLine("  menu - Показать список доступных команд");
            Console.WriteLine("  help - Показать справку по всем командам");
            Console.WriteLine("  clear - Очистить экран");
            Console.WriteLine("  exit - Выход из программы");
            Console.WriteLine();

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
                    var cmdName = parts[0];
                    var cmdArgs = parts.Skip(1).ToArray();

                    var command = registry.Get(cmdName);
                    if (command != null)
                    {
                        registry.Execute(cmdName, cmdArgs);
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