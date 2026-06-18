using ShopProject.ConsoleCommands.BaseCommands;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.OrderServiceCommand
{
    public class GetUserOrdersCommand : BaseCommand
    {
        private readonly OrderService _orderService;
        private readonly AuthService _authService;

        public override string Name => "orders";
        public override string Description => "Показать мои заказы";
        public GetUserOrdersCommand(OrderService orderService, AuthService authService)
        {
            _orderService = orderService;
            _authService = authService;
        }
        public override void Execute(string[] args)
        {
            if (_authService.currentUser == null) { Error("Сначала выполните вход"); return; }
            var orders = _orderService.getUserOrders(_authService.currentUser.Id);
            if (orders.Count == 0) { Info("У вас нет заказов"); return; }
            Info($"Всего заказов: {orders.Count}");
            Console.WriteLine(new string('-', 90));
            Console.WriteLine($"{"ID заказа",-36} {"Дата",-12} {"Цена",-10} {"Кол-во",-6}");
            Console.WriteLine(new string('-', 90));

            foreach (var o in orders) { Console.WriteLine($"{o.Id,-36} {o.CreatedAt:dd.MM.yyyy,-12} {o.Price,-10} руб. {o.Count,-6} шт."); }
        }
    }
}
