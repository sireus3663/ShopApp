using ShopProject.ConsoleCommands.BaseCommands;
using ShopProject.Models;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.CartServiceCommand
{
    public class RemoveFromCartCommand : BaseCommand
    {
        private readonly AuthService _authService;
        private readonly CartService _cartService;

        public override string Name => "cart remove";
        public override string Description => "Удалить из корзины. Использование: cart remove <id товара>";
        public override List<Role> AvailableFor => new List<Role> { Role.Buyer };

        public RemoveFromCartCommand(AuthService authService, CartService cartService)
        {
            _authService = authService;
            _cartService = cartService;
        }

        public override void Execute(string[] args)
        {
            if (args.Length < 1) { Error("Укажите ID товара"); return; }
            if (_authService.currentUser == null) { Error("Сначала выполните вход"); return; }
            if (!Guid.TryParse(args[0], out var productId)) { Error("Некорректный ID товара"); return; }
            try
            {
                _cartService.RemoveFromCart(productId);
                Success("Товар удалён из корзин");
            }
            catch (Exception ex) { Error(ex.Message); }
        }
    }
}
