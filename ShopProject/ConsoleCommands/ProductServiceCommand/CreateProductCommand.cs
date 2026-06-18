using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Models;
using System;
using System.Globalization;
using ShopProject.Services;
namespace ShopProject.ConsoleCommands.ProductServiceCommand
{
    public class CreateProductCommand : BaseCommand
    {
        private readonly IProductService _productService;
        private readonly IAuthService _authService;

        public override string Name => "create-product";
        public override string Description => "Создание продукта. Использование: create-product <название> <описание> <цена> <категория>";
        public override List<Role> AvailableFor => new List<Role> { Role.Seller, Role.Admin };

        public CreateProductCommand(IProductService productService, IAuthService authService)
        {
            _productService = productService;
            _authService = authService;
        }
        public override void Execute(string[] args)
        {
            if (args.Length < 4)
            {
                Error("Укажите все параметры: название, описание, цену, категорию");
                Info("Пример: create-product Хлеб Свежий хлеб 50.00 Хлебобулочные");
                return;
            }

            var user = _authService.CurrentUser;
            if (user == null)
            {
                Error("Сначала выполните вход");
                return;
            }

            if (string.IsNullOrWhiteSpace(args[0]))
            {
                Error("Название не может быть пустым");
                return;
            }

            if (string.IsNullOrWhiteSpace(args[1]))
            {
                Error("Описание не может быть пустым");
                return;
            }

            if (!decimal.TryParse(args[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var price) || price <= 0)
            {
                Error("Цена должна быть положительным числом");
                return;
            }

            if (string.IsNullOrWhiteSpace(args[3]))
            {
                Error("Категория не может быть пустой");
                return;
            }

            string name = args[0].Trim();
            string description = args[1].Trim();
            string category = args[3].Trim();

            try
            {
                var product = _productService.CreateProduct(name, description, price, category);
                Success($"Товар '{product.Name}' создан! ID: {product.Id}");
                Info("Товар ожидает проверки модератором");
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }
    }
}