using Microsoft.EntityFrameworkCore;
using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.Db
{
    public class CartRepository : BaseRepository<Cart>
    {
        public CartRepository(AppDbContext context) : base(context) { }

        public Cart GetByUser(Guid userId)
        {
            return _dbSet
                .Include(c => c.ProductId)
                .FirstOrDefault(c => c.UserId == userId);
        }
    }
}
