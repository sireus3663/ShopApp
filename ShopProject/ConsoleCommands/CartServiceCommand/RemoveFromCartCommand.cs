using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopProject.Services;
using ShopProject.Db;

namespace ShopProject.ConsoleCommands.CartServiceCommand
{
    public class RemoveFromCartCommand : BaseCommand
    {
        private readonly CartService _cartService;
        private readonly AuthService _authService;

        public override string Name => "cart-remove";
        public override string Description => "Удалить из корзины. Использование: cart-remove <id товара>";
        public RemoveFromCartCommand(CartService cartService, AuthService authService)
        {
            _cartService = cartService;
            _authService = authService;
        }
        public override void Execute(string[] args)
        {
            if (args.Length < 1) { Error("Укажите ID товара"); return; }
            if (_authService.currentUser == null) { Error("Сначала выполните вход"); return; }
            if (!Guid.TryParse(args[0], out var productId)) { Error("Некоректный ID товара"); return; }
            try
            {
                _cartService.RemoveFromCart(_authService.currentUser.Id, productId);
                Success($"Товар удалён из корзины");
            }
            catch (Exception ex) { Error(ex.Message); }
        }
    }
}
