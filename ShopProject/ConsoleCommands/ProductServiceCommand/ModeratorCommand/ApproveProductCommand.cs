using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Models;
using ShopProject.Services;
using ShopProject.Services.Interfaces;
using System;

namespace ShopProject.ConsoleCommands.ProductServiceCommand.ModeratorCommand
{
    public class ApproveProductCommand : BaseCommand
    {
        private readonly IProductService _productService;
        private readonly IAuthService _authService;

        public override string Name => "approve";
        public override string Description => "Одобрить товар. Использование: approve <id товара>";
        public override List<Role> AvailableFor => new List<Role> { Role.Moderator, Role.Admin };

        public ApproveProductCommand(IProductService productService, IAuthService authService)
        {
            _productService = productService;
            _authService = authService;
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

            if (!PermissionService.CanModerate(user.Role))
            {
                Error("Только модераторы и администраторы могут одобрять товары");
                return;
            }

            if (!Guid.TryParse(args[0], out var productId))
            {
                Error("Некорректный ID товара");
                return;
            }

            try
            {
                var product = _productService.Approve(productId);
                if (product != null)
                {
                    Success($"Товар '{product.Name}' одобрен!");
                }
                else
                {
                    Error("Не удалось одобрить товар");
                }
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }
    }
}