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
    public class DeleteProductCommand : BaseCommand
    {
        private readonly ProductService _productService;
        private readonly ProductRepository _productRepo;
        private readonly AuthService _authService;

        public override string Name => "product delete";
        public override string Description => "Удалить товар. Использование: product delete <id>";
        public override List<Role> AvailableFor => new List<Role> { Role.Admin };

        public DeleteProductCommand(ProductService productService, ProductRepository productRepo, AuthService authService)
        {
            _productService = productService;
            _productRepo = productRepo;
            _authService = authService;
        }
        public override void Execute(string[] args)
        {
            if (args.Length < 1) { Error("Укажите ID товара"); return; }
            if (_authService.currentUser == null) { Error("Сначала выполните вход"); return; }
            if (!Guid.TryParse(args[0], out var productId)) { Error("Некорректный ID товара"); return; }
            var product =_productRepo.GetById(productId);
            if (product == null) { Error("Товар не найден"); return; }
            if (_authService.currentUser.Role != Models.Role.Admin) { Error("Только администратор может удалять товары"); return; }

            try
            {
                _productService.Delete(productId);
                Success($"Товар '{product.Name}' удалён");
            }
            catch (Exception ex) { Error(ex.Message); }
        }
    }

}
