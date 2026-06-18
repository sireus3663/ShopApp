using ShopProject.ConsoleCommands.BaseCommands;
using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.CartServiceCommand
{
    public class ViewCartCommand : BaseCommand
    {
        private readonly CartService _cartService;
        private readonly AuthService _authService;
        private readonly ProductRepository _productRepo;

        public override string Name => "cart-view";
        public override string Description => "Показать корзину";
        public override List<Role> AvailableFor => new List<Role> { Role.Buyer, Role.Seller };

        public ViewCartCommand(CartService cartService, AuthService authService, ProductRepository productRepo)
        {
            _cartService = cartService;
            _authService = authService;
            _productRepo = productRepo;
        }
        public override void Execute(string[] args)
        {
            if (_authService.currentUser == null) { Error("Сначала выполните вход"); return; }
            var cartItems = _cartService.GetCurrentUserCart();
            if (cartItems.Count == 0) { Info("Корзина пуста"); return; }
            Console.WriteLine($"\nКорзина пользователя: {_authService.currentUser.Name}");
            Console.WriteLine(new string('=', 95));
            Console.WriteLine($"{"ID товара",-38} {"Название",-25} {"Цена",-10} {"Кол-во",-6}");
            Console.WriteLine(new string('-', 95));
            foreach (var item in cartItems )
            {
                var product = _productRepo.GetById(item.ProductId);
                if (product != null) { Console.WriteLine($"{item.ProductId,-38} {product.Name,-25} {product.Price,-10} руб. {item.Count,-6} шт."); }
            }
        }
    }
}
