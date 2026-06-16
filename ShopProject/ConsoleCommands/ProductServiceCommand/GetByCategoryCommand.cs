using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Db.Interfaces;
using System;

namespace ShopProject.ConsoleCommands.ProductServiceCommand
{
    public class GetByCategoryCommand : BaseCommand
    {
        private readonly IProductRepository _productRepo;

        public override string Name => "category";
        public override string Description => "Товары по категории. Использование: category <категория>";
        public override bool AvailableForGuest => true;
        public GetByCategoryCommand(IProductRepository productRepo) { _productRepo = productRepo; }
        public override void Execute(string[] args)
        {
            if (args.Length < 1) { Error("Укажите категорию"); return; }
            var products = _productRepo.GetByCategory(args[0]);
            if (products.Count == 0) { Info($"Товары в категории '{args[0]}' не найдено"); return; }

            Info($"Товары в категории '{args[0]}': {products.Count}");
            Console.WriteLine(new string('-', 80));
            Console.WriteLine($"{"ID",-36} {"Название",-25} {"Цена",-10}");
            Console.WriteLine(new string('-', 80));

            foreach (var p in products) { Console.WriteLine($"{p.Id,-36} {p.Name,-25} {p.Price,-10} руб."); }
        }
    }
}