using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.ProductServiceCommand.ModeratorCommand
{
    public class DeclineProductCommand : BaseCommand
    {
        private readonly ProductService _productService;
        private readonly AuthService _authService;

        public override string Name => "decline";
        public override string Description => "Отклонить товар. Использование: decline <id товара>";
        public DeclineProductCommand(ProductService productService, AuthService authService)
        {
            _productService = productService;
            _authService = authService;
        }
        public override void Execute(string[] args)
        {
            if (args.Length < 1) { Error("Уажаите ID товара"); return; }
            if (_authService.currentUser == null) { Error("Сначала выполните вход"); return; }
            if (!PermissionService.CanModerate(_authService.currentUser.Role)) { Error("Только модераторы и администраторы могут отклонять товары"); return; }
            if (!Guid.TryParse(args[0], out var productId)) { Error("Некорректный ID товара"); return; }
            try
            {
                _productService.Decline(productId);
                Success($"Товар отклонён и удалён");
            }
            catch (Exception ex) { Error(ex.Message); }
        }
    }
}
