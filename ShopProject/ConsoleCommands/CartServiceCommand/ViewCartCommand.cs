using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopProject.Services;
using ShopProject.Db;

namespace ShopProject.ConsoleCommands.CartServiceCommand
{
    public class ViewCartCommand : BaseCommand
    {
        private readonly CartService _cartService;
        private readonly AuthService _authService;
        private readonly ProductRepository _productRepo;
        public override string Name => "cart-view";
        public override string Description => "Показать корзину";
        public ViewCartCommand(CartService cartService, AuthService authService, ProductRepository productRepo)
        {
            _cartService = cartService;
            _authService = authService;
            _productRepo = productRepo;
        }
        public override void Execute(string[] args)
        {
            if (_authService.currentUser == null) { Error("Сначала выполните вход"); return; }
            var cartItems = _cartService.GetCart(_authService.currentUser.Id);
            if(cartItems.Count ==0) { Info("Корзина пуста"); return; }
            Info($"В корзине {cartItems.Count} товаров");
            foreach (var productId in cartItems)
            {
                var product = _productRepo.GetById(productId);
                if(product != null) { Console.WriteLine($"  - {product.Name} | {product.Price} руб."); }
            }
        }
    }
}
