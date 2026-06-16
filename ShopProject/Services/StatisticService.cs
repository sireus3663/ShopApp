using ShopProject.Db.Interfaces;
using ShopProject.Models;
using ShopProject.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

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
            if (product == null) throw new Exception("Продукта не существует");
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