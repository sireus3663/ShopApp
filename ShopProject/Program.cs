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
try
{
    authService.Login("adminAdmin", "ShopAdminPassword");

}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
try
{
    Console.WriteLine(authService.currentUser.Role);
    userService.ChangeRole(userRepo.GetByEmail(email).Id, Role.Moderator);

}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}*/

































































//------------------------
try
{
    authService.Login("adminAdmin", "ShopAdminPassword");

}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
var productRepo = new ProductRepository(context);
var productService = new ProductService(productRepo, authService);
var cartRepo = new CartRepository(context);
var cartService = new CartService(cartRepo, authService);
var orderRepo = new OrderRepository(context);
var discountRepo = new DiscountRepository(context);
var discountService = new DiscountService(discountRepo, authService, productRepo);
var orderService = new OrderService(authService, cartService, orderRepo, productRepo, userRepo, discountService);
var lst = new List<Product>();
lst = productService.GetAllApproved();
try
{
    /*foreach (var item in lst)
    {
        cartService.AddToCart(item.Id);
    }*/
    orderService.BuyCart();
}
catch (Exception ex)
{
    Console.WriteLine(ex.InnerException?.Message);
}


