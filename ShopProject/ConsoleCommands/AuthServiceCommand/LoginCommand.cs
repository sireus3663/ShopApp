using ShopProject.ConsoleCommands.BasseCommands;
using System;

using ShopProject.Services;
namespace ShopProject.ConsoleCommands.AuthServiceCommand
{
    public class LoginCommand : BaseCommand
    {
        private readonly IAuthService _authService;
        public override string Name => "login";
        public override bool AvailableForGuest => true;

        public override string Description => "Вход. Использование: login <email> <пароль>";
        public LoginCommand(IAuthService authService) { _authService = authService; }
        public override void Execute(string[] args)
        {
            if (args.Length < 2)
            {
                Error("Укажите email и пароль");
                return;
            }

            if (string.IsNullOrWhiteSpace(args[0]))
            {
                Error("Email не может быть пустым");
                return;
            }

            if (string.IsNullOrWhiteSpace(args[1]))
            {
                Error("Пароль не может быть пустым");
                return;
            }

            try
            {
                _authService.Login(args[0], args[1]);
                var user = _authService.CurrentUser;
                Success($"Добро пожаловать, {user?.Name ?? "пользователь"}");
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }
    }
}