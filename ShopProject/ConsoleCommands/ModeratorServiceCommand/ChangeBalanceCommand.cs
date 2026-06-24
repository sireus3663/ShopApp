using ShopProject.ConsoleCommands.BaseCommands;
using ShopProject.Models;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.ModeratorServiceCommand
{
    public class ChangeBalanceCommand : BaseCommand
    {
        private readonly ModeratorService _moderatorService;
        private readonly AuthService _authService;

        public override string Name => "balance set";
        public override string Description => "Изменить баланс пользователя. Использование: balance set <email> <сумма>";
        public override List<Role> AvailableFor => new List<Role> { Role.Moderator, Role.Admin };

        public ChangeBalanceCommand(ModeratorService moderatorService, AuthService authService)
        {
            _moderatorService = moderatorService;
            _authService = authService;
        }

        public override void Execute(string[] args)
        {
            if (args.Length < 2) { Error("Укажите email и сумму"); return; }
            if (_authService.currentUser == null) { Error("Сначал выполните вход"); return; }
            if (!decimal.TryParse(args[1], out var newBalance)) { Error("Сумма должна быть числом"); return; }
            
            try
            {
                _moderatorService.ChangeUserBalance(args[0], newBalance);
                Success("Баланс успешно изменён");
            }
            catch (Exception ex) { Error(ex.Message); }
        }
    }
}
