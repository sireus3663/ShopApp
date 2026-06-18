using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;

using static ShopProject.Services.StatisticService;

using ShopProject.Db;
namespace ShopProject.Services
{
    public interface IStatisticService
    {
        List<Order> GetProductSales(Guid productId);
        ProductStatistic GetProductStatistic(Guid productId);
    }
}

namespace ShopProject.Services
{
    public class StatisticService : IStatisticService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IDiscountService _discountService;

        public StatisticService(IOrderRepository orderRepository, IProductRepository productRepository, IDiscountService discountService)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _discountService = discountService;
        }

        public List<Order> GetProductSales(Guid productId)
        {
            return _orderRepository.GetByProduct(productId);
        }

        public ProductStatistic GetProductStatistic(Guid productId)
        {
            var product = _productRepository.GetById(productId);
            if (product == null) throw new Exception("РџСЂРѕРґСѓРєС‚Р° РЅРµ СЃСѓС‰РµСЃС‚РІСѓРµС‚");
            var productSales = GetProductSales(productId);
            var discount = _discountService.GetByProduct(productId);
            bool HasDiscount;
            decimal FinalPrice;
            decimal DiscountPercent;
            if (discount == null)
            {
                HasDiscount = false;
                FinalPrice = product.Price;
                DiscountPercent = 0;
            }
            else
            {
                HasDiscount = true;
                FinalPrice = _discountService.CalculatePrice(product);
                DiscountPercent = discount.Percent;
            }
            ProductStatistic statistic = new ProductStatistic
            (
                product.Name,
                product.Price,
                FinalPrice,
                DiscountPercent,
                HasDiscount,
                productSales.Count,
                productSales.Count * product.Price
            );
            return statistic;
        }

        public record ProductStatistic
        (
            string ProductName,
            decimal Price,
            decimal FinalPrice,
            decimal DiscountPercent,
            bool HasDiscount,
            int SalesCount,
            decimal Revenue
        );
    }
}