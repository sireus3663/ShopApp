using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ShopProject.Db
{
    public interface IOrderRepository : IRepository<Order>
    {
        List<Order> GetByUser(Guid userId);
        List<Order> GetByProduct(Guid productId);
        Task<List<Order>> GetByUserAsync(Guid userId);
        Task<List<Order>> GetByProductAsync(Guid productId);
    }
}

namespace ShopProject.Db
{
    public class OrderRepository : BaseRepository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context) { }

        public List<Order> GetByUser(Guid userId)
        {
            return _dbSet.Where(o => o.UserId == userId).ToList();
        }

        public List<Order> GetByProduct(Guid productId)
        {
            return _dbSet.Where(o => o.ProductId == productId).ToList();
        }

        public async Task<List<Order>> GetByUserAsync(Guid userId)
        {
            return await _dbSet
                .Where(o => o.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<Order>> GetByProductAsync(Guid productId)
        {
            return await _dbSet
                .Where(o => o.ProductId == productId)
                .ToListAsync();
        }
    }
}