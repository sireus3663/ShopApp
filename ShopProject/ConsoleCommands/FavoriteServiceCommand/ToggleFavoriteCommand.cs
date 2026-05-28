using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (_authService.currentUser == null) { Error("Сначала выполните вход"); return; }
            if (!Guid.TryParse(args[0], out var productId)) { Error("Некорректный ID товара"); return; }
            try
            {
                _favoriteService.ToggleFavorite(productId);
                Success("Избранное обновлено");
            }
            catch (Exception ex) { Error(ex.Message); }
        }
    }
}
