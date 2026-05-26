using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Db;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.ProductServiceCommand
{
    public class AddDiscountCommand : BaseCommand
    {
        private readonly DiscountService _discountService;
        private readonly ProductRepository _productRepo;
        private readonly AuthService _authService;

        public override string Name => "add-descount";
        public override string Description => "Добавить скидку на товар. Использование: add-discount <id товара> <процент>";
        
        public AddDiscountCommand(DiscountService discountService, ProductRepository productRepo, AuthService authService)
        {
            _discountService = discountService;
            _productRepo = productRepo;
            _authService = authService;
        }
        public override void Execute(string[] args)
        {
            if (args.Length < 2) { Error("кажите ID товара и процент скидки"); return; }
            if (_authService.currentUser == null) { Error("Сначала выполните вход"); return; }
            if (!Guid.TryParse(args[0], out var productId)) { Error("Некорректный ID товара"); return; }
            if (!Decimal.TryParse(args[1], out var percent)) { Error("Процент должен быть числом"); return; }
            var product = _productRepo.GetById(productId);
            if (product == null) { Error("Товар не найден"); return; }
            if (product.SellerId != _authService.currentUser.Id && _authService.currentUser.Role != Models.Role.Admin) { Error("Вы можете добавлять скидку только на свои товары"); return; }

            try
            {
                var discount = _discountService.CreateDiscount(product, percent);
                Success($"Скидка {percent}% добавлена на товар '{product.Name}'");
            }
            catch (Exception ex) { Error(ex.Message); }
        }
    }
}
