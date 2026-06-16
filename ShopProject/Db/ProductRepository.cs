using Microsoft.EntityFrameworkCore;
using ShopProject.Models;
using ShopProject.Db.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopProject.Db
{
    public class ProductRepository : BaseRepository<Product>, IProductRepository
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

        public async Task<List<Product>> SearchAsync(string name)
        {
            return await _dbSet
                .Where(p => p.Name.ToLower().Contains(name.ToLower()))
                .ToListAsync();
        }

        public async Task<List<Product>> GetSortedByPriceAsync()
        {
            return await _dbSet.OrderBy(p => p.Price).ToListAsync();
        }

        public async Task<List<Product>> GetByCategoryAsync(string category)
        {
            return await _dbSet
                .Where(p => p.Category == category)
                .ToListAsync();
        }

        public async Task<PaginatedResult<Product>> GetApprovedProductsPaginatedAsync(
            string? searchText = null,
            string? category = null,
            decimal? priceFrom = null,
            decimal? priceTo = null,
            int page = 1,
            int pageSize = 12)
        {
            var query = _dbSet
                .Where(p => p.IsApproved)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchText))
                query = query.Where(p => p.Name.Contains(searchText));

            if (!string.IsNullOrEmpty(category) && category != "Все категории")
                query = query.Where(p => p.Category == category);

            if (priceFrom.HasValue)
                query = query.Where(p => p.Price >= priceFrom.Value);

            if (priceTo.HasValue)
                query = query.Where(p => p.Price <= priceTo.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Product>(items, totalCount, page, pageSize);
        }
    }
}