using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.Db
{
    public class ProductRepository : BaseRepository<Product>
    {
        public ProductRepository(AppDbContext context) : base(context) { }

        public List<Product> Search(string name)
        {
            return _dbSet
                .Where(p => p.Name.ToLower().Contains(name.ToLower()))
                .ToList();
        }

        public List<Product> GetSortedByPrice()
        {
            return _dbSet.OrderBy(p => p.Price).ToList();
        }

        public List<Product> GetByCategory(string category)
        {
            return _dbSet.Where(p => p.Category == category).ToList();
        }
    }
}