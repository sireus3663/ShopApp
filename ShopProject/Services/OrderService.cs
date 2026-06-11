using ShopProject.Db;
using ShopProject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopProject.Services
{
    public class OrderService
    {
        private readonly AuthService _authService;
        private readonly CartService _cartService;
        private readonly OrderRepository _orderRepository;
        private readonly ProductRepository _productRepository;
        private readonly UserRepository _userRepository;
        private readonly DiscountService _discountService;
        private readonly AppDbContext _context;

        public OrderService(AuthService authService, CartService cartService, OrderRepository orderRepository, ProductRepository productRepository, UserRepository userRepository, DiscountService discountService, AppDbContext context)
        {
            _authService = authService;
            _cartService = cartService;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _discountService = discountService;
            _context = context;
        }

        // ========== СИНХРОННЫЕ МЕТОДЫ ==========

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
                var totalPrice = cart.Sum(product =>
                    _discountService.CalculatePrice(_productRepository.GetById(product.ProductId)) * product.Count
                );

                if (currentUser.Balance < totalPrice)
                    throw new Exception("У пользователя недостаточно денег");

                currentUser.Balance -= totalPrice;
                _userRepository.Update(currentUser);

                foreach (var productInCart in cart)
                {
                    var product = _productRepository.GetById(productInCart.ProductId);

                    if (product == null)
                        throw new Exception($"Товар не найден: {productInCart.ProductId}");

                    if (product.Amount < productInCart.Count)
                        throw new Exception($"Недостаточно товара: {product.Name}. Доступно: {product.Amount}");

                    product.Amount -= productInCart.Count;
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

        public List<Order> getUserOrders(Guid userId)
        {
            return _orderRepository.GetByUser(userId);
        }

        // ========== АСИНХРОННЫЕ МЕТОДЫ ==========

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

                    if (product.Amount < productInCart.Count)
                        throw new Exception($"Недостаточно товара: {product.Name}. Доступно: {product.Amount}");

                    product.Amount -= productInCart.Count;
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