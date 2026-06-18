using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Models;
using System;

using ShopProject.Services;
namespace ShopProject.ConsoleCommands
{
    public class BuyCartCommand : BaseCommand
    {
        private readonly IOrderService _orderService;
        private readonly IAuthService _authService;

        public override string Name => "buy";
        public override string Description => "Оформить покупку всех товаров из корзины";
        public override List<Role> AvailableFor => new List<Role> { Role.Buyer, Role.Seller };

        public BuyCartCommand(IOrderService orderService, IAuthService authService)
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

            try
            {
                _orderService.BuyCart();
                Success("Покупка успешно оформлена");
                Info("Спасибо за покупку!");
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }
    }
}