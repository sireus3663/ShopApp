using ShopProject.ConsoleCommands.BasseCommands;
using System;

using ShopProject.Services;
namespace ShopProject.ConsoleCommands.AuthServiceCommand
{
    public class LogoutCommand : BaseCommand
    {
        private readonly IAuthService _authService;
        public override string Name => "logout";
        public override string Description => "Выход из системы";
        public LogoutCommand(IAuthService authService) { _authService = authService; }
        public override void Execute(string[] args)
        {
            var user = _authService.CurrentUser;
            if (user == null)
            {
                Error("Вы не авторизованы");
                return;
            }

            var name = user.Name ?? "пользователь";
            _authService.Logout();
            Success($"До свидания, {name}");
        }
    }
}