using ShopProject.ConsoleCommands;
using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;

System.Windows.Forms.Application.EnableVisualStyles();
System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

var logger        = new LoggerService();
var configService = new AppConfigService();
var startup       = new StartupService(logger, configService);

logger.Info(startup.IsFirstLaunch() ? "Первый запуск" : "Запуск приложения");

AppDbContext? context = startup.TryConnect();

if (context == null)
{
    var dbForm = new ShopProject.Forms.DbConnectionForm(configService);
    if (dbForm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
    {
        logger.Warning("Пользователь отказался от подключения. Завершение.");
        return;
    }
    context = startup.TryConnect();
    if (context == null)
    {
        System.Windows.Forms.MessageBox.Show(
            "Не удалось подключиться к базе данных.",
            "Ошибка", System.Windows.Forms.MessageBoxButtons.OK,
            System.Windows.Forms.MessageBoxIcon.Error);
        return;
    }
}

logger.Info("Подключение к БД установлено");

var userRepo     = new UserRepository(context);
var productRepo  = new ProductRepository(context);
var cartRepo     = new CartRepository(context);
var orderRepo    = new OrderRepository(context);
var favoriteRepo = new FavoriteRepository(context);
var discountRepo = new DiscountRepository(context);

var authService      = new AuthService(context, configService);
var userService      = new UserService(context, authService, logger);
var productService   = new ProductService(productRepo, authService);
var discountService  = new DiscountService(discountRepo, authService, productRepo);
var cartService      = new CartService(cartRepo, authService);
var favoriteService  = new FavoriteService(favoriteRepo, authService);
var orderService     = new OrderService(authService, cartService, orderRepo, productRepo, userRepo, discountService);
var moderatorService = new ModeratorService(userRepo, authService);

startup.RestoreSession(context, authService);

if (!userRepo.Exists("admin@shop.com"))
    userRepo.Add(new User { Id = Guid.NewGuid(), Name = "Administrator",
        Email = "admin@shop.com", Password = "Admin123",
        Balance = 999999, Role = Role.Admin, IsBlocked = false });

if (!userRepo.Exists("moder@shop.com"))
    userRepo.Add(new User { Id = Guid.NewGuid(), Name = "Moderator",
        Email = "moder@shop.com", Password = "Moder123",
        Balance = 10000, Role = Role.Moderator, IsBlocked = false });

if (!userRepo.Exists("seller@shop.com"))
    userRepo.Add(new User { Id = Guid.NewGuid(), Name = "Seller",
        Email = "seller@shop.com", Password = "Seller123",
        Balance = 50000, Role = Role.Seller, IsBlocked = false });

if (!userRepo.Exists("buyer@shop.com"))
    userRepo.Add(new User { Id = Guid.NewGuid(), Name = "Buyer",
        Email = "buyer@shop.com", Password = "Buyer123",
        Balance = 10000, Role = Role.Buyer, IsBlocked = false });

var registry = new CommandRegistry(logger);
CommandInitializer.RegisterAll(registry, logger, authService, userService, cartService,
    favoriteService, productService, orderService, moderatorService, discountService,
    productRepo, orderRepo, userRepo, context);

var loginForm = new ShopProject.Forms.LoginForm(authService, userService);
if (loginForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
    System.Windows.Forms.Application.Run(
        new ShopProject.Forms.MainForm(authService, userService, context));
