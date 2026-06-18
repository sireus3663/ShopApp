using ShopProject.ConsoleCommands.BaseCommands;
using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.ProductServiceCommand
{
    public class CreateProductCommand : BaseCommand
    {
        private readonly ProductService _productService;
        private readonly AuthService _authService;

        public override string Name => "create-product";
        public override string Description => "Создание продукта. Использование: create-product <название> <описание> <цена> <категория>";
        public override List<Role> AvailableFor => new List<Role> { Role.Seller, Role.Admin };

        public CreateProductCommand(ProductService productService, AuthService authService)
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
            if (_authService.currentUser == null) { Error("Сначала выполните вход"); return; }
            string name = args[0];
            string description = args[1];

            if (!decimal.TryParse(args[2], out var price)) { Error("Цена должно быть числом"); return; }
            string cotegory = args[3];
            try
            {
                var product = _productService.createProduct(name, description, price, cotegory);
                Success($"Товар '{product.Name}' созда! ID: {product.Id}");
                Info("Товары ожидает проверки модератором");
            }
            catch (Exception ex) { Error(ex.Message); }


        }
    }
}
