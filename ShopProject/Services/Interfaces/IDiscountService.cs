using ShopProject.Models;
using System;

namespace ShopProject.Services.Interfaces
{
    public interface IDiscountService
    {
        Discount CreateDiscount(Product product, decimal percent);
        Discount? GetByProduct(Guid productId);
        decimal CalculatePrice(Product product);
        Discount RemoveDiscount(Guid productId);
    }
}