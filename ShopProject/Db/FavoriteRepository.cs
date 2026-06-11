using Microsoft.EntityFrameworkCore;
using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopProject.Db
{
    public class FavoriteRepository : BaseRepository<Favorite>
    {
        public FavoriteRepository(AppDbContext context) : base(context) { }

        public List<Favorite> GetByUser(Guid userId)
        {
            return _dbSet
                .Where(f => f.UserId == userId)
                .ToList();
        }

        public Favorite GetFavoriteItem(Guid userId, Guid productId)
        {
            return _dbSet
                .FirstOrDefault(f => f.UserId == userId && f.ProductId == productId);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public async Task<List<Favorite>> GetByUserAsync(Guid userId)
        {
            return await _dbSet
                .Where(f => f.UserId == userId)
                .ToListAsync();
        }

        public async Task<Favorite> GetFavoriteItemAsync(Guid userId, Guid productId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}