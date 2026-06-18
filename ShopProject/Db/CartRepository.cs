using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ShopProject.Db
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

namespace ShopProject.Db
{
    public class CartRepository : BaseRepository<Cart>, ICartRepository
    {
        public CartRepository(AppDbContext context) : base(context) { }

        public List<Cart> GetByUser(Guid userId)
        {
            return _dbSet
                .Where(c => c.UserId == userId)
                .ToList();
        }

        public Cart? GetCartItem(Guid userId, Guid productId)
        {
            return _dbSet
                .FirstOrDefault(c => c.UserId == userId && c.ProductId == productId);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public async Task<List<Cart>> GetByUserAsync(Guid userId)
        {
            return await _dbSet
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task<Cart?> GetCartItemAsync(Guid userId, Guid productId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<List<Cart>> GetUserCartWithProductsAsync(Guid userId)
        {
            return await _dbSet
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToListAsync();
        }
    }
}