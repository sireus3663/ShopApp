using ShopProject.Db;
using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.Services
{
    public class OrderService
    {
        private readonly AuthService _authService;
        private readonly CartService _cartService;
        private readonly OrderRepository _orderRepository;
        public OrderService(AuthService authService, CartService cartService, OrderRepository orderRepository)
        {
            _authService = authService;
            _cartService = cartService;
            _orderRepository = orderRepository;
        }
        public Order BuyProduct(Product product)
        {
            var currentUser = _authService.RequireUser();
            if(currentUser.Balance < product.Price) { throw new Exception("У пользователя не хватает денег"); }
            if (!PermissionService.CanBuy(currentUser.Role)) { throw new Exception("У пользователя недостаточно прав для покупки товара"); }
            currentUser.Balance -= product.Price;
            Order newOrder = new Order
            {
                Id = Guid.NewGuid(),
                UserId = currentUser.Id,
                ProductId = product.Id,
                Price = product.Price,
                CreatedAt = DateTime.Now
            };
            _cartService.RemoveFromCart(currentUser.Id, product.Id);
            _orderRepository.Add(newOrder);
            return newOrder;
        }
        public List<Order> getUserOrders(Guid User)
        {
            return _orderRepository.GetByUser(User);
        }
    }
}
