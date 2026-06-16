using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopProject.Db.Interfaces
{
    public interface ICartRepository : IRepository<Cart>
    {
        List<Cart> GetByUser(Guid userId);
        Cart? GetCartItem(Guid userId, Guid productId);
        void Save();
        Task<List<Cart>> GetByUserAsync(Guid userId);
        Task<Cart?> GetCartItemAsync(Guid userId, Guid productId);
        Task SaveAsync();
        Task<int> SaveChangesAsync();
        Task<List<Cart>> GetUserCartWithProductsAsync(Guid userId);
    }
}