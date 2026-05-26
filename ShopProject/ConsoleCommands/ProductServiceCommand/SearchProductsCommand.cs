using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.ProductServiceCommand
{
    public class SearchProductsCommand : BaseCommand
    {
        private readonly ProductRepository _productRepo;

        public override string Name => "search";
        public override string Description => "Поиск товаров. Испорльзование: search <название>";
        public SearchProductsCommand(ProductRepository productRepo) { _productRepo = productRepo; }
        public override void Execute(string[] args)
        {
            if (args.Length < 1) { Error("Укажите название для поиска"); return; }
            var products = _productRepo.Search(args[0]);
            if (products.Count == 0) { Info($"Товары по запросу '{args[0]}' не найдены"); return; }

            Info ($"Найдено {products.Count} товаров");
            Console.WriteLine(new string('-', 80));
            Console.WriteLine($"{"ID",-36} {"Название",-25} {"Цена",-10} {"Категория",-15}");
            Console.WriteLine(new string('-', 80));

            foreach (var p in products ) { Console.WriteLine($"{p.Id,-36} {p.Name,-25} {p.Price,-10} руб. | {p.Category}"); }
        }
    }
}
