using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShopProject.Models;
using System.Threading.Tasks;

namespace ShopProject.Services.Interfaces
{
    public interface IProductService
    {
        List<Product> GetAllApproved();
        List<Product> GetForModerate();
        Product CreateProduct(string name, string description, decimal price, string category, byte[]? productImage = null, int amount = 0);
        Product Approve(Guid productId);
        void Decline(Guid productId);
        void Delete(Guid productId);

        Task<List<Product>> GetAllApprovedAsync();
        Task<List<Product>> GetForModerateAsync();
        Task<Product> CreateProductAsync(string name, string description, decimal price, string category, byte[]? productImage = null, int amount = 0);
        Task<Product> ApproveAsync(Guid productId);
        Task DeclineAsync(Guid productId);

        Task<PaginatedResult<Product>> GetApprovedProductsPaginatedAsync(
            string? searchText = null,
            string? category = null,
            decimal? priceFrom = null,
            decimal? priceTo = null,
            int page = 1,
            int pageSize = 12);
    }
}