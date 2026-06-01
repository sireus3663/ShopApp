using System;
using System.Collections.Generic;
using System.Linq;
using ShopProject.Db;
using ShopProject.Models;

namespace ShopProject.Services
{
    public class FavoriteService
    {
        private readonly FavoriteRepository _favoriteRepository;
        private readonly AuthService _authService;

        public FavoriteService(FavoriteRepository favoriteRepository, AuthService authService)
        {
            _favoriteRepository = favoriteRepository;
            _authService = authService;
        }

        public void ToggleFavorite(Guid productId)
        {
            var existingItem = _favoriteRepository.GetFavoriteItem(_authService.RequireUser().Id, productId);

            if (existingItem != null)
            {
                _favoriteRepository.Delete(existingItem.Id);
            }
            else
            {
                var newFavorite = new Favorite
                {
                    Id = Guid.NewGuid(),
                    UserId = _authService.RequireUser().Id,
                    ProductId = productId
                };

                _favoriteRepository.Add(newFavorite);
            }

            _favoriteRepository.Save();
        }

        public List<Guid> GetFavorites()
        {
            return _favoriteRepository.GetByUser(_authService.RequireUser().Id)
                .Select(f => f.ProductId)
                .ToList();
        }
        public List<Favorite> GetUserFavorites(Guid userId)
        {
            return _favoriteRepository.GetByUser(userId);
        }
    }
}