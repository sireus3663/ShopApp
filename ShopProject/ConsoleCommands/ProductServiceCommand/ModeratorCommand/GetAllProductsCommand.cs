using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Models;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.ProductServiceCommand.ModeratorCommand
{
    public class GetAllProductsCommand : BaseCommand
    {
        private readonly ProductService _productService;

        public override string Name => "products";
        public override string Description => "Показать все одобренные товары";
        public GetAllProductsCommand(ProductService productService) { _productService = productService; }
        public override void Execute(string[] args)
        {
            var products = _productService.GetAllApproved();
            if(products.Count == 0) { Info("Товаров нет"); return; }
            Info($"Всего товаров {products.Count}");
            Console.WriteLine(new string('-', 60));
            foreach (var p in products) { Console.WriteLine($"{p.Id} {p.Name,-25} {p.Price,-10} руб. | {p.Category}"); }
        }
    }
}
