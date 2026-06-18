using ShopProject.ConsoleCommands.BaseCommands;
using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.OrderServiceCommand
{
    public class ReturnOrderCommand : BaseCommand
    {
        private readonly OrderService _orderService;
        private readonly AuthService _authService;
        private readonly OrderRepository _orderRepo;
        private readonly UserRepository _userRepo;

        public override string Name => "return-product";
        public override string Description => "Вернуть заказ. Использование: return <id заказа>";
        public override List<Role> AvailableFor => new List<Role> { Role.Buyer };

        public ReturnOrderCommand(OrderService orderService, AuthService authService, OrderRepository orderRepo, UserRepository userRepo)
        {
            _orderService = orderService;
            _authService = authService;
            _orderRepo = orderRepo;
            _userRepo = userRepo;
        }

        public override void Execute(string[] args)
        {
            if (args.Length < 1) { Error("Укажите ID заказа для возврата"); return; }
            if (_authService.currentUser == null) { Error("Сначала выполните вход"); return; }
            if (!Guid.TryParse(args[0], out var orderId)) { Error("Некорректный ID заказа"); return; }
            
            try
            {
                var order = _orderRepo.GetById(orderId);
                if (order == null) { Error("Заказ не найден"); return; }
                if (order.UserId != _authService.currentUser.Id) { Error("Вы можете вернуть только свои заказы"); }

                var user = _userRepo.GetById(order.UserId);
                user.Balance += order.Price;
                _userRepo.Update(user);

                _orderRepo.Delete(orderId);
                Success($"Заказ {orderId} возвращён. Возвращено {order.Price} руб.");
            }
            catch (Exception ex) { Error(ex.Message); }
        }
    }
}
