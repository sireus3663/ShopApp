using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Models;
using System;
using System.Globalization;
using ShopProject.Services;
namespace ShopProject.ConsoleCommands.ModeratorServiceCommand
{
    public class ChangeBalanceCommand : BaseCommand
    {
        private readonly IModeratorService _moderatorService;
        private readonly IAuthService _authService;

        public override string Name => "set-balance";
        public override string Description => "Изменить баланс пользователя. Использование: set-balance <email> <сумма>";
        public override List<Role> AvailableFor => new List<Role> { Role.Moderator, Role.Admin };

        public ChangeBalanceCommand(IModeratorService moderatorService, IAuthService authService)
        {
            _moderatorService = moderatorService;
            _authService = authService;
        }

        public override void Execute(string[] args)
        {
            if (args.Length < 2)
            {
                Error("Укажите email и сумму");
                return;
            }

            if (string.IsNullOrWhiteSpace(args[0]))
            {
                Error("Email не может быть пустым");
                return;
            }

            if (string.IsNullOrWhiteSpace(args[1]))
            {
                Error("Сумма не может быть пустой");
                return;
            }

            var user = _authService.CurrentUser;
            if (user == null)
            {
                Error("Сначала выполните вход");
                return;
            }

            if (!decimal.TryParse(args[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var newBalance) || newBalance < 0)
            {
                Error("Сумма должна быть положительным числом");
                return;
            }

            try
            {
                _moderatorService.ChangeUserBalance(args[0].Trim(), newBalance);
                Success($"Баланс пользователя {args[0]} успешно изменён на {newBalance} руб.");
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }
    }
}