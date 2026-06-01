using ShopProject.ConsoleCommands.BasseCommands;
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
    public class TopProductsCommand : BaseCommand
    {
        private readonly OrderRepository _orderRepo;
        private readonly ProductRepository _productRepo;
        private readonly AuthService _authService;

        public override string Name => "top-products";
        public override string Description => "Топ товаров по продажам (только админ)";
        public override List<Role> AvailableFor => new List<Role> { Role.Admin };

        public TopProductsCommand(OrderRepository orderRepo, ProductRepository productRepo, AuthService authService)
        {
            _orderRepo = orderRepo;
            _productRepo = productRepo;
            _authService = authService;
        }
        public override void Execute(string[] args)
        {
            if (_authService.currentUser == null) { Error("Сначала выполните вход"); return; }
            if (_authService.currentUser.Role != Models.Role.Admin) { Error("Только администратор может просматривать статистику"); return; }
            var orders = _orderRepo.GetAll();
            var topProducts = orders
                .GroupBy(o => o.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Count = g.Sum(o => o.Count),
                    Revenue = g.Sum(o => o.Price)
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();
            if (topProducts.Count == 0) { Error("Нет данных о продажах"); return; }

            Console.WriteLine("\n=== ТОП-10 ТОВАРОВ ПО ПРОДАЖАМ ===");
            Console.WriteLine(new string('-', 80));
            Console.WriteLine($"{"Название",-35} {"Продано шт.",-12} {"Выручка",-15}");
            Console.WriteLine(new string('-', 80));
            foreach (var item in topProducts)
            {
                var product = _productRepo.GetById(item.ProductId);
                string name = product?.Name ?? "Удалённый товар";
                Console.WriteLine($"{name,-35} {item.Count,-12} {item.Revenue,-15} руб.");
            }
            Console.WriteLine(new string('-', 80));
        }
    }
}
