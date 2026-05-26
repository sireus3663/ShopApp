using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.ModeratorServiceCommand
{
    public class ToggleBlockCommand : BaseCommand
    {
        private readonly ModeratorService _moderatorService;
        private readonly AuthService _authService;

        public override string Name => "block";
        public override string Description => "Блокировка/разблокировка пользователя. Использование: block <email>";

        public ToggleBlockCommand(ModeratorService moderatorService, AuthService authService)
        {
            _moderatorService = moderatorService;
            _authService = authService;
        }

        public override void Execute(string[] args)
        {
            if (args.Length < 1) { Error("Укажите email пользователя"); return; }
            if (_authService.currentUser == null) { Error("Сначала выполните вход"); return; }

            try
            {
                _moderatorService.ToggleBlockUser(args[0]);
                Success("Статус пользователя изменён");
            }
            catch (Exception ex) { Error(ex.Message); }
        }
    }
}
