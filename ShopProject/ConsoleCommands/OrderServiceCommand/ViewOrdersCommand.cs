using ShopProject.ConsoleCommands.BasseCommands;
using System;

using ShopProject.Services;
namespace ShopProject.ConsoleCommands.OrderServiceCommand
{
    public class ViewOrdersCommand : BaseCommand
    {
        private readonly IOrderService _orderService;
        private readonly IAuthService _authService;

        public override string Name => "my-order";
        public override string Description => "Показать все мои заказы";

        public ViewOrdersCommand(IOrderService orderService, IAuthService authService)
        {
            _orderService = orderService;
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

            var orders = _orderService.GetUserOrders(user.Id);

            if (orders.Count == 0)
            {
                Info("У вас нет заказов");
                return;
            }

            Info($"Всего заказов: {orders.Count}");
            Console.WriteLine(new string('-', 90));
            Console.WriteLine($"{"ID заказа",-36} {"Дата",-12} {"Цена",-10} {"Кол-во",-6}");
            Console.WriteLine(new string('-', 90));

            foreach (var o in orders)
            {
                Console.WriteLine($"{o.Id,-36} {o.CreatedAt:dd.MM.yyyy,-12} {o.Price,-10} руб. {o.Count,-6} шт.");
            }
            Console.WriteLine(new string('-', 90));
        }
    }
}