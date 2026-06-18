using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShopProject.Db;
using System.Linq;

namespace ShopProject.Services
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

namespace ShopProject.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IAuthService _authService;

        public CartService(ICartRepository cartRepository, IAuthService authService)
        {
            _cartRepository = cartRepository;
            _authService = authService;
        }

        public void AddToCart(Guid productId)
        {
            if (!PermissionService.CanBuy(_authService.RequireUser().Role))
                throw new Exception("РЈ РІР°С€РµР№ СЂРѕР»Рё РЅРµС‚ РїСЂР°РІ РґР»СЏ РґРѕР±Р°РІР»РµРЅРёСЏ С‚РѕРІР°СЂРѕРІ РІ РєРѕСЂР·РёРЅСѓ.");

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
                throw new Exception("РЅРµ СѓРґР°Р»РѕСЃСЊ РЅР°Р№С‚Рё РєРѕСЂР·РёРЅСѓ");
            _cartRepository.Delete(cartId);
        }

        public void RemoveFromCart(Guid productId)
        {
            var existingItem = _cartRepository.GetCartItem(_authService.RequireUser().Id, productId);
            if (existingItem == null)
                throw new Exception("С‚Р°РєРѕРіРѕ С‚РѕРІР°СЂР° РЅРµС‚ РІ С‚РµР»РµР¶РєРµ");

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
                throw new Exception("РЈ РІР°С€РµР№ СЂРѕР»Рё РЅРµС‚ РїСЂР°РІ РґР»СЏ РїСЂРѕСЃРјРѕС‚СЂР° С‡СѓР¶РёС… РєРѕСЂР·РёРЅ");
            return _cartRepository.GetByUser(userId);
        }

        public List<Cart> GetCurrentUserCart()
        {
            return _cartRepository.GetByUser(_authService.RequireUser().Id);
        }

        public async Task AddToCartAsync(Guid productId)
        {
            if (!PermissionService.CanBuy(_authService.RequireUser().Role))
                throw new Exception("РЈ РІР°С€РµР№ СЂРѕР»Рё РЅРµС‚ РїСЂР°РІ РґР»СЏ РґРѕР±Р°РІР»РµРЅРёСЏ С‚РѕРІР°СЂРѕРІ РІ РєРѕСЂР·РёРЅСѓ.");

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
                throw new Exception("РЅРµ СѓРґР°Р»РѕСЃСЊ РЅР°Р№С‚Рё РєРѕСЂР·РёРЅСѓ");
            await _cartRepository.DeleteAsync(cartId);
        }

        public async Task RemoveFromCartAsync(Guid productId)
        {
            var existingItem = await _cartRepository.GetCartItemAsync(_authService.RequireUser().Id, productId);
            if (existingItem == null)
                throw new Exception("С‚Р°РєРѕРіРѕ С‚РѕРІР°СЂР° РЅРµС‚ РІ С‚РµР»РµР¶РєРµ");

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
                throw new Exception("РЈ РІР°С€РµР№ СЂРѕР»Рё РЅРµС‚ РїСЂР°РІ РґР»СЏ РїСЂРѕСЃРјРѕС‚СЂР° С‡СѓР¶РёС… РєРѕСЂР·РёРЅ");
            return await _cartRepository.GetByUserAsync(userId);
        }

        public async Task<List<Cart>> GetCurrentUserCartAsync()
        {
            return await _cartRepository.GetByUserAsync(_authService.RequireUser().Id);
        }
    }
}