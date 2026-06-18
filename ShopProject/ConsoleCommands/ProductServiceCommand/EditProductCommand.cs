using Microsoft.EntityFrameworkCore;
using ShopProject.ConsoleCommands.BaseCommands;
using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.ProductServiceCommand
{
    public class EditProductCommand : BaseCommand
    {
        private readonly ProductRepository _productRepo;
        private readonly AuthService _authService;

        public override string Name => "edit-product";
        public override string Description => "Редактировать товар. Использование: edit-product <id> <price|name|desc|category> <новое_значение>";
        public override List<Role> AvailableFor => new List<Role> { Role.Seller, Role.Admin };

        public EditProductCommand(ProductRepository productRepo, AuthService authService)
        {
            _productRepo = productRepo;
            _authService = authService;
        }

        public override void Execute(string[] args)
        {
            if (args.Length < 3)
            {
                Error("Укажите ID товара, поле и новое значение");
                Info("Примеры: edit-product <id> price 500");
                Info("         edit-product <id> name НовоеНазвание");
                Info("         edit-product <id> category НоваяКатегория");
                return;
            }
            if (_authService.currentUser == null) { Error("Сначала выполните вход"); return; }
            if (!Guid.TryParse(args[0], out var productId)) { Error("Некорректный ID товара"); return; }
            var product= _productRepo.GetById(productId);
            if (product == null) { Error("Товар не найден"); return; }
            if (product.SellerId != _authService.currentUser.Id && _authService.currentUser.Role !=Models.Role.Admin) { Error("Вы можете редактировать только свои товары"); return; }
            string field = args[1].ToLower();
            string newValue = args[2];

            try
            {
                switch (field)
                {
                    case "price":
                        if (!decimal.TryParse(newValue, out var price)) { Error("Цена должно быть числом"); return; }
                        product.Price = price;
                        break;
                    case "name":
                        product.Name = newValue;
                        break;
                    case "desc":
                    case "description":
                        product.Description = newValue;
                        break;
                    case "category":
                        product.Category = newValue;
                        break;
                    default:
                        Error("Доступные поля: price, name, desc, category");
                        return;
                }
                _productRepo.Update(product);
                Success($"Товар обновлён. {field} = {newValue}");
            }
            catch (Exception ex) { Error(ex.Message); }
        }
    }
}
