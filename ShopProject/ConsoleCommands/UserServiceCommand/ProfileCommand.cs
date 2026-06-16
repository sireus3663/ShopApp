using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Services.Interfaces;
using System;

namespace ShopProject.ConsoleCommands.UserServiceCommand
{
    public class ProfileCommand : BaseCommand
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public override string Name => "profile";
        public override string Description => "Показать профиль";

        public ProfileCommand(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        public override void Execute(string[] args)
        {
            var user = _authService.CurrentUser;
            if (user == null)
            {
                Error("Сначала выполните вход");
                return;
            }

            try
            {
                _userService.ShowProfile();
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }
    }
}