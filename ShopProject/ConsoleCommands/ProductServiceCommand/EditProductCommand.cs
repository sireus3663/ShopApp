using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Models;
using System;
using System.Globalization;
using ShopProject.Db;
using ShopProject.Services;
namespace ShopProject.ConsoleCommands.ProductServiceCommand
{
    public class EditProductCommand : BaseCommand
    {
        private readonly IProductRepository _productRepo;
        private readonly IAuthService _authService;

        public override string Name => "edit-product";
        public override string Description => "Редактировать товар. Использование: edit-product <id> <price|name|desc|category> <новое_значение>";
        public override List<Role> AvailableFor => new List<Role> { Role.Seller, Role.Admin };

        public EditProductCommand(IProductRepository productRepo, IAuthService authService)
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

            if (string.IsNullOrWhiteSpace(args[0]))
            {
                Error("ID товара не может быть пустым");
                return;
            }

            var user = _authService.CurrentUser;
            if (user == null)
            {
                Error("Сначала выполните вход");
                return;
            }

            if (!Guid.TryParse(args[0], out var productId))
            {
                Error("Некорректный ID товара");
                return;
            }

            var product = _productRepo.GetById(productId);
            if (product == null)
            {
                Error("Товар не найден");
                return;
            }

            if (product.SellerId != user.Id && user.Role != Role.Admin)
            {
                Error("Вы можете редактировать только свои товары");
                return;
            }

            if (string.IsNullOrWhiteSpace(args[1]))
            {
                Error("Поле не может быть пустым");
                return;
            }

            if (string.IsNullOrWhiteSpace(args[2]))
            {
                Error("Значение не может быть пустым");
                return;
            }

            string field = args[1].ToLower();
            string newValue = args[2].Trim();

            try
            {
                switch (field)
                {
                    case "price":
                        if (!decimal.TryParse(newValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var price) || price <= 0)
                        {
                            Error("Цена должна быть положительным числом");
                            return;
                        }
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
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }
    }
}