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
                var validItems = new List<(Product product, Cart cartItem)>();

                foreach (var cartItem in cart)
                {
                    var product = _productRepository.GetById(cartItem.ProductId);

                    if (product == null)
                        throw new Exception($"Товар не найден: {cartItem.ProductId}");

                    if (product.Amount < cartItem.Count)
                        throw new Exception($"Недостаточно товара: {product.Name}. Доступно: {product.Amount}, требуется: {cartItem.Count}");

                    validItems.Add((product, cartItem));
                }

                var totalPrice = validItems.Sum(item =>
                    _discountService.CalculatePrice(item.product) * item.cartItem.Count);

                if (currentUser.Balance < totalPrice)
                    throw new Exception($"Недостаточно средств. Нужно: {totalPrice} руб., доступно: {currentUser.Balance} руб.");

                currentUser.Balance -= totalPrice;
                _userRepository.Update(currentUser);

                foreach (var (product, cartItem) in validItems)
                {
                    product.Amount -= cartItem.Count;
                    _productRepository.Update(product);

                    Order newOrder = new Order
                    {
                        Id = Guid.NewGuid(),
                        UserId = currentUser.Id,
                        ProductId = cartItem.ProductId,
                        Price = _discountService.CalculatePrice(product) * cartItem.Count,
                        Count = cartItem.Count,
                        CreatedAt = DateTime.UtcNow
                    };

                    _orderRepository.Add(newOrder);
                    _cartService.DeleteCart(cartItem.Id);
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

        public void ReturnOrder(Guid orderId)
        {
            var currentUser = _authService.RequireUser();
            var order = _orderRepository.GetById(orderId);

            if (order == null)
                throw new Exception("Заказ не найден");

            if (order.UserId != currentUser.Id)
                throw new Exception("Вы можете вернуть только свои заказы");

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var product = _productRepository.GetById(order.ProductId);
                if (product != null)
                {
                    product.Amount += order.Count;
                    _productRepository.Update(product);
                }
                var user = _userRepository.GetById(order.UserId);
                user.Balance += order.Price;
                _userRepository.Update(user);

                _orderRepository.Delete(orderId);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Order> getUserOrders(Guid userId)
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
                var validItems = new List<(Product product, Cart cartItem)>();

                foreach (var cartItem in cart)
                {
                    var product = await _productRepository.GetByIdAsync(cartItem.ProductId);

                    if (product == null)
                        throw new Exception($"Товар не найден: {cartItem.ProductId}");

                    if (product.Amount < cartItem.Count)
                        throw new Exception($"Недостаточно товара: {product.Name}. Доступно: {product.Amount}, требуется: {cartItem.Count}");

                    validItems.Add((product, cartItem));
                }

                var totalPrice = 0m;
                foreach (var (product, cartItem) in validItems)
                {
                    totalPrice += _discountService.CalculatePrice(product) * cartItem.Count;
                }

                if (currentUser.Balance < totalPrice)
                    throw new Exception($"Недостаточно средств. Нужно: {totalPrice} руб., доступно: {currentUser.Balance} руб.");

                currentUser.Balance -= totalPrice;
                await _userRepository.UpdateAsync(currentUser);

                foreach (var (product, cartItem) in validItems)
                {
                    product.Amount -= cartItem.Count;
                    await _productRepository.UpdateAsync(product);

                    Order newOrder = new Order
                    {
                        Id = Guid.NewGuid(),
                        UserId = currentUser.Id,
                        ProductId = cartItem.ProductId,
                        Price = _discountService.CalculatePrice(product) * cartItem.Count,
                        Count = cartItem.Count,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _orderRepository.AddAsync(newOrder);
                    await _cartService.DeleteCartAsync(cartItem.Id);
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