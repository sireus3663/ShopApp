using ShopProject.Models;
using System;
using System.Threading.Tasks;

namespace ShopProject.Db.Interfaces
{
    public interface IDiscountRepository : IRepository<Discount>
    {
        Discount? GetByProduct(Guid productId);
        Task<Discount?> GetByProductAsync(Guid productId);
    }
}