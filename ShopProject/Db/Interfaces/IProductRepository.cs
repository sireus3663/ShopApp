using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopProject.Db.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        List<Product> Search(string name);
        List<Product> GetSortedByPrice();
        List<Product> GetByCategory(string category);
        Task<List<Product>> SearchAsync(string name);
        Task<List<Product>> GetSortedByPriceAsync();
        Task<List<Product>> GetByCategoryAsync(string category);

        Task<PaginatedResult<Product>> GetApprovedProductsPaginatedAsync(
            string? searchText = null,
            string? category = null,
            decimal? priceFrom = null,
            decimal? priceTo = null,
            int page = 1,
            int pageSize = 12);
    }
}