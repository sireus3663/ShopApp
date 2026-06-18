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
    public class ViewUserProfileCommand : BaseCommand
    {
        private readonly ModeratorService _moderatorService;
        private readonly AuthService _authService;

        public override string Name => "view-profile";
        public override string Description => "Просмотр профиля пользователя. Использование: view-profile <email>";
        public override List<Role> AvailableFor => new List<Role> { Role.Moderator, Role.Admin };

        public ViewUserProfileCommand(ModeratorService moderatorService, AuthService authService)
        {
            _moderatorService = moderatorService;
            _authService = authService;
        }

        public override void Execute(string[] args)
        {
            if (args.Length < 1) { Error("Укажите email пользователя"); return; }  
            if(_authService.currentUser == null) { Error("Сначала выполните вход"); return; }

            try
            {
                _moderatorService.ViewUserProfile(args[0]);
            }
            catch (Exception ex) { Error(ex.Message); }
        }
    }
}
