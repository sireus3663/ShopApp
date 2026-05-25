using System;
using System.Collections.Generic;
using System.Linq;
using ShopProject.Db;
using ShopProject.Models;

namespace ShopProject.Services
{
    public class CartService
    {
        private readonly CartRepository _cartRepository;

        public CartService(CartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public void AddToCart(Guid userId, Guid productId, int price, Role currentRole)
        {

            if (!PermissionService.CanBuy(currentRole))
            {
                throw new Exception("У вашей роли нет прав для добавления товаров в корзину.");
            }

            var existingItem = _cartRepository.GetCartItem(userId, productId);

            if (existingItem != null)
            {
                existingItem.Count++;
            }
            else
            {
                var newCartItem = new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ProductId = productId,
                    Count = 1,
                    PriceAtMoment = price
                };

                _cartRepository.Add(newCartItem);
            }

            _cartRepository.Save();
        }

        public void RemoveFromCart(Guid userId, Guid productId)
        {
            var existingItem = _cartRepository.GetCartItem(userId, productId);

            if (existingItem != null)
            {
                if (existingItem.Count > 1)
                {
                    existingItem.Count--;
                }
                else
                {
                    _cartRepository.Delete(existingItem.Id);
                }

                _cartRepository.Save();
            }
        }

        public List<Guid> GetCart(Guid userId)
        {
            return _cartRepository.GetByUser(userId)
                .Select(c => c.ProductId)
                .ToList();
        }
    }
}