using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopProject.Services.Interfaces
{
    public interface IFavoriteService
    {
        void ToggleFavorite(Guid productId);
        List<Guid> GetFavorites();
        List<Favorite> GetUserFavorites(Guid userId);

        Task ToggleFavoriteAsync(Guid productId);
        Task<List<Guid>> GetFavoritesAsync();
        Task<List<Favorite>> GetUserFavoritesAsync(Guid userId);
    }
}