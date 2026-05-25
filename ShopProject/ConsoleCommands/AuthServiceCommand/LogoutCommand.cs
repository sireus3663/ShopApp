using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.AuthServiceCommand
{
    public class LogoutCommand : BaseCommand
    {
        private readonly AuthService _authService;
        public override string Name => "logout";
        public override string Description => "Выход из системы";
        public LogoutCommand(AuthService authService) { _authService = authService; }
        public override void Execute(string[] args)
        {
            if (_authService.currentUser == null) { Error("Вы не авторизованы"); return; }
            var name = _authService.currentUser.Name;
            _authService.Logout();
            Success($"До свидания, {name}");
        }
        
    }
}
