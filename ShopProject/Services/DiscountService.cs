using ShopProject.Db;
using ShopProject.Models;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.Services
{

    public class DiscountService
    {
        private readonly DiscountRepository _discountRepository;
        private readonly AuthService _authService;
        private readonly ProductRepository _productRepository;
        public DiscountService(DiscountRepository discountRepository, AuthService authService, ProductRepository productRepository) {
            _discountRepository = discountRepository;
            _authService = authService;
            _productRepository = productRepository;
        }
        public Discount CreateDiscount(Product product, decimal percent)
        {
            if (product == null) throw new Exception("Продукта не существует");
            var existingDiscount = GetByProduct(product.Id);
            if (existingDiscount != null) throw new Exception("Скидка на товар уже существует");
            var currentUser = _authService.RequireUser();
            if (percent < 0) throw new Exception("Процент скидки не может быть отрицательным");
            if (percent > 99) throw new Exception("Процент скидки не может быть больше 99 процентов");
            if (product.SellerId != currentUser.Id) throw new Exception("Только владелец товара может изменять/добавлять скидку");
            Discount discount = new Discount {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Percent = percent
            };
            _discountRepository.Add(discount);
            return discount;
        }
        public Discount GetByProduct(Guid productId)
        {
            return _discountRepository.GetByProduct(productId);
        }
        public decimal CalculatePrice(Product product)
        {
            if (product == null) throw new Exception("Продукта не существует");
            var disc = GetByProduct(product.Id);
            if (disc == null) throw new Exception("На продукт нет скидки");
            return product.Price - (product.Price * disc.Percent / 100);
        }
        public Discount RemoveDiscount(Guid productId)
        {
            var disc = GetByProduct(productId);
            var currentUser = _authService.RequireUser();
            var product = _productRepository.GetById(productId);
            if (product == null) throw new Exception("Продукта не существует");
            if (disc == null) throw new Exception("На продукт нет скидки");
            if (currentUser.Id != product.SellerId) throw new Exception("Пользователь не является владельцем продукта");
            _discountRepository.Delete(disc.Id);
            return disc;
        }
    }
}