using ShopProject.Db.Interfaces;
using ShopProject.Models;
using ShopProject.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopProject.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IAuthService _authService;

        public ProductService(IProductRepository productRepository, IAuthService authService)
        {
            _productRepository = productRepository;
            _authService = authService;
        }

        public List<Product> GetAllApproved()
        {
            return _productRepository.GetAll().Where(p => p.IsApproved).ToList();
        }

        public List<Product> GetForModerate()
        {
            if (!PermissionService.CanModerate(_authService.RequireUser().Role))
                throw new Exception("Только модераторы могут видить список непроверенных товаров");
            return _productRepository.GetAll().Where(p => !p.IsApproved).ToList();
        }

        public Product CreateProduct(string Name, string Description, decimal Price, string Category, byte[]? ProductImage = null, int Amount = 0)
        {
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
                Amount = Amount,
                IsApproved = false,
                ProductImage = ProductImage
            };
            _productRepository.Add(newProduct);
            return newProduct;
        }

        public Product Approve(Guid ProductId)
        {
            var product = _productRepository.GetById(ProductId);
            if (product == null) throw new Exception("Товар не найден");
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
            if (product == null) throw new Exception("Товар не найден");
            if (product.IsApproved) throw new Exception("Нельзя отклонить уже одобренный товар");
            var currentUser = _authService.RequireUser();
            if (!PermissionService.CanModerate(currentUser.Role))
                throw new Exception("Только модераторы или админы могут отклонять продукт");
            _productRepository.Delete(ProductId);
        }

        public void Delete(Guid ProductId)
        {
            var product = _productRepository.GetById(ProductId);
            if (product == null) throw new Exception("Товар не найден");
            if (!product.IsApproved) throw new Exception("Нельзя удалить неодобренный товар");
            var currentUser = _authService.RequireUser();
            if (!PermissionService.CanAdministrate(currentUser.Role))
                throw new Exception("Только админы могут удалять продукт");
            _productRepository.Delete(ProductId);
        }

        public async Task<List<Product>> GetAllApprovedAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Where(p => p.IsApproved).ToList();
        }

        public async Task<List<Product>> GetForModerateAsync()
        {
            if (!PermissionService.CanModerate(_authService.RequireUser().Role))
                throw new Exception("Только модераторы могут видить список непроверенных товаров");
            var products = await _productRepository.GetAllAsync();
            return products.Where(p => !p.IsApproved).ToList();
        }

        public async Task<Product> CreateProductAsync(string Name, string Description, decimal Price, string Category, byte[]? ProductImage = null, int Amount = 0)
        {
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
                Amount = Amount,
                IsApproved = false,
                ProductImage = ProductImage
            };
            await _productRepository.AddAsync(newProduct);
            return newProduct;
        }

        public async Task<Product> ApproveAsync(Guid ProductId)
        {
            var product = await _productRepository.GetByIdAsync(ProductId);
            if (product == null) throw new Exception("Товар не найден");
            var currentUser = _authService.RequireUser();
            if (!PermissionService.CanModerate(currentUser.Role))
                throw new Exception("Только модераторы или админы могут подтвержать товар");
            product.IsApproved = true;
            await _productRepository.UpdateAsync(product);
            return product;
        }

        public async Task DeclineAsync(Guid ProductId)
        {
            var product = await _productRepository.GetByIdAsync(ProductId);
            if (product == null) throw new Exception("Товар не найден");
            if (product.IsApproved) throw new Exception("Нельзя отклонить уже одобренный товар");
            var currentUser = _authService.RequireUser();
            if (!PermissionService.CanModerate(currentUser.Role))
                throw new Exception("Только модераторы или админы могут отклонять продукт");
            await _productRepository.DeleteAsync(ProductId);
        }

        public async Task<PaginatedResult<Product>> GetApprovedProductsPaginatedAsync(
            string? searchText = null,
            string? category = null,
            decimal? priceFrom = null,
            decimal? priceTo = null,
            int page = 1,
            int pageSize = 12)
        {
            return await _productRepository.GetApprovedProductsPaginatedAsync(
                searchText, category, priceFrom, priceTo, page, pageSize);
        }
    }
}