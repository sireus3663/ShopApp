using ShopProject.ConsoleCommands;
using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Db;
using ShopProject.Db.Interfaces;
using ShopProject.Models;
using ShopProject.Services;
using ShopProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ShopProject
{
    public class ConsoleMode
    {
        private readonly ILoggerService _logger;
        private readonly IAppConfigService _configService;
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly IFavoriteService _favoriteService;
        private readonly IOrderService _orderService;
        private readonly IModeratorService _moderatorService;
        private readonly IDiscountService _discountService;
        private readonly IProductRepository _productRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IUserRepository _userRepo;
        private readonly AppDbContext _context;
        private readonly CommandRegistry _registry;

        public ConsoleMode(
            ILoggerService logger,
            IAppConfigService configService,
            IAuthService authService,
            IUserService userService,
            IProductService productService,
            ICartService cartService,
            IFavoriteService favoriteService,
            IOrderService orderService,
            IModeratorService moderatorService,
            IDiscountService discountService,
            IProductRepository productRepo,
            IOrderRepository orderRepo,
            IUserRepository userRepo,
            AppDbContext context,
            CommandRegistry registry)
        {
            _logger = logger;
            _configService = configService;
            _authService = authService;
            _userService = userService;
            _productService = productService;
            _cartService = cartService;
            _favoriteService = favoriteService;
            _orderService = orderService;
            _moderatorService = moderatorService;
            _discountService = discountService;
            _productRepo = productRepo;
            _orderRepo = orderRepo;
            _userRepo = userRepo;
            _context = context;
            _registry = registry;
        }

        public void Run(string autoLoginEmail = null)
        {
            ShowBanner();

            // Проверяем подключение к БД
            if (!CheckDatabaseConnection())
                return;

            // Создаем тестовых пользователей
            CreateTestUsers();

            // Восстанавливаем сессию
            RestoreSession();

            // Автоматический вход
            if (!string.IsNullOrEmpty(autoLoginEmail))
            {
                AutoLogin(autoLoginEmail);
            }

            // Регистрируем команды
            CommandInitializer.RegisterAll(
                _registry,
                _logger,
                _authService,
                _userService,
                _cartService,
                _favoriteService,
                _productService,
                _orderService,
                _moderatorService,
                _discountService,
                _productRepo,
                _orderRepo,
                _userRepo,
                _context
            );

            ShowWelcomeMessage();

            var menuCommand = new MenuCommand(_authService, _registry);

            while (true)
            {
                var user = _authService.currentUser;
                Console.Write(user != null ? $"\n[{user.Name}]> " : "\n> ");

                var input = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(input)) continue;

                if (input.ToLower() == "menu")
                {
                    menuCommand.Execute(Array.Empty<string>());
                }
                else if (input.ToLower() == "help")
                {
                    _registry.ShowHelp();
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

                    var command = _registry.Get(cmdName);
                    if (command != null)
                    {
                        _registry.Execute(cmdName, cmdArgs);
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

        private bool CheckDatabaseConnection()
        {
            try
            {
                _context.Database.OpenConnection();
                _context.Database.CloseConnection();
                _context.Database.Migrate();
                _logger.Info("Подключение к БД установлено");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка подключения к БД", ex);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка подключения к базе данных. Проверьте настройки.");
                Console.ResetColor();
                return false;
            }
        }

        private void CreateTestUsers()
        {
            CreateTestUser("admin@shop.com", "Administrator", "Admin123", Role.Admin, 999999);
            CreateTestUser("moder@shop.com", "Moderator", "Moder123", Role.Moderator, 10000);
            CreateTestUser("seller@shop.com", "Seller", "Seller123", Role.Seller, 50000);
            CreateTestUser("buyer@shop.com", "Buyer", "Buyer123", Role.Buyer, 10000);
        }

        private void CreateTestUser(string email, string name, string password, Role role, decimal balance)
        {
            if (!_userRepo.Exists(email))
            {
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Email = email,
                    Balance = balance,
                    Role = role,
                    IsBlocked = false
                };
                user.SetPassword(password);
                _userRepo.Add(user);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[OK] Создан {role}: {email} / {password}");
                Console.ResetColor();
            }
        }

        private void RestoreSession()
        {
            var userId = _configService.GetCurrentUserId();
            if (userId == null) return;

            try
            {
                var user = _userRepo.GetById(userId.Value);
                if (user != null)
                {
                    _authService.LoginById(user);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[OK] Сессия восстановлена: {user.Name} ({user.Role})");
                    Console.ResetColor();
                    _logger.Info($"Сессия восстановлена для пользователя {user.Email}");
                }
                else
                {
                    _configService.SetCurrentUserId(null);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Ошибка при восстановлении сессии", ex);
                _configService.SetCurrentUserId(null);
            }
        }

        private void AutoLogin(string email)
        {
            var user = _userRepo.GetByEmail(email);
            if (user != null)
            {
                _authService.LoginById(user);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[OK] Автоматический вход: {user.Name} (Роль: {user.Role})");
                Console.ResetColor();
                _logger.Info($"Авто-вход пользователя {user.Email} из WinForms консоли");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[WARNING] Пользователь {email} не найден");
                Console.ResetColor();
            }
        }

        private static void ShowBanner()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
╔══════════════════════════════════════════════════════╗
║                                                      ║
║   ███████╗██╗  ██╗ ██████╗ ██████╗  █████╗ ██████╗  ║
║   ██╔════╝██║  ██║██╔═══██╗██╔══██╗██╔══██╗██╔══██╗ ║
║   ███████╗███████║██║   ██║██████╔╝███████║██████╔╝ ║
║   ╚════██║██╔══██║██║   ██║██╔═══╝ ██╔══██║██╔═══╝  ║
║   ███████║██║  ██║╚██████╔╝██║     ██║  ██║██║      ║
║   ╚══════╝╚═╝  ╚═╝ ╚═════╝ ╚═╝     ╚═╝  ╚═╝╚═╝      ║
║                                                      ║
║           Добро пожаловать в ShopApp!                ║
╚══════════════════════════════════════════════════════╝
");
            Console.ResetColor();
        }

        private void ShowWelcomeMessage()
        {
            Console.WriteLine("\n📌 Тестовые пользователи:");
            Console.WriteLine($"  🔑 Админ:      admin@shop.com / Admin123");
            Console.WriteLine($"  🔑 Модератор:  moder@shop.com / Moder123");
            Console.WriteLine($"  🔑 Продавец:   seller@shop.com / Seller123");
            Console.WriteLine($"  🔑 Покупатель: buyer@shop.com / Buyer123");
            Console.WriteLine();
            Console.WriteLine("💡 Введите 'menu' для просмотра доступных команд");
            Console.WriteLine("💡 Введите 'help' для детальной справки");
            Console.WriteLine();
        }
    }
}