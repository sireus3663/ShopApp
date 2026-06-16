using ShopProject.Models;
using System;
using System.Collections.Generic;
using static ShopProject.Services.StatisticService;

namespace ShopProject.Services.Interfaces
{
    public interface IStatisticService
    {
        List<Order> GetProductSales(Guid productId);
        ProductStatistic GetProductStatistic(Guid productId);
    }
}