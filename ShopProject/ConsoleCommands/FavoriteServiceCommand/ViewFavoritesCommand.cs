using ShopProject.ConsoleCommands.BaseCommands;
using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.FavoriteServiceCommand
{
    public class ViewFavoritesCommand : BaseCommand
    {
        private readonly FavoriteService _favoriteService;
        private readonly AuthService _authService;
        private readonly ProductRepository _productRepo;

        public override string Name => "favorites-view";
        public override string Description => "Показать избранные товары";
        public override List<Role> AvailableFor => new List<Role> { Role.Buyer };

        public ViewFavoritesCommand(FavoriteService favoriteService, AuthService authService, ProductRepository productRepo)
        {
            _favoriteService = favoriteService;
            _authService = authService;
            _productRepo = productRepo;
        }

        public override void Execute(string[] args)
        {
            if (_authService.currentUser == null) { Error("Сначала выполните вход"); return; }

            var favorites = _favoriteService.GetUserFavorites(_authService.currentUser.Id);

            if (favorites.Count == 0) { Info("Избранное пусто"); return; }

            Console.WriteLine($"\nИзбранные товары ({favorites.Count}):");
            Console.WriteLine(new string('-', 80));
            Console.WriteLine($"{"ID",-36} {"Название",-25} {"Цена",-10}");
            Console.WriteLine(new string('-', 80));

            foreach (var fav in favorites)
            {
                var product = _productRepo.GetById(fav.ProductId);
                if (product != null && product.IsApproved) { Console.WriteLine($"{product.Id,-36} {product.Name,-25} {product.Price,-10} руб."); }
            }
            Console.WriteLine(new string('-', 80));
        }
    }
}
