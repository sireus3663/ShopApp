using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ShopProject.Db
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

namespace ShopProject.Db
{
    public class FavoriteRepository : BaseRepository<Favorite>, IFavoriteRepository
    {
        public FavoriteRepository(AppDbContext context) : base(context) { }

        public List<Favorite> GetByUser(Guid userId)
        {
            return _dbSet
                .Where(f => f.UserId == userId)
                .ToList();
        }

        public Favorite? GetFavoriteItem(Guid userId, Guid productId)
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

        public async Task<Favorite?> GetFavoriteItemAsync(Guid userId, Guid productId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}