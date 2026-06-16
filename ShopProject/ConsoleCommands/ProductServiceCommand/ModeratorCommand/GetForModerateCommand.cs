using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Models;
using ShopProject.Services;
using ShopProject.Services.Interfaces;
using System;

namespace ShopProject.ConsoleCommands.ProductServiceCommand.ModeratorCommand
{
    public class GetForModerateCommand : BaseCommand
    {
        private readonly IAuthService _authService;
        private readonly IProductService _productService;

        public override string Name => "moderate";
        public override string Description => "Показать товары на модерации (только модератор/админ)";
        public override List<Role> AvailableFor => new List<Role> { Role.Moderator, Role.Admin };

        public GetForModerateCommand(IAuthService authService, IProductService productService)
        {
            _authService = authService;
            _productService = productService;
        }
        public override void Execute(string[] args)
        {
            var user = _authService.CurrentUser;
            if (user == null)
            {
                Error("Сначала выполните вход");
                return;
            }

            if (!PermissionService.CanModerate(user.Role))
            {
                Error("Только модераторы и администраторы могут просматривать товары на модерации");
                return;
            }

            try
            {
                var products = _productService.GetForModerate();
                if (products.Count == 0)
                {
                    Info("Нет товаров, ожидающих модерации");
                    return;
                }

                Info($"Товаров на модерации: {products.Count}");
                Console.WriteLine(new string('-', 80));
                Console.WriteLine($"{"ID",-36} {"Название",-25} {"Цена",-10} {"Категория",-15}");
                Console.WriteLine(new string('-', 80));

                foreach (var p in products)
                {
                    Console.WriteLine($"{p.Id,-36} {p.Name,-25} {p.Price,-10} руб. | {p.Category}");
                }
                Console.WriteLine(new string('-', 80));
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }
    }
}