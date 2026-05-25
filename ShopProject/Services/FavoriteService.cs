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

        public FavoriteService(FavoriteRepository favoriteRepository)
        {
            _favoriteRepository = favoriteRepository;
        }
        public void ToggleFavorite(Guid userId, Guid productId)
        {
            var existingItem = _favoriteRepository.GetFavoriteItem(userId, productId);

            if (existingItem != null)
            {
                _favoriteRepository.Delete(existingItem.Id);
            }
            else
            {
                var newFavorite = new Favorite
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ProductId = productId
                };

                _favoriteRepository.Add(newFavorite);
            }

            _favoriteRepository.Save();
        }

        public List<Guid> GetFavorites(Guid userId)
        {
            return _favoriteRepository.GetByUser(userId)
                .Select(f => f.ProductId)
                .ToList();
        }
    }
}