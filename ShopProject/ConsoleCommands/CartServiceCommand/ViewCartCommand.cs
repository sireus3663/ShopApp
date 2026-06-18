using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Models;
using System;
using System.Collections.Generic;

using ShopProject.Db;
using ShopProject.Services;
namespace ShopProject.ConsoleCommands.CartServiceCommand
{
    public class ViewCartCommand : BaseCommand
    {
        private readonly ICartService _cartService;
        private readonly IAuthService _authService;
        private readonly IProductRepository _productRepo;

        public override string Name => "cart-view";
        public override string Description => "Показать корзину";
        public override List<Role> AvailableFor => new List<Role> { Role.Buyer, Role.Seller };

        public ViewCartCommand(ICartService cartService, IAuthService authService, IProductRepository productRepo)
        {
            _cartService = cartService;
            _authService = authService;
            _productRepo = productRepo;
        }
        public override void Execute(string[] args)
        {
            var user = _authService.CurrentUser;
            if (user == null)
            {
                Error("Сначала выполните вход");
                return;
            }

            var cartItems = _cartService.GetCurrentUserCart();
            if (cartItems.Count == 0)
            {
                Info("Корзина пуста");
                return;
            }

            Console.WriteLine($"\nКорзина пользователя: {user.Name}");
            Console.WriteLine(new string('=', 95));
            Console.WriteLine($"{"ID товара",-38} {"Название",-25} {"Цена",-10} {"Кол-во",-6}");
            Console.WriteLine(new string('-', 95));

            foreach (var item in cartItems)
            {
                var product = _productRepo.GetById(item.ProductId);
                if (product != null)
                {
                    Console.WriteLine($"{item.ProductId,-38} {product.Name,-25} {product.Price,-10} руб. {item.Count,-6} шт.");
                }
                else
                {
                    Console.WriteLine($"{item.ProductId,-38} {"Товар удалён",-25} {"0",-10} руб. {item.Count,-6} шт.");
                }
            }
            Console.WriteLine(new string('=', 95));
        }
    }
}