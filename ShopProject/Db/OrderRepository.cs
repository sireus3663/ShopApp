using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.Db
{
    public class OrderRepository : BaseRepository<Order>
    {
        public OrderRepository(AppDbContext context) : base(context) { }

        public List<Order> GetByUser(Guid userId)
        {
            return _dbSet.Where(o => o.UserId == userId).ToList();
        }
        public List<Order> GetByProduct(Guid productId)
        {
            return _dbSet.Where(o=>o.ProductId == productId).ToList();
        }
    }
}
