using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Models;
using System;

using ShopProject.Db;
using ShopProject.Services;
namespace ShopProject.ConsoleCommands.CartServiceCommand
{
    public class AddToCartCommand : BaseCommand
    {
        private readonly ICartService _cartService;
        private readonly IAuthService _authService;
        private readonly IProductRepository _productRepo;

        public override string Name => "cart-add";
        public override string Description => "Добавить в корзину. Использование: cart-add <id товара>";
        public override List<Role> AvailableFor => new List<Role> { Role.Buyer };

        public AddToCartCommand(ICartService cartService, IAuthService authService, IProductRepository productRepo)
        {
            _cartService = cartService;
            _authService = authService;
            _productRepo = productRepo;
        }
        public override void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                Error("Укажите ID товара");
                return;
            }

            if (string.IsNullOrWhiteSpace(args[0]))
            {
                Error("ID товара не может быть пустым");
                return;
            }

            var user = _authService.CurrentUser;
            if (user == null)
            {
                Error("Сначала выполните вход");
                return;
            }

            if (!Guid.TryParse(args[0], out var productId))
            {
                Error("Некорректный ID товара");
                return;
            }

            var product = _productRepo.GetById(productId);
            if (product == null)
            {
                Error("Товар не найден");
                return;
            }

            try
            {
                _cartService.AddToCart(productId);
                Success($"Товар '{product.Name}' добавлен в корзину");
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }
    }
}