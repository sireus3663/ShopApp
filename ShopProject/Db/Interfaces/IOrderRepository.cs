using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopProject.Db.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        List<Order> GetByUser(Guid userId);
        List<Order> GetByProduct(Guid productId);
        Task<List<Order>> GetByUserAsync(Guid userId);
        Task<List<Order>> GetByProductAsync(Guid productId);
    }
}