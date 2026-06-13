using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopProject.Forms.ViewModels
{
    public class CartViewModel
    {
        private readonly CartService _cartService;
        private readonly ProductRepository _productRepo;

        public CartViewModel(CartService cartService, ProductRepository productRepo)
        {
            _cartService = cartService;
            _productRepo = productRepo;
        }

        public List<Cart> GetCartItems(Guid userId)
        {
            return _cartService.GetCart(userId);
        }

        public decimal GetItemTotal(Product product, int count)
        {
            return product.Price * count;
        }

        public decimal GetCartTotal(List<Cart> cartItems)
        {
            decimal total = 0;
            foreach (var item in cartItems)
            {
                var product = _productRepo.GetById(item.ProductId);
                if (product != null)
                    total += product.Price * item.Count;
            }
            return total;
        }

        public void RemoveFromCart(Guid productId)
        {
            _cartService.RemoveFromCart(productId);
        }

        public void AddToCart(Guid productId)
        {
            _cartService.AddToCart(productId);
        }

        public Product GetProduct(Guid productId)
        {
            return _productRepo.GetById(productId);
        }
    }
}