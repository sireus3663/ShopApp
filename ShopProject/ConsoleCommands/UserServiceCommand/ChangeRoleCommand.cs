using Microsoft.EntityFrameworkCore;
using ShopProject.Db;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopProject.Models;


namespace ShopProject.ConsoleCommands.UserServiceCommand
{
    public class ChangeRoleCommand : BaseCommand
    {
        private readonly UserService _userService;
        private readonly AuthService _authService;
        private readonly AppDbContext _context;

        public override string Name => "chanegrole";
        public override string Description => "Изменить роль. Использование: changerole <email> <Buyer | Seller | Moderator | Admin>";
        public ChangeRoleCommand(UserService userService, AuthService authService, AppDbContext context)
        {
            _userService = userService;
            _authService = authService;
            _context = context;
        }
        public override void Execute(string[] args)
        {
            if (args.Length <2) { Error("Укажите email и роль"); return; }
            if (_authService.currentUser == null) { Error("Вы не авторизованы"); return; }
            if (!Enum.TryParse<Role>(args[1], true, out var newRole)) { Error("Некорректная роль. Доступны: Buyer, Seller, Moderator, Admin"); return; }
            try
            {
                var userRepo = new UserRepository(_context);
                var user = userRepo.GetByEmail(args[0]);
                if (user == null) { Error($"Пользователь {args[0]} не найден"); return; }
                _userService.ChangeRole(user.Id, newRole);
                Success($"Роль пользователя {args[0]} изменена на {newRole}");
            }
            catch (Exception ex) { Error(ex.Message); }
        }
    }
}
