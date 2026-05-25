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
        public override string Name => "changerole";
        public override string Description => "Изменить роль. Использование changerole <email> <Buyer | Seller | Moderator | Admin>";
        public ChangeRoleCommand(UserService userService, AuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }
        public override void Execute(string[] args)
        {
            if (args.Length <2) { Error("Укажите email и роль"); return; }
            if (_authService.currentUser.Role != Role.Admin) { Error("Только администратор может менять роли"); return; }
            if (Enum.TryParse<Role>(args[0], true, out Role newRole)) { Error("Некорректная роль. Доступны: Buyer, Seller, Moderator, Admin"); return; }
            try
            {
                var userRepo = new Db.UserRepository(new Db.AppDbContext());
                var user = userRepo.GetByEmail(args[0]);
                if (user == null) { Error($"Пользователь {args[0]} не найдена"); return; }
                _userService.ChangeRole(user.Id, newRole);
                Success($"Роль пользователя {args[0]} изменена на {newRole}");
            }
            catch (Exception ex) { Error(ex.Message); }
        }
    }
}
