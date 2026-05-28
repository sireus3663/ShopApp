using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands
{
    public class BuyCartCommand : BaseCommand
    {
        private readonly OrderService _orderService;
        private readonly AuthService _authService;

        public override string Name => "buy";
        public override string Description => "Оформить покупку всех товаров из корзины";
        
        public BuyCartCommand(OrderService orderService, AuthService authService)
        {
            _orderService = orderService;
            _authService = authService;
        }

        public override void Execute(string[] args)
        {
            if (_authService.currentUser == null) { Error("Сначала выполните вход"); return; }
            try
            {
                _orderService.BuyCart();
                Success("Покупка успешно оформлена");
                Info("Спасибо за покупку!");
            }
            catch (Exception ex) { Error(ex.Message); }
        }
    }
}
