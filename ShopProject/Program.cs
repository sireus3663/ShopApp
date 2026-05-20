using ShopProject.Models;
using ShopProject.Db;

// подключение к БД
var context = new AppDbContext();

// создаём репозиторий
var userRepo = new UserRepository(context);

var newUser = new User
{
    Name = "test_user",
    Email = "test@mail.com",
    Password = "1234",
    Balance = 1000,
    Role = Role.Buyer
};

//userRepo.Add(newUser);
var userFromDb = userRepo.GetByEmail("test@mail.com");

Console.WriteLine(userFromDb.Role);