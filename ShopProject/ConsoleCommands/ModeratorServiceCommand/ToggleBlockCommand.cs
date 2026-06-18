using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Models;
using System;

using ShopProject.Services;
namespace ShopProject.ConsoleCommands.ModeratorServiceCommand
{
    public class ToggleBlockCommand : BaseCommand
    {
        private readonly IModeratorService _moderatorService;
        private readonly IAuthService _authService;

        public override string Name => "block";
        public override string Description => "Блокировка/разблокировка пользователя. Использование: block <email>";
        public override List<Role> AvailableFor => new List<Role> { Role.Moderator, Role.Admin };

        public ToggleBlockCommand(IModeratorService moderatorService, IAuthService authService)
        {
            _moderatorService = moderatorService;
            _authService = authService;
        }

        public override void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                Error("Укажите email пользователя");
                return;
            }

            if (string.IsNullOrWhiteSpace(args[0]))
            {
                Error("Email не может быть пустым");
                return;
            }

            var user = _authService.CurrentUser;
            if (user == null)
            {
                Error("Сначала выполните вход");
                return;
            }

            try
            {
                _moderatorService.ToggleBlockUser(args[0].Trim());
                Success($"Статус пользователя {args[0]} изменён");
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }
    }
}