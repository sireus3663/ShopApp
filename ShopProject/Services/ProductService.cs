using Microsoft.EntityFrameworkCore;
using ShopProject.Db;
using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.Services
{
    public class ProductService
    {
        private readonly ProductRepository _productRepository;
        private readonly AuthService _authService;

        public ProductService(ProductRepository productRepository, AuthService authService)
        {
            _productRepository = productRepository;
            _authService = authService;
        }

        public List<Product> GetAllApproved()
        {
            return _productRepository.GetAll()
                .Where(p => p.IsApproved)
                .ToList();
        }
        public List<Product> GetForModerate()
        {
            if (!PermissionService.CanModerate(_authService.RequireUser().Role))
                throw new Exception("Только модераторы могут видить список непроверенных товаров");
            return _productRepository.GetAll()
                .Where(p => !p.IsApproved)
                .ToList();
        }
        public Product createProduct(string Name, string Description, decimal Price, string Category)
        {
            if (string.IsNullOrWhiteSpace(Name) || Name.Length < 3)
                throw new Exception("Название продукта должно быть не менее 3 символов");
            if (Name.Length > 100)
                throw new Exception("Название должно быть короче 100 символов");
            if (Price < 0)
                throw new Exception("Цена не может быть отрицательной");
            if (Price > 10000000)
                throw new Exception("Цена не может быть больше 10000000");
            if (Math.Round(Price, 2) != Price)
                throw new Exception("Цена может содержать не более 2 знаков после запятой");
            if (string.IsNullOrWhiteSpace(Category))
                throw new Exception("Категория не может быть пустой");
            if (Category.Length > 50)
                throw new Exception("Длина категории должна быть меньше 50 символов");
            if (string.IsNullOrWhiteSpace(Description))
                throw new Exception("Описание не может быть пустым");
            if (Description.Length > 500)
                throw new Exception("Длина описания должна быть меньше 500 символов");
            var currentUser = _authService.RequireUser();
            if (!PermissionService.CanSell(currentUser.Role))
                throw new Exception("Только продавцы могут создавать товары");
            var newProduct = new Product
            {
                Id = Guid.NewGuid(),
                Name = Name,
                Description = Description,
                Price = Price,
                SellerId = currentUser.Id,
                Category = Category,
                IsApproved = false
            };
            _productRepository.Add(newProduct);
            return newProduct;
        }
        public Product Approve(Guid ProductId)
        {
            var product = _productRepository.GetById(ProductId);
            if (product == null)
                throw new Exception("Товар не найден");
            var currentUser = _authService.RequireUser();
            if (!PermissionService.CanModerate(currentUser.Role))
                throw new Exception("Только модераторы или админы могут подтвержать товар");
            product.IsApproved = true;
            _productRepository.Update(product);
            return product;
        }
        public void Decline(Guid ProductId)
        {
            var product = _productRepository.GetById(ProductId);
            if (product == null)
                throw new Exception("Товар не найден");
            if (product.IsApproved)
                throw new Exception("Нельзя отклонить уже одобренный товар");
            var currentUser = _authService.RequireUser();
            if (!PermissionService.CanModerate(currentUser.Role))
                throw new Exception("Только модераторы или админы могут отклонять продукт");
            _productRepository.Delete(ProductId);
        }
        public void Delete(Guid ProductId)
        {
            var product = _productRepository.GetById(ProductId);
            if (product == null)
                throw new Exception("Товар не найден");
            if (!product.IsApproved)
                throw new Exception("Нельзя удалить неодобренный товар. Сначала одобрите или отклоните.");
            var currentUser = _authService.RequireUser();
            if (!PermissionService.CanAdministrate(currentUser.Role))
                throw new Exception("Только админы могут удалять продукт");
            _productRepository.Delete(ProductId);
        }
    }
}