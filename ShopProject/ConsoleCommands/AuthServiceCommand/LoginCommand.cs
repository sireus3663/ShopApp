using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.AuthServiceCommand
{
    public class LoginCommand : BaseCommand
    {
        private readonly AuthService _authService;
        public override string Name => "login";
        public override string Description => "Вход. Использование: login <email> <пароль>";
        public LoginCommand(AuthService authService) { _authService = authService; }
        public override void Execute(string[] args)
        {
            if (args.Length < 2) { Error("Укажите email и пароль"); return; }
            try
            {
                _authService.Login(args[0], args[1]);
                Success($"Добро пожаловать, {_authService.currentUser?.Name}");
            }
            catch (Exception ex) { Error(ex.Message); }
        }
    }
}
