using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopProject.Services.Interfaces
{
    public interface ICartService
    {
        void AddToCart(Guid productId);
        void DeleteCart(Guid cartId);
        void RemoveFromCart(Guid productId);
        List<Cart> GetCart(Guid userId);
        List<Cart> GetCurrentUserCart();

        Task AddToCartAsync(Guid productId);
        Task DeleteCartAsync(Guid cartId);
        Task RemoveFromCartAsync(Guid productId);
        Task<List<Cart>> GetCartAsync(Guid userId);
        Task<List<Cart>> GetCurrentUserCartAsync();
    }
}