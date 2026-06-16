using Microsoft.EntityFrameworkCore;
using ShopProject.Models;
using ShopProject.Db.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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