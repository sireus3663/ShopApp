using System;
using ShopProject.Models;
using ShopProject.Services;

namespace ShopProject.Forms.ViewModels
{
    public class ProductCardViewModel
    {
        private readonly CartService _cartService;
        private readonly FavoriteService _favoriteService;

        public ProductCardViewModel(CartService cartService, FavoriteService favoriteService)
        {
            _cartService = cartService;
            _favoriteService = favoriteService;
        }

        public string GetFormattedPrice(decimal price)
        {
            return $"{price:N0} руб.";
        }

        public string GetShortName(string name, int maxLength = 30)
        {
            if (name.Length <= maxLength)
                return name;
            return name.Substring(0, maxLength - 3) + "...";
        }

        public void AddToCart(Guid productId)
        {
            _cartService.AddToCart(productId);
        }

        public void ToggleFavorite(Guid productId)
        {
            _favoriteService.ToggleFavorite(productId);
        }

        public bool IsFavorite(Guid productId, Guid userId)
        {
            var favorites = _favoriteService.GetUserFavorites(userId);
            return favorites.Exists(f => f.ProductId == productId);
        }
    }
}