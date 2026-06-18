using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Models;
using System;
using System.Globalization;
using ShopProject.Db;
using ShopProject.Services;
namespace ShopProject.ConsoleCommands.ProductServiceCommand
{
    public class AddDiscountCommand : BaseCommand
    {
        private readonly IDiscountService _discountService;
        private readonly IProductRepository _productRepo;
        private readonly IAuthService _authService;

        public override string Name => "add-discount";
        public override string Description => "Добавить скидку на товар. Использование: add-discount <id товара> <процент>";
        public override List<Role> AvailableFor => new List<Role> { Role.Seller, Role.Admin };

        public AddDiscountCommand(IDiscountService discountService, IProductRepository productRepo, IAuthService authService)
        {
            _discountService = discountService;
            _productRepo = productRepo;
            _authService = authService;
        }

        public override void Execute(string[] args)
        {
            if (args.Length < 2) { Error("Укажите ID товара и процент скидки"); return; }
            if (_authService.CurrentUser == null) { Error("Сначала выполните вход"); return; }
            if (!Guid.TryParse(args[0], out var productId)) { Error("Некорректный ID товара"); return; }
            if (!decimal.TryParse(args[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var percent)) { Error("Процент должен быть числом"); return; }
            var product = _productRepo.GetById(productId);
            if (product == null) { Error("Товар не найден"); return; }
            if (product.SellerId != _authService.CurrentUser.Id && _authService.CurrentUser.Role != Role.Admin) { Error("Вы можете добавлять скидку только на свои товары"); return; }

            try
            {
                var discount = _discountService.CreateDiscount(product, percent);
                Success($"Скидка {percent}% добавлена на товар '{product.Name}'");
            }
            catch (Exception ex) { Error(ex.Message); }
        }
    }
}