using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Db.Interfaces;
using ShopProject.Models;
using ShopProject.Services.Interfaces;
using System;

namespace ShopProject.ConsoleCommands.UserServiceCommand
{
    public class UsersCommand : BaseCommand
    {
        private readonly IUserRepository _userRepo;
        private readonly IAuthService _authService;

        public override string Name => "users";
        public override string Description => "Показать всех пользователей (только админ)";
        public override List<Role> AvailableFor => new List<Role> { Role.Admin };

        public UsersCommand(IUserRepository userRepo, IAuthService authService)
        {
            _userRepo = userRepo;
            _authService = authService;
        }
        public override void Execute(string[] args)
        {
            var user = _authService.CurrentUser;
            if (user == null)
            {
                Error("Сначала выполните вход");
                return;
            }

            var users = _userRepo.GetAll();
            Console.WriteLine($"\nВсего пользователей: {users.Count}");
            Console.WriteLine(new string('-', 100));
            Console.WriteLine($"{"Email",-30} {"Имя",-20} {"Роль",-12} {"Баланс",-10} {"Статус",-10}");
            Console.WriteLine(new string('-', 100));

            foreach (var u in users)
            {
                string status = u.IsBlocked ? "Заблокирован" : "Активен";
                Console.WriteLine($"{u.Email,-30} {u.Name,-20} {u.Role,-12} {u.Balance,-10} {status,-10}");
            }
            Console.WriteLine(new string('-', 100));
        }
    }
}