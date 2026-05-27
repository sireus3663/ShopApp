using ShopProject.ConsoleCommands;
using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;

var context = new AppDbContext();

var logger = new LoggerService();

// репозитории
var userRepo = new UserRepository(context);
var productRepo = new ProductRepository(context);
var cartRepo = new CartRepository(context);
var orderRepo = new OrderRepository(context);
var discountRepo = new DiscountRepository(context);
var favoriteRepo = new FavoriteRepository(context);

// очистка тестовых данных
Console.WriteLine("Очистка тестовых данных...");

var testEmails = new[]
{
    "admin@test.com",
    "seller@test.com",
    "buyer@test.com"
};

foreach (var email in testEmails)
{
    if (userRepo.Exists(email))
    {
        var user = userRepo.GetByEmail(email);

        foreach (var cart in cartRepo.GetByUser(user.Id))
            cartRepo.Delete(cart.Id);

        foreach (var order in orderRepo.GetByUser(user.Id))
            orderRepo.Delete(order.Id);

        foreach (var fav in favoriteRepo.GetByUser(user.Id))
            favoriteRepo.Delete(fav.Id);

        userRepo.Delete(user.Id);

        Console.WriteLine($"Удален {email}");
    }
}

Console.WriteLine("Очистка завершена\n");

// сервисы
var authService = new AuthService(context);

var userService = new UserService(
    context,
    authService,
    logger
);

var productService = new ProductService(
    productRepo,
    authService
);

var cartService = new CartService(
    cartRepo,
    authService
);

var favoriteService = new FavoriteService(
    favoriteRepo,
    authService
);

var discountService = new DiscountService(
    discountRepo,
    authService,
    productRepo
);

var orderService = new OrderService(
    authService,
    cartService,
    orderRepo,
    productRepo,
    userRepo,
    discountService
);

var statisticService = new StatisticService(
    orderRepo,
    productRepo,
    discountService
);

// команды
var registry = new CommandRegistry(logger);
CommandInitializer.RegisterAll(registry, logger, userService);

Console.WriteLine("=== TEST START ===\n");

try
{
    // =========================
    // СОЗДАНИЕ АДМИНА
    // =========================

    Console.WriteLine("1. Создание администратора");

    var admin = new User
    {
        Id = Guid.NewGuid(),
        Name = "Admin",
        Email = "admin@test.com",
        Password = "admin123",
        Balance = 1000000,
        Role = Role.Admin
    };

    userRepo.Add(admin);

    Console.WriteLine($"Администратор создан: {admin.Email}");

    // =========================
    // ЛОГИН АДМИНА
    // =========================

    Console.WriteLine("\n2. Логин администратора");

    authService.Login(
        "admin@test.com",
        "admin123"
    );

    Console.WriteLine($"Вход выполнен: {authService.currentUser.Email}");

    // =========================
    // СОЗДАНИЕ ПРОДАВЦА
    // =========================

    Console.WriteLine("\n3. Создание продавца");

    var seller = userService.Register(
        "Seller",
        "seller@test.com",
        "password123"
    );

    userService.ChangeRole(
        seller.Id,
        Role.Seller
    );

    Console.WriteLine($"Продавец создан: {seller.Email}");

    // =========================
    // СОЗДАНИЕ ПОКУПАТЕЛЯ
    // =========================

    Console.WriteLine("\n4. Создание покупателя");

    var buyer = userService.Register(
        "Buyer",
        "buyer@test.com",
        "password123"
    );

    buyer.Balance = 100000;
    userRepo.Update(buyer);

    Console.WriteLine($"Покупатель создан: {buyer.Email}");

    authService.Logout();

    // =========================
    // ЛОГИН ПРОДАВЦА
    // =========================

    Console.WriteLine("\n5. Логин продавца");

    authService.Login(
        "seller@test.com",
        "password123"
    );

    // =========================
    // СОЗДАНИЕ ТОВАРА
    // =========================

    Console.WriteLine("\n6. Создание товара");

    var product = productService.createProduct(
        "Gaming Laptop",
        "RTX laptop",
        75000,
        "Electronics",
        5
    );

    Console.WriteLine($"Товар создан: {product.Name}");

    authService.Logout();

    // =========================
    // АДМИН ОДОБРЯЕТ ТОВАР
    // =========================

    Console.WriteLine("\n7. Одобрение товара");

    authService.Login(
        "admin@test.com",
        "admin123"
    );

    productService.Approve(product.Id);

    Console.WriteLine("Товар одобрен");

    authService.Logout();

    // =========================
    // ПРОДАВЕЦ СОЗДАЕТ СКИДКУ
    // =========================

    Console.WriteLine("\n8. Создание скидки");

    authService.Login(
        "seller@test.com",
        "password123"
    );

    var discount = discountService.CreateDiscount(
        product,
        15
    );

    Console.WriteLine($"Скидка: {discount.Percent}%");

    authService.Logout();

    // =========================
    // ПОКУПАТЕЛЬ
    // =========================

    Console.WriteLine("\n9. Логин покупателя");

    authService.Login(
        "buyer@test.com",
        "password123"
    );

    // =========================
    // КОРЗИНА
    // =========================

    Console.WriteLine("\n10. Добавление в корзину");

    cartService.AddToCart(product.Id);
    cartService.AddToCart(product.Id);

    var cart = cartService.GetCurrentUserCart();

    Console.WriteLine($"Товаров в корзине: {cart.Count}");
    Console.WriteLine($"Количество первого товара: {cart[0].Count}");

    // =========================
    // ИЗБРАННОЕ
    // =========================

    Console.WriteLine("\n11. Избранное");

    favoriteService.ToggleFavorite(product.Id);

    var favorites = favoriteService.GetFavorites();

    Console.WriteLine($"Товаров в избранном: {favorites.Count}");

    // =========================
    // ПОКУПКА
    // =========================

    Console.WriteLine("\n12. Покупка");

    orderService.BuyCart();

    Console.WriteLine("Покупка успешно оформлена");

    // =========================
    // СТАТИСТИКА
    // =========================

    Console.WriteLine("\n13. Статистика");

    var statistic = statisticService.GetProductStatistic(product.Id);

    Console.WriteLine($"Продано: {statistic.SalesCount}");
    Console.WriteLine($"Выручка: {statistic.Revenue}");
    Console.WriteLine($"Финальная цена: {statistic.FinalPrice}");

    Console.WriteLine("\n=== ВСЕ ТЕСТЫ ПРОЙДЕНЫ ===");
}
catch (Exception ex)
{
    Console.WriteLine("\n=== ОШИБКА ===");
    Console.WriteLine(ex.Message);
}

Console.WriteLine("\nКонсоль команд запущена");

while (true)
{
    Console.Write("> ");

    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
        continue;

    var parts = input.Split(' ');

    var commandName = parts[0];

    var CommandArgs = parts
        .Skip(1)
        .ToArray();

    registry.Execute(commandName, CommandArgs);
}