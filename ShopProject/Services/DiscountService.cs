using ShopProject.Models;
using System;
using ShopProject.Db;
using System.Threading.Tasks;

namespace ShopProject.Services
{
    public interface IDiscountService
    {
        Discount CreateDiscount(Product product, decimal percent);
        Discount? GetByProduct(Guid productId);
        decimal CalculatePrice(Product product);
        Discount RemoveDiscount(Guid productId);
    }
}

namespace ShopProject.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly IDiscountRepository _discountRepository;
        private readonly IAuthService _authService;
        private readonly IProductRepository _productRepository;

        public DiscountService(IDiscountRepository discountRepository, IAuthService authService, IProductRepository productRepository)
        {
            _discountRepository = discountRepository;
            _authService = authService;
            _productRepository = productRepository;
        }

        public Discount CreateDiscount(Product product, decimal percent)
        {
            if (product == null) throw new Exception("РџСЂРѕРґСѓРєС‚Р° РЅРµ СЃСѓС‰РµСЃС‚РІСѓРµС‚");
            var existingDiscount = GetByProduct(product.Id);
            if (existingDiscount != null) throw new Exception("РЎРєРёРґРєР° РЅР° С‚РѕРІР°СЂ СѓР¶Рµ СЃСѓС‰РµСЃС‚РІСѓРµС‚");
            var currentUser = _authService.RequireUser();
            if (percent < 0) throw new Exception("РџСЂРѕС†РµРЅС‚ СЃРєРёРґРєРё РЅРµ РјРѕР¶РµС‚ Р±С‹С‚СЊ РѕС‚СЂРёС†Р°С‚РµР»СЊРЅС‹Рј");
            if (percent > 99) throw new Exception("РџСЂРѕС†РµРЅС‚ СЃРєРёРґРєРё РЅРµ РјРѕР¶РµС‚ Р±С‹С‚СЊ Р±РѕР»СЊС€Рµ 99 РїСЂРѕС†РµРЅС‚РѕРІ");
            if (product.SellerId != currentUser.Id) throw new Exception("РўРѕР»СЊРєРѕ РІР»Р°РґРµР»РµС† С‚РѕРІР°СЂР° РјРѕР¶РµС‚ РёР·РјРµРЅСЏС‚СЊ/РґРѕР±Р°РІР»СЏС‚СЊ СЃРєРёРґРєСѓ");

            Discount discount = new Discount
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Percent = percent
            };
            _discountRepository.Add(discount);
            return discount;
        }

        public Discount? GetByProduct(Guid productId)
        {
            return _discountRepository.GetByProduct(productId);
        }

        public decimal CalculatePrice(Product product)
        {
            if (product == null) throw new Exception("РџСЂРѕРґСѓРєС‚Р° РЅРµ СЃСѓС‰РµСЃС‚РІСѓРµС‚");
            var disc = GetByProduct(product.Id);
            if (disc == null) return product.Price;
            return product.Price - (product.Price * disc.Percent / 100);
        }

        public Discount RemoveDiscount(Guid productId)
        {
            var disc = GetByProduct(productId);
            var currentUser = _authService.RequireUser();
            var product = _productRepository.GetById(productId);
            if (product == null) throw new Exception("РџСЂРѕРґСѓРєС‚Р° РЅРµ СЃСѓС‰РµСЃС‚РІСѓРµС‚");
            if (disc == null) throw new Exception("РќР° РїСЂРѕРґСѓРєС‚ РЅРµС‚ СЃРєРёРґРєРё");
            if (currentUser.Id != product.SellerId) throw new Exception("РџРѕР»СЊР·РѕРІР°С‚РµР»СЊ РЅРµ СЏРІР»СЏРµС‚СЃСЏ РІР»Р°РґРµР»СЊС†РµРј РїСЂРѕРґСѓРєС‚Р°");
            _discountRepository.Delete(disc.Id);
            return disc;
        }
    }
}