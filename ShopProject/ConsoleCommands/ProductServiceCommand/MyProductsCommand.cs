using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Models;
using ShopProject.Services;
using System;
using System.Linq;

using ShopProject.Db;
namespace ShopProject.ConsoleCommands.ProductServiceCommand
{
    public class MyProductsCommand : BaseCommand
    {
        private readonly IProductRepository _productRepo;
        private readonly IAuthService _authService;

        public override string Name => "my-products";
        public override string Description => "Показать мои товары (продавец)";
        public override List<Role> AvailableFor => new List<Role> { Role.Seller, Role.Admin };

        public MyProductsCommand(IProductRepository productRepo, IAuthService authService)
        {
            _productRepo = productRepo;
            _authService = authService;
        }

        public override void Execute(string[] args)
        {
            var user = _authService.CurrentUser;
            if (user == null)
            {
                Error("Сначала выполните вход");
                return;
            }

            if (!PermissionService.CanSell(user.Role))
            {
                Error("Только продавцы могут просматривать свои товары");
                return;
            }

            var products = _productRepo.GetAll()
                .Where(p => p.SellerId == user.Id)
                .ToList();

            if (products.Count == 0)
            {
                Info("У вас нет товаров");
                return;
            }

            Console.WriteLine($"\nМои товары ({products.Count}):");
            Console.WriteLine(new string('-', 100));
            Console.WriteLine($"{"ID",-36} {"Название",-25} {"Цена",-10} {"Категория",-15} {"Статус",-10}");
            Console.WriteLine(new string('-', 100));

            foreach (var p in products)
            {
                string status = p.IsApproved ? "Одобрен" : "На модерации";
                Console.WriteLine($"{p.Id,-36} {p.Name,-25} {p.Price,-10} руб. {p.Category,-15} {status,-10}");
            }
            Console.WriteLine(new string('-', 100));
        }
    }
}