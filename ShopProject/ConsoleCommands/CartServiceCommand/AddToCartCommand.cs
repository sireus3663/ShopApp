using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopProject.Models;
using ShopProject.Services;
using ShopProject.Db;

namespace ShopProject.ConsoleCommands.CartServiceCommand
{
    public class AddToCartCommand : BaseCommand
    {
        private readonly CartService _cartService;
        private readonly AuthService _authService;
        private readonly ProductRepository _productRepo;
        public override string Name => "cart-add";
        public override string Description => "Добавить в корзину. Использование: cart-add <id товара>";
        public AddToCartCommand(CartService cartService,  AuthService authService, ProductRepository productRepo)
        {
            _cartService = cartService;
            _authService = authService;
            _productRepo = productRepo;
        }
        public override void Execute(string[] args)
        {
            if (args.Length < 1) { Error("Укажите ID товара"); return; }
            if (_authService.currentUser == null) { Error("Сначала выполните вход"); return; }
            if (!Guid.TryParse(args[0], out var productId)) { Error("Некоректный ID товара"); return; }
            var product = _productRepo.GetById(productId);
            if (product == null) { Error("Товар не найден"); return; }
            try
            {
                _cartService.AddToCart(_authService.currentUser.Id, productId, (int)product.Price, _authService.currentUser.Role);
                Success($"Товар '{product.Name}' добавлен в корзину");
            }
            catch (Exception ex) { Error(ex.Message); }
        }
    }
}
