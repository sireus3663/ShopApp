using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Db.Interfaces;
using ShopProject.Models;
using ShopProject.Services.Interfaces;
using System;

namespace ShopProject.ConsoleCommands.OrderServiceCommand
{
    public class ReturnOrderCommand : BaseCommand
    {
        private readonly IOrderService _orderService;
        private readonly IAuthService _authService;
        private readonly IOrderRepository _orderRepo;
        private readonly IUserRepository _userRepo;

        public override string Name => "return-product";
        public override string Description => "Вернуть заказ. Использование: return <id заказа>";
        public override List<Role> AvailableFor => new List<Role> { Role.Buyer };

        public ReturnOrderCommand(IOrderService orderService, IAuthService authService, IOrderRepository orderRepo, IUserRepository userRepo)
        {
            _orderService = orderService;
            _authService = authService;
            _orderRepo = orderRepo;
            _userRepo = userRepo;
        }

        public override void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                Error("Укажите ID заказа для возврата");
                return;
            }

            if (string.IsNullOrWhiteSpace(args[0]))
            {
                Error("ID заказа не может быть пустым");
                return;
            }

            var user = _authService.CurrentUser;
            if (user == null)
            {
                Error("Сначала выполните вход");
                return;
            }

            if (!Guid.TryParse(args[0], out var orderId))
            {
                Error("Некорректный ID заказа");
                return;
            }

            try
            {
                var order = _orderRepo.GetById(orderId);
                if (order == null)
                {
                    Error("Заказ не найден");
                    return;
                }

                if (order.UserId != user.Id)
                {
                    Error("Вы можете вернуть только свои заказы");
                    return;
                }

                var orderUser = _userRepo.GetById(order.UserId);
                if (orderUser == null)
                {
                    Error("Пользователь не найден");
                    return;
                }

                orderUser.Balance += order.Price;
                _userRepo.Update(orderUser);

                _orderRepo.Delete(orderId);
                Success($"Заказ {orderId} возвращён. Возвращено {order.Price} руб.");
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }
    }
}