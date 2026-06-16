using ShopProject.Db;
using ShopProject.Db.Interfaces;
using ShopProject.Models;
using ShopProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                throw new Exception("У пользователя недостаточно прав для покупки товара");

            var cart = _cartService.GetCurrentUserCart();
            if (!cart.Any())
                throw new Exception("Корзина пуста");

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
                    throw new Exception("У пользователя недостаточно денег");

                currentUser.Balance -= totalPrice;
                _userRepository.Update(currentUser);

                foreach (var productInCart in cart)
                {
                    var product = _productRepository.GetById(productInCart.ProductId);

                    if (product == null)
                        throw new Exception($"Товар не найден: {productInCart.ProductId}");

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
                Console.WriteLine($"[OK] Покупка оформлена на сумму {totalPrice} руб.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Ошибка при оформлении покупки: {ex.Message}");
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
                throw new Exception("У пользователя недостаточно прав для покупки товара");

            var cart = await _cartService.GetCurrentUserCartAsync();
            if (!cart.Any())
                throw new Exception("Корзина пуста");

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
                    throw new Exception("У пользователя недостаточно денег");

                currentUser.Balance -= totalPrice;
                await _userRepository.UpdateAsync(currentUser);

                foreach (var productInCart in cart)
                {
                    var product = await _productRepository.GetByIdAsync(productInCart.ProductId);

                    if (product == null)
                        throw new Exception($"Товар не найден: {productInCart.ProductId}");

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
                Console.WriteLine($"[OK] Покупка оформлена на сумму {totalPrice} руб.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Ошибка при оформлении покупки: {ex.Message}");
            }
        }

        public async Task<List<Order>> GetUserOrdersAsync(Guid userId)
        {
            return await _orderRepository.GetByUserAsync(userId);
        }
    }
}