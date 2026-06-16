using ShopProject.Db.Interfaces;
using ShopProject.Models;
using ShopProject.Services.Interfaces;
using System;

namespace ShopProject.Services
{
    public class ModeratorService : IModeratorService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;

        public ModeratorService(IUserRepository userRepository, IAuthService authService)
        {
            _userRepository = userRepository;
            _authService = authService;
        }

        public void ViewUserProfile(string email)
        {
            var currentUser = _authService.RequireUser();

            if (!PermissionService.CanModerate(currentUser.Role))
            {
                throw new Exception("Только модераторы и администраторы могут просматривать профили других пользователей");
            }
            var user = _userRepository.GetByEmail(email);
            if (user == null)
            {
                throw new Exception($"Пользователь с email '{email}' не найден");
            }
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"{"Профиль пользователя",-40}");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"{"ID:",-15} {user.Id}");
            Console.WriteLine($"{"Имя:",-15} {user.Name}");
            Console.WriteLine($"{"Email:",-15} {user.Email}");
            Console.WriteLine($"{"Роль:",-15} {user.Role}");
            Console.WriteLine($"{"Баланс:",-15} {user.Balance} руб.");
            Console.WriteLine($"{"Статус:",-15} {(user.IsBlocked ? "Заблокирован" : "Активен")}");
            Console.WriteLine(new string('=', 50));
        }

        public void ChangeUserBalance(string email, decimal newBalance)
        {
            var currentUser = _authService.RequireUser();
            if (!PermissionService.CanModerate(currentUser.Role))
            {
                throw new Exception("Только модераторы и администраторы могут изменять баланс пользователей");
            }
            if (newBalance < 0)
            {
                throw new Exception("Баланс не может быть отрицательным");
            }
            var user = _userRepository.GetByEmail(email);
            if (user == null)
            {
                throw new Exception($"Пользователь с email '{email}' не найден");
            }
            var oldBalance = user.Balance;
            user.Balance = newBalance;
            _userRepository.Update(user);
            Console.WriteLine($"Баланс пользователя {user.Name} изменён: {oldBalance} руб. → {newBalance} руб.");
        }

        public void ToggleBlockUser(string email)
        {
            var currentUser = _authService.RequireUser();
            if (!PermissionService.CanModerate(currentUser.Role))
            {
                throw new Exception("Только модераторы и администраторы могут блокировать пользователей");
            }
            var user = _userRepository.GetByEmail(email);
            if (user == null)
            {
                throw new Exception($"Пользователь с email '{email}' не найден");
            }
            if (user.Id == currentUser.Id)
            {
                throw new Exception("Нельзя заблокировать самого себя");
            }
            user.IsBlocked = !user.IsBlocked;
            _userRepository.Update(user);

            var status = user.IsBlocked ? "заблокирован" : "разблокирован";
            Console.WriteLine($"Пользователь {user.Name} {status}");
        }
    }
}