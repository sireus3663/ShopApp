using System;
using ShopProject.Db;
using ShopProject.Models;

namespace ShopProject.Forms.ViewModels
{
    public class CreateProductViewModel
    {
        private readonly ProductRepository _productRepo;
        private readonly User _currentUser;

        public CreateProductViewModel(ProductRepository productRepo, User currentUser)
        {
            _productRepo = productRepo;
            _currentUser = currentUser;
        }

        public Product CreateProduct(string name, string description, decimal price, string category)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Укажите название товара");

            if (price <= 0)
                throw new Exception("Цена должна быть больше 0");

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = name,
                Price = price,
                Category = category,
                Description = description,
                SellerId = _currentUser?.Id,
                IsApproved = false
            };

            _productRepo.Add(product);
            return product;
        }
    }
}