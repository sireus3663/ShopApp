using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}