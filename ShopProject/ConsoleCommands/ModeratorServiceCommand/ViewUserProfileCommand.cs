using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Models;
using ShopProject.Services.Interfaces;
using System;

namespace ShopProject.ConsoleCommands.ModeratorServiceCommand
{
    public class ViewUserProfileCommand : BaseCommand
    {
        private readonly IModeratorService _moderatorService;
        private readonly IAuthService _authService;

        public override string Name => "view-profile";
        public override string Description => "Просмотр профиля пользователя. Использование: view-profile <email>";
        public override List<Role> AvailableFor => new List<Role> { Role.Moderator, Role.Admin };

        public ViewUserProfileCommand(IModeratorService moderatorService, IAuthService authService)
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
                _moderatorService.ViewUserProfile(args[0].Trim());
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }
    }
}