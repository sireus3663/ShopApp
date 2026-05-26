using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopProject.Services;
using ShopProject.Db;

namespace ShopProject.ConsoleCommands.FavoriteServiceCommand
{
    public class ToggleFavoriteCommand : BaseCommand
    {
        private readonly FavoriteService _favoriteService;
        private readonly AuthService _authService;

        public override string Name => "favorite";
        public override string Description => "Добавить/Удалить из избранного. Использование: favorite <id товара>";

        public ToggleFavoriteCommand(FavoriteService favoriteService, AuthService authService)
        {
            _favoriteService = favoriteService;
            _authService = authService;
        }
        public override void Execute(string[] args)
        {
            if (args.Length < 1) { Error("Укажите ID товара"); return; }
            if(_authService.currentUser == null) { Error("Сначала выполните вход"); return; }
            if (!Guid.TryParse(args[0], out var productId)) { Error("Некоректный ID товара"); return; }
            try
            {
                _favoriteService.ToggleFavorite(_authService.currentUser.Id, productId);
                Success("Избранное обнавлено");
            }
            catch (Exception ex) { Error(ex.Message); }
        }
    }
}
