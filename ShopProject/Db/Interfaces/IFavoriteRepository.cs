using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopProject.Db.Interfaces
{
    public interface IFavoriteRepository : IRepository<Favorite>
    {
        List<Favorite> GetByUser(Guid userId);
        Favorite? GetFavoriteItem(Guid userId, Guid productId);
        void Save();
        Task<List<Favorite>> GetByUserAsync(Guid userId);
        Task<Favorite?> GetFavoriteItemAsync(Guid userId, Guid productId);
        Task SaveAsync();
        Task<int> SaveChangesAsync();
    }
}