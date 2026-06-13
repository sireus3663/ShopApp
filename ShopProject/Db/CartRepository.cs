using Microsoft.EntityFrameworkCore;
using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopProject.Db
{
    public class CartRepository : BaseRepository<Cart>
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
    }
}