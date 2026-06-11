using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShopProject.Db;
using ShopProject.Models;

namespace ShopProject.WinForms.ViewModels
{
    public class ModeratorViewModel
    {
        private readonly ProductRepository _productRepo;

        public ModeratorViewModel(ProductRepository productRepo)
        {
            _productRepo = productRepo;
        }

        public List<Product> GetProductsForModeration()
        {
            return _productRepo.GetAll()
                .Where(p => !p.IsApproved)
                .ToList();
        }

        public void ApproveProduct(Guid productId)
        {
            var product = _productRepo.GetById(productId);
            if (product == null)
                throw new Exception("Товар не найден");

            product.IsApproved = true;
            _productRepo.Update(product);
        }

        public void DeclineProduct(Guid productId)
        {
            var product = _productRepo.GetById(productId);
            if (product == null)
                throw new Exception("Товар не найден");

            _productRepo.Delete(productId);
        }

        public string GetProductInfo(Product product)
        {
            return $"ID: {product.Id}\nНазвание: {product.Name}\nЦена: {product.Price} руб.\nКатегория: {product.Category}\nПродавец: {product.SellerId}";
        }

        public async Task<List<Product>> GetProductsForModerationAsync()
        {
            var products = await _productRepo.GetAllAsync();
            return products.Where(p => !p.IsApproved).ToList();
        }

        public async Task ApproveProductAsync(Guid productId)
        {
            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null)
                throw new Exception("Товар не найден");

            product.IsApproved = true;
            await _productRepo.UpdateAsync(product);
        }

        public async Task DeclineProductAsync(Guid productId)
        {
            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null)
                throw new Exception("Товар не найден");

            await _productRepo.DeleteAsync(productId);
        }

        public async Task<string> GetProductInfoAsync(Product product)
        {
            return await Task.FromResult($"ID: {product.Id}\nНазвание: {product.Name}\nЦена: {product.Price} руб.\nКатегория: {product.Category}\nПродавец: {product.SellerId}");
        }
    }
}