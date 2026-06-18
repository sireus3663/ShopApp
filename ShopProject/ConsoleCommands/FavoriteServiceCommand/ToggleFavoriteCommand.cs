using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Models;
using System;

using ShopProject.Services;
namespace ShopProject.ConsoleCommands.FavoriteServiceCommand
{
    public class ToggleFavoriteCommand : BaseCommand
    {
        private readonly IFavoriteService _favoriteService;
        private readonly IAuthService _authService;

        public override string Name => "favorite";
        public override string Description => "Добавить/Удалить из избранного. Использование: favorite <id товара>";
        public override List<Role> AvailableFor => new List<Role> { Role.Buyer };

        public ToggleFavoriteCommand(IFavoriteService favoriteService, IAuthService authService)
        {
            _favoriteService = favoriteService;
            _authService = authService;
        }
        public override void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                Error("Укажите ID товара");
                return;
            }

            if (string.IsNullOrWhiteSpace(args[0]))
            {
                Error("ID товара не может быть пустым");
                return;
            }

            var user = _authService.CurrentUser;
            if (user == null)
            {
                Error("Сначала выполните вход");
                return;
            }

            if (!Guid.TryParse(args[0], out var productId))
            {
                Error("Некорректный ID товара");
                return;
            }

            try
            {
                _favoriteService.ToggleFavorite(productId);
                Success("Избранное обновлено");
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }
    }
}