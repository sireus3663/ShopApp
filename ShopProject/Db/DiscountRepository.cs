using ShopProject.Models;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ShopProject.Db
{
    public interface IDiscountRepository : IRepository<Discount>
    {
        Discount? GetByProduct(Guid productId);
        Task<Discount?> GetByProductAsync(Guid productId);
    }
}

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