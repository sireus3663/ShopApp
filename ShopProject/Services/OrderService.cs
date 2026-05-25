using ShopProject.Db;
using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
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
        public OrderService(AuthService authService, CartService cartService, OrderRepository orderRepository, ProductRepository productRepository, UserRepository userRepository, DiscountService discountService)
        {
            _authService = authService;
            _cartService = cartService;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _discountService = discountService;
        }
        public void BuyCart() 
        {
            var currentUser = _authService.RequireUser();
            if (!PermissionService.CanBuy(currentUser.Role)) { throw new Exception("У пользователя недостаточно прав для покупки товара"); }
            var cart = _cartService.GetCurrentUserCart();
            if (!cart.Any())
            {
                throw new Exception("Корзина пуста");
            }
            var totalPrice = cart.Sum(
                product =>
                    _discountService.CalculatePrice(_productRepository.GetById(product.ProductId)) * product.Count
            );
            if (currentUser.Balance < totalPrice) { throw new Exception("у пользователя недостаточно денег"); }
            currentUser.Balance -= totalPrice;
            foreach ( var productInCart in cart )
            {
                Order newOrder = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUser.Id,
                    ProductId = productInCart.ProductId,
                    Price = _discountService.CalculatePrice(_productRepository.GetById(productInCart.ProductId)) * productInCart.Count,
                    Count = productInCart.Count,
                    CreatedAt = DateTime.UtcNow
                };
                _orderRepository.Add(newOrder);
                _cartService.DeleteCart(productInCart.Id);
            }
            _userRepository.Update(currentUser);
        }
        public List<Order> getUserOrders(Guid User)
        {
            return _orderRepository.GetByUser(User);
        }
    }
}
