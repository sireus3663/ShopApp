using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Db;
using ShopProject.Db.Interfaces;
using ShopProject.Models;
using ShopProject.Services.Interfaces;
using System;

namespace ShopProject.ConsoleCommands.UserServiceCommand
{
    public class ChangeRoleCommand : BaseCommand
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly AppDbContext _context;

        public override string Name => "changerole";
        public override string Description => "Изменить роль. Использование: changerole <email> <Buyer | Seller | Moderator | Admin>";
        public override List<Role> AvailableFor => new List<Role> { Role.Admin };

        public ChangeRoleCommand(IUserService userService, IAuthService authService, AppDbContext context)
        {
            _userService = userService;
            _authService = authService;
            _context = context;
        }
        public override void Execute(string[] args)
        {
            if (args.Length < 2)
            {
                Error("Укажите email и роль");
                return;
            }

            if (string.IsNullOrWhiteSpace(args[0]))
            {
                Error("Email не может быть пустым");
                return;
            }

            if (string.IsNullOrWhiteSpace(args[1]))
            {
                Error("Роль не может быть пустой");
                return;
            }

            var user = _authService.CurrentUser;
            if (user == null)
            {
                Error("Вы не авторизованы");
                return;
            }

            if (!Enum.TryParse<Role>(args[1], true, out var newRole))
            {
                Error("Некорректная роль. Доступны: Buyer, Seller, Moderator, Admin");
                return;
            }

            try
            {
                var userRepo = new UserRepository(_context);
                var targetUser = userRepo.GetByEmail(args[0].Trim());
                if (targetUser == null)
                {
                    Error($"Пользователь {args[0]} не найден");
                    return;
                }

                _userService.ChangeRole(targetUser.Id, newRole);
                Success($"Роль пользователя {args[0]} изменена на {newRole}");
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }
    }
}