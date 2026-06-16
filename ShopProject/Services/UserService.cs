using ShopProject.Db;
using ShopProject.Db.Interfaces;
using ShopProject.Models;
using ShopProject.Services.Interfaces;
using System;
using System.Text.RegularExpressions;

namespace ShopProject.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly ILoggerService _logger;

        public UserService(IUserRepository userRepository, IAuthService authService, ILoggerService logger)
        {
            _userRepository = userRepository;
            _authService = authService;
            _logger = logger;
        }

        public User Register(string name, string email, string password)
        {
            if (_userRepository.Exists(email))
                throw new Exception("Пользователь с таким email уже существует");

            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                throw new Exception("Пароль должен содержать минимум 8 символов");

            if (!Regex.IsMatch(password, @"\d"))
                throw new Exception("Пароль должен содержать хотя бы одну цифру");

            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Имя не может быть пустым");

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = email,
                Balance = 0,
                Role = Role.Buyer,
                IsBlocked = false
            };
            newUser.SetPassword(password);

            _userRepository.Add(newUser);
            _logger.Info($"User Created [{newUser.Email}]");
            return newUser;
        }

        public void ChangeRole(Guid userId, Role newRole)
        {
            if (!PermissionService.CanAdministrate(_authService.RequireUser().Role))
            {
                throw new Exception("недостаточно прав");
            }
            var user = _userRepository.GetById(userId);
            if (user == null)
                throw new Exception("Пользователь не найден");

            user.Role = newRole;
            _userRepository.Update(user);
        }

        public void ShowProfile()
        {
            var currentUser = _authService.RequireUser();

            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"{"Профиль пользователя",-40}");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"{"ID:",-15} {currentUser.Id}");
            Console.WriteLine($"{"Имя:",-15} {currentUser.Name}");
            Console.WriteLine($"{"Email:",-15} {currentUser.Email}");
            Console.WriteLine($"{"Роль:",-15} {currentUser.Role}");
            Console.WriteLine($"{"Баланс:",-15} {currentUser.Balance} руб.");
            Console.WriteLine(new string('=', 50));
        }
    }
}