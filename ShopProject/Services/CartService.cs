using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShopProject.Db;
using ShopProject.Models;

namespace ShopProject.Services
{
    public class CartService
    {
        private readonly CartRepository _cartRepository;
        private readonly AuthService _authService;

        public CartService(CartRepository cartRepository, AuthService authService)
        {
            _cartRepository = cartRepository;
            _authService = authService;
        }

        public void AddToCart(Guid productId)
        {
            if (!PermissionService.CanBuy(_authService.RequireUser().Role))
                throw new Exception("У вашей роли нет прав для добавления товаров в корзину.");

            var existingItem = _cartRepository.GetCartItem(_authService.RequireUser().Id, productId);

            if (existingItem != null)
            {
                existingItem.Count++;
                _cartRepository.Update(existingItem);
            }
            else
            {
                var newCartItem = new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = _authService.RequireUser().Id,
                    ProductId = productId,
                    Count = 1,
                };
                _cartRepository.Add(newCartItem);
            }
            _cartRepository.Save();
        }

        public void DeleteCart(Guid cartId)
        {
            if (_cartRepository.GetById(cartId) == null)
                throw new Exception("не удалось найти корзину");
            _cartRepository.Delete(cartId);
        }

        public void RemoveFromCart(Guid productId)
        {
            var existingItem = _cartRepository.GetCartItem(_authService.RequireUser().Id, productId);
            if (existingItem == null)
                throw new Exception("такого товара нет в тележке");

            if (existingItem.Count > 1)
            {
                existingItem.Count--;
                _cartRepository.Update(existingItem);
            }
            else
            {
                _cartRepository.Delete(existingItem.Id);
            }
            _cartRepository.Save();
        }

        public List<Cart> GetCart(Guid userId)
        {
            if (!PermissionService.CanAdministrate(_authService.RequireUser().Role))
                throw new Exception("У вашей роли нет прав для просмотра чужих корзин");
            return _cartRepository.GetByUser(userId);
        }

        public List<Cart> GetCurrentUserCart()
        {
            return _cartRepository.GetByUser(_authService.RequireUser().Id);
        }

        public async Task AddToCartAsync(Guid productId)
        {
            if (!PermissionService.CanBuy(_authService.RequireUser().Role))
                throw new Exception("У вашей роли нет прав для добавления товаров в корзину.");

            var existingItem = await _cartRepository.GetCartItemAsync(_authService.RequireUser().Id, productId);

            if (existingItem != null)
            {
                existingItem.Count++;
                await _cartRepository.UpdateAsync(existingItem);
            }
            else
            {
                var newCartItem = new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = _authService.RequireUser().Id,
                    ProductId = productId,
                    Count = 1,
                };
                await _cartRepository.AddAsync(newCartItem);
            }
            await _cartRepository.SaveChangesAsync();
        }

        public async Task DeleteCartAsync(Guid cartId)
        {
            if (await _cartRepository.GetByIdAsync(cartId) == null)
                throw new Exception("не удалось найти корзину");
            await _cartRepository.DeleteAsync(cartId);
        }

        public async Task RemoveFromCartAsync(Guid productId)
        {
            var existingItem = await _cartRepository.GetCartItemAsync(_authService.RequireUser().Id, productId);
            if (existingItem == null)
                throw new Exception("такого товара нет в тележке");

            if (existingItem.Count > 1)
            {
                existingItem.Count--;
                await _cartRepository.UpdateAsync(existingItem);
            }
            else
            {
                await _cartRepository.DeleteAsync(existingItem.Id);
            }
            await _cartRepository.SaveChangesAsync();
        }

        public async Task<List<Cart>> GetCartAsync(Guid userId)
        {
            if (!PermissionService.CanAdministrate(_authService.RequireUser().Role))
                throw new Exception("У вашей роли нет прав для просмотра чужих корзин");
            return await _cartRepository.GetByUserAsync(userId);
        }

        public async Task<List<Cart>> GetCurrentUserCartAsync()
        {
            return await _cartRepository.GetByUserAsync(_authService.RequireUser().Id);
        }
    }
}