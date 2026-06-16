using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopProject.Services.Interfaces
{
    public interface IOrderService
    {
        void BuyCart();
        List<Order> GetUserOrders(Guid userId);

        Task BuyCartAsync();
        Task<List<Order>> GetUserOrdersAsync(Guid userId);
    }
}