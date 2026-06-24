using ShopProject.ConsoleCommands.BaseCommands;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.UserServiceCommand
{
    public class ProfileCommand : BaseCommand
    {
        private readonly UserService _userService;
        private readonly AuthService _authService;

        public override string Name => "user profile";
        public override string Description => "Показать профиль";

        public ProfileCommand(UserService userService, AuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        public override void Execute(string[] args)
        {
            if(_authService.currentUser == null) { Error("Сначала выполните вход"); return; }   
            try
            {
                _userService.ShowProfile();
            }
            catch (Exception ex) { Error(ex.Message); }
        }
    }
}
