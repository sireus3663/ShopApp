using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShopProject.Db;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ShopProject.Services
{
    public interface IOrderService
    {
        void BuyCart();
        List<Order> GetUserOrders(Guid userId);

        Task BuyCartAsync();
        Task<List<Order>> GetUserOrdersAsync(Guid userId);
    }
}

namespace ShopProject.Services
{
    public class OrderService : IOrderService
    {
        private readonly IAuthService _authService;
        private readonly ICartService _cartService;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDiscountService _discountService;
        private readonly AppDbContext _context;

        public OrderService(
            IAuthService authService,
            ICartService cartService,
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IUserRepository userRepository,
            IDiscountService discountService,
            AppDbContext context)
        {
            _authService = authService;
            _cartService = cartService;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _discountService = discountService;
            _context = context;
        }

        public void BuyCart()
        {
            var currentUser = _authService.RequireUser();

            if (!PermissionService.CanBuy(currentUser.Role))
                throw new Exception("РЈ РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ РЅРµРґРѕСЃС‚Р°С‚РѕС‡РЅРѕ РїСЂР°РІ РґР»СЏ РїРѕРєСѓРїРєРё С‚РѕРІР°СЂР°");

            var cart = _cartService.GetCurrentUserCart();
            if (!cart.Any())
                throw new Exception("РљРѕСЂР·РёРЅР° РїСѓСЃС‚Р°");

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var totalPrice = 0m;
                foreach (var productInCart in cart)
                {
                    var product = _productRepository.GetById(productInCart.ProductId);
                    if (product != null)
                    {
                        totalPrice += _discountService.CalculatePrice(product) * productInCart.Count;
                    }
                }

                if (currentUser.Balance < totalPrice)
                    throw new Exception("РЈ РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ РЅРµРґРѕСЃС‚Р°С‚РѕС‡РЅРѕ РґРµРЅРµРі");

                currentUser.Balance -= totalPrice;
                _userRepository.Update(currentUser);

                foreach (var productInCart in cart)
                {
                    var product = _productRepository.GetById(productInCart.ProductId);

                    if (product == null)
                        throw new Exception($"РўРѕРІР°СЂ РЅРµ РЅР°Р№РґРµРЅ: {productInCart.ProductId}");

                    _productRepository.Update(product);

                    Order newOrder = new Order
                    {
                        Id = Guid.NewGuid(),
                        UserId = currentUser.Id,
                        ProductId = productInCart.ProductId,
                        Price = _discountService.CalculatePrice(product) * productInCart.Count,
                        Count = productInCart.Count,
                        CreatedAt = DateTime.UtcNow
                    };

                    _orderRepository.Add(newOrder);
                    _cartService.DeleteCart(productInCart.Id);
                }

                transaction.Commit();
                Console.WriteLine($"[OK] РџРѕРєСѓРїРєР° РѕС„РѕСЂРјР»РµРЅР° РЅР° СЃСѓРјРјСѓ {totalPrice} СЂСѓР±.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"РћС€РёР±РєР° РїСЂРё РѕС„РѕСЂРјР»РµРЅРёРё РїРѕРєСѓРїРєРё: {ex.Message}");
            }
        }

        public List<Order> GetUserOrders(Guid userId)
        {
            return _orderRepository.GetByUser(userId);
        }

        public async Task BuyCartAsync()
        {
            var currentUser = _authService.RequireUser();

            if (!PermissionService.CanBuy(currentUser.Role))
                throw new Exception("РЈ РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ РЅРµРґРѕСЃС‚Р°С‚РѕС‡РЅРѕ РїСЂР°РІ РґР»СЏ РїРѕРєСѓРїРєРё С‚РѕРІР°СЂР°");

            var cart = await _cartService.GetCurrentUserCartAsync();
            if (!cart.Any())
                throw new Exception("РљРѕСЂР·РёРЅР° РїСѓСЃС‚Р°");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var totalPrice = 0m;
                foreach (var productInCart in cart)
                {
                    var product = await _productRepository.GetByIdAsync(productInCart.ProductId);
                    if (product != null)
                    {
                        totalPrice += _discountService.CalculatePrice(product) * productInCart.Count;
                    }
                }

                if (currentUser.Balance < totalPrice)
                    throw new Exception("РЈ РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ РЅРµРґРѕСЃС‚Р°С‚РѕС‡РЅРѕ РґРµРЅРµРі");

                currentUser.Balance -= totalPrice;
                await _userRepository.UpdateAsync(currentUser);

                foreach (var productInCart in cart)
                {
                    var product = await _productRepository.GetByIdAsync(productInCart.ProductId);

                    if (product == null)
                        throw new Exception($"РўРѕРІР°СЂ РЅРµ РЅР°Р№РґРµРЅ: {productInCart.ProductId}");

                    await _productRepository.UpdateAsync(product);

                    Order newOrder = new Order
                    {
                        Id = Guid.NewGuid(),
                        UserId = currentUser.Id,
                        ProductId = productInCart.ProductId,
                        Price = _discountService.CalculatePrice(product) * productInCart.Count,
                        Count = productInCart.Count,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _orderRepository.AddAsync(newOrder);
                    await _cartService.DeleteCartAsync(productInCart.Id);
                }

                await transaction.CommitAsync();
                Console.WriteLine($"[OK] РџРѕРєСѓРїРєР° РѕС„РѕСЂРјР»РµРЅР° РЅР° СЃСѓРјРјСѓ {totalPrice} СЂСѓР±.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"РћС€РёР±РєР° РїСЂРё РѕС„РѕСЂРјР»РµРЅРёРё РїРѕРєСѓРїРєРё: {ex.Message}");
            }
        }

        public async Task<List<Order>> GetUserOrdersAsync(Guid userId)
        {
            return await _orderRepository.GetByUserAsync(userId);
        }
    }
}