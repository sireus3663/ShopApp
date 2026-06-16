using Microsoft.EntityFrameworkCore;
using ShopProject.Models;
using ShopProject.Db.Interfaces;
using System;
using System.Threading.Tasks;

namespace ShopProject.Db
{
    public class DiscountRepository : BaseRepository<Discount>, IDiscountRepository
    {
        public DiscountRepository(AppDbContext context) : base(context) { }

        public Discount? GetByProduct(Guid productId)
        {
            return _dbSet.FirstOrDefault(d => d.ProductId == productId);
        }

        public async Task<Discount?> GetByProductAsync(Guid productId)
        {
            return await _dbSet.FirstOrDefaultAsync(d => d.ProductId == productId);
        }
    }
}