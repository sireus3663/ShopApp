using ShopProject.Models;
using ShopProject.Db;
using ShopProject.Services;

// подключение к БД
var context = new AppDbContext();

// создание репозиториев и сервисов
var userRepo = new UserRepository(context);
var authService = new AuthService(context);
var userService = new UserService(context, authService);


//тест регистрации и логина (не стирать)
/*string email = "mega@gmail.com";
string password = "coolPassword123";
try
{
    userService.Register("coolUser123", email, password);

}
catch (Exception ex) 
{
    Console.WriteLine(ex.Message);
}
try
{
    authService.Login(email, password);

}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
Console.WriteLine(authService.currentUser == null ? "не залогинен" : authService.currentUser.Name);

if (!userRepo.Exists("adminAdmin"))
{
    var admin = new User { Name = "admin", Role = Role.Admin, Balance = 999999, Email = "adminAdmin", Password = "ShopAdminPassword" };
    userRepo.Add(admin);
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


