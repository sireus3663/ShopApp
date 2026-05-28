using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.UserServiceCommand
{
    public class UsersCommand : BaseCommand
    {
        private readonly UserRepository _userRepo;
        private readonly AuthService _authService;

        public override string Name => "users";
        public override string Description => "Показать всех пользователей (только админ)";
        public override List<Role> AvailableFor => new List<Role> { Role.Admin };

        public UsersCommand(UserRepository userRepo, AuthService authService)
        {
            _userRepo = userRepo;
            _authService = authService;
        }
        public override void Execute(string[] args)
        {
            if (_authService.currentUser == null) { Error("Сначала выполните вход"); return; }
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
