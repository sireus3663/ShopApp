using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Db.Interfaces;
using ShopProject.Models;
using ShopProject.Services.Interfaces;
using System;

namespace ShopProject.ConsoleCommands.ProductServiceCommand
{
    public class DeleteProductCommand : BaseCommand
    {
        private readonly IProductService _productService;
        private readonly IProductRepository _productRepo;
        private readonly IAuthService _authService;

        public override string Name => "delete-product";
        public override string Description => "Удалить товар. Использование: delete-product <id>";
        public override List<Role> AvailableFor => new List<Role> { Role.Admin };

        public DeleteProductCommand(IProductService productService, IProductRepository productRepo, IAuthService authService)
        {
            _productService = productService;
            _productRepo = productRepo;
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

            if (user.Role != Role.Admin)
            {
                Error("Только администратор может удалять товары");
                return;
            }

            try
            {
                _productService.Delete(productId);
                Success($"Товар '{product.Name}' удалён");
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }
    }
}