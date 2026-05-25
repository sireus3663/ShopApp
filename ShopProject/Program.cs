using ShopProject.ConsoleCommands;
using ShopProject.Db;
using ShopProject.Services;

// подключение к БД
var context = new AppDbContext();

// логгер
var logger = new LoggerService();

// создание репозиториев
var userRepo = new UserRepository(context);
var productRepo = new ProductRepository(context);
var cartRepo = new CartRepository(context);
var orderRepo = new OrderRepository(context);
var favoriteRepo = new FavoriteRepository(context);
var discountRepo = new DiscountRepository(context);

// создание сервисов
var authService = new AuthService(context);
var userService = new UserService(context, authService, logger);
var productService = new ProductService(productRepo, authService);
var discountService = new DiscountService(discountRepo, authService, productRepo);
var cartService = new CartService(cartRepo, authService);
var favoriteService = new FavoriteService(favoriteRepo, authService);  
var orderService = new OrderService(authService, cartService, orderRepo, productRepo, userRepo, discountService);

// Регистрация команд
var registry = new CommandRegistry(logger);
CommandInitializer.RegisterAll(
    registry,
    logger,
    authService,
    userService,
    cartService,
    favoriteService,
    productService,
    orderService,
    productRepo,
    context
);

Console.WriteLine("=== ShopProject ====");
Console.WriteLine("Введите help для списка команд\n");

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(input)) continue;

    var parts = input.Split(' ');
    var cmdName = parts[0];
    var cmdArgs = parts.Skip(1).ToArray();

    registry.Execute(cmdName, cmdArgs);
}