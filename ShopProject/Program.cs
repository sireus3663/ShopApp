using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Service;
using ShopProject.Services;

// подключение к БД
var context = new AppDbContext();

// создаём репозиторий
var userRepo = new UserRepository(context);

var userService = new UserService();
var authService = new AuthService();

// Регистрация
var user = userService.Register("Тест", "test2gjdffks@mail.com", "password123");
Console.WriteLine($"Регистрация: {user.Name}");

// Вход
authService.Login("test2gjdffks@mail.com", "password123");
Console.WriteLine("Вход выполнен");

// Смена роли
userService.ChangeRole(user.Id, ShopProject.Models.Role.Seller);
Console.WriteLine("Роль изменена");

