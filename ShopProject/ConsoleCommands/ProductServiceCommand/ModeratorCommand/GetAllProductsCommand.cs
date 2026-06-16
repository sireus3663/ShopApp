using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Services.Interfaces;
using System;

namespace ShopProject.ConsoleCommands.ProductServiceCommand.ModeratorCommand
{
    public class GetAllProductsCommand : BaseCommand
    {
        private readonly IProductService _productService;

        public override string Name => "products";
        public override string Description => "Показать все одобренные товары";
        public override bool AvailableForGuest => true;

        public GetAllProductsCommand(IProductService productService) { _productService = productService; }
        public override void Execute(string[] args)
        {
            var products = _productService.GetAllApproved();
            if (products.Count == 0)
            {
                Info("Товаров нет");
                return;
            }

            Info($"Всего товаров: {products.Count}");
            Console.WriteLine(new string('-', 60));

            foreach (var p in products)
            {
                Console.WriteLine($"{p.Id} {p.Name,-25} {p.Price,-10} руб. | {p.Category}");
            }
            Console.WriteLine(new string('-', 60));
        }
    }
}