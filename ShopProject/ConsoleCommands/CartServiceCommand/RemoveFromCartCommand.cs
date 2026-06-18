using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Models;
using System;

using ShopProject.Services;
namespace ShopProject.ConsoleCommands.CartServiceCommand
{
    public class RemoveFromCartCommand : BaseCommand
    {
        private readonly IAuthService _authService;
        private readonly ICartService _cartService;

        public override string Name => "cart-remove";
        public override string Description => "Удалить из корзины. Использование: cart-remove <id товара>";
        public override List<Role> AvailableFor => new List<Role> { Role.Buyer };

        public RemoveFromCartCommand(IAuthService authService, ICartService cartService)
        {
            _authService = authService;
            _cartService = cartService;
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

            try
            {
                _cartService.RemoveFromCart(productId);
                Success("Товар удалён из корзины");
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }
    }
}