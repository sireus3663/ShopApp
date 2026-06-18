using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Models;
using System;

using ShopProject.Db;
using ShopProject.Services;
namespace ShopProject.ConsoleCommands.FavoriteServiceCommand
{
    public class ViewFavoritesCommand : BaseCommand
    {
        private readonly IFavoriteService _favoriteService;
        private readonly IAuthService _authService;
        private readonly IProductRepository _productRepo;

        public override string Name => "favorites-view";
        public override string Description => "Показать избранные товары";
        public override List<Role> AvailableFor => new List<Role> { Role.Buyer };

        public ViewFavoritesCommand(IFavoriteService favoriteService, IAuthService authService, IProductRepository productRepo)
        {
            _favoriteService = favoriteService;
            _authService = authService;
            _productRepo = productRepo;
        }

        public override void Execute(string[] args)
        {
            var user = _authService.CurrentUser;
            if (user == null)
            {
                Error("Сначала выполните вход");
                return;
            }

            var favorites = _favoriteService.GetUserFavorites(user.Id);

            if (favorites.Count == 0)
            {
                Info("Избранное пусто");
                return;
            }

            Console.WriteLine($"\nИзбранные товары ({favorites.Count}):");
            Console.WriteLine(new string('-', 80));
            Console.WriteLine($"{"ID",-36} {"Название",-25} {"Цена",-10}");
            Console.WriteLine(new string('-', 80));

            var hasItems = false;
            foreach (var fav in favorites)
            {
                var product = _productRepo.GetById(fav.ProductId);
                if (product != null && product.IsApproved)
                {
                    Console.WriteLine($"{product.Id,-36} {product.Name,-25} {product.Price,-10} руб.");
                    hasItems = true;
                }
            }

            if (!hasItems)
            {
                Console.WriteLine("Нет доступных товаров в избранном");
            }

            Console.WriteLine(new string('-', 80));
        }
    }
}