using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Models;
using ShopProject.Services;
using System;

namespace ShopProject.ConsoleCommands.ProductServiceCommand.ModeratorCommand
{
    public class DeclineProductCommand : BaseCommand
    {
        private readonly IProductService _productService;
        private readonly IAuthService _authService;

        public override string Name => "decline";
        public override string Description => "Отклонить товар. Использование: decline <id товара>";
        public override List<Role> AvailableFor => new List<Role> { Role.Moderator, Role.Admin };

        public DeclineProductCommand(IProductService productService, IAuthService authService)
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
                Error("Только модераторы и администраторы могут отклонять товары");
                return;
            }

            if (!Guid.TryParse(args[0], out var productId))
            {
                Error("Некорректный ID товара");
                return;
            }

            try
            {
                _productService.Decline(productId);
                Success("Товар отклонён и удалён");
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }
    }
}