using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;

namespace ShopProject.Forms.ViewModels
{
    public class MainViewModel
    {
        private readonly AuthService _authService;
        private readonly ProductRepository _productRepo;
        private readonly CartService _cartService;
        private readonly FavoriteService _favoriteService;
        private List<Guid> _favoritesList;

        public MainViewModel(
            AuthService authService,
            ProductRepository productRepo,
            CartService cartService,
            FavoriteService favoriteService)
        {
            _authService = authService;
            _productRepo = productRepo;
            _cartService = cartService;
            _favoriteService = favoriteService;
            _favoritesList = new List<Guid>();
        }

        public User CurrentUser => _authService.currentUser;
        public bool IsAuthenticated => CurrentUser != null;
        public void LoadFavorites()
        {
            if (IsAuthenticated)
            {
                _favoritesList = _favoriteService.GetFavorites();
            }
        }

        public List<Product> GetApprovedProducts()
        {
            return _productRepo.GetAll()
                .Where(p => p.IsApproved)
                .ToList();
        }

        public List<Product> SearchProducts(string searchText)
        {
            var products = GetApprovedProducts();
            if (string.IsNullOrWhiteSpace(searchText))
                return products;

            return products.Where(p => p.Name.ToLower().Contains(searchText.ToLower())).ToList();
        }

        public List<Product> FilterByCategory(List<Product> products, string category)
        {
            if (string.IsNullOrEmpty(category) || category == "Все категории")
                return products;

            return products.Where(p => p.Category == category).ToList();
        }

        public List<string> GetCategories()
        {
            return GetApprovedProducts()
                .Select(p => p.Category)
                .Distinct()
                .ToList();
        }

        public void AddToCart(Guid productId)
        {
            if (!IsAuthenticated)
                throw new Exception("Войдите в аккаунт");

            _cartService.AddToCart(productId);
        }

        public void RemoveFromCart(Guid productId)
        {
            _cartService.RemoveFromCart(productId);
        }

        public List<Cart> GetCurrentUserCart()
        {
            if (!IsAuthenticated)
                return new List<Cart>();

            return _cartService.GetCurrentUserCart();
        }

        public int GetCartItemsCount()
        {
            return GetCurrentUserCart().Sum(c => c.Count);
        }

        public decimal GetCartTotalPrice()
        {
            var cart = GetCurrentUserCart();
            decimal total = 0;
            foreach (var item in cart)
            {
                var product = _productRepo.GetById(item.ProductId);
                if (product != null)
                    total += product.Price * item.Count;
            }
            return total;
        }

        public bool IsFavorite(Guid productId)
        {
            return _favoritesList.Contains(productId);
        }

        public void ToggleFavorite(Guid productId)
        {
            if (!IsAuthenticated)
                throw new Exception("Войдите в аккаунт");

            _favoriteService.ToggleFavorite(productId);
            if (_favoritesList.Contains(productId))
                _favoritesList.Remove(productId);
            else
                _favoritesList.Add(productId);
        }

        public Product GetProduct(Guid productId)
        {
            return _productRepo.GetById(productId);
        }

        public List<Product> GetMyProducts()
        {
            if (!IsAuthenticated)
                return new List<Product>();

            return _productRepo.GetAll()
                .Where(p => p.SellerId == CurrentUser.Id)
                .ToList();
        }

        public List<Product> GetFavoriteProducts()
        {
            if (!IsAuthenticated)
                return new List<Product>();

            return _favoritesList
                .Select(id => _productRepo.GetById(id))
                .Where(p => p != null && p.IsApproved)
                .ToList();
        }

        public async void Login(string email, string password)
        {
            await _authService.Login(email, password);
            LoadFavorites();
        }

        public async void Logout()
        {
            await _authService.Logout();
            _favoritesList.Clear();
        }

        public AuthService GetAuthService()
        {
            return _authService;
        }

        public void LoginById(User user)
        {
            _authService.LoginById(user);
            LoadFavorites();
        }

        public async Task LoadFavoritesAsync()
        {
            if (IsAuthenticated)
            {
                _favoritesList = await _favoriteService.GetFavoritesAsync();
            }
        }

        public async Task<List<Product>> GetApprovedProductsAsync()
        {
            var products = await _productRepo.GetAllAsync();
            return products.Where(p => p.IsApproved).ToList();
        }

        public async Task<List<Product>> SearchProductsAsync(string searchText)
        {
            var products = await GetApprovedProductsAsync();
            if (string.IsNullOrWhiteSpace(searchText))
                return products;

            return products.Where(p => p.Name.ToLower().Contains(searchText.ToLower())).ToList();
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            var products = await GetApprovedProductsAsync();
            return products.Select(p => p.Category).Distinct().ToList();
        }

        public async Task AddToCartAsync(Guid productId)
        {
            if (!IsAuthenticated)
                throw new Exception("Войдите в аккаунт");

            await _cartService.AddToCartAsync(productId);
        }

        public async Task RemoveFromCartAsync(Guid productId)
        {
            await _cartService.RemoveFromCartAsync(productId);
        }

        public async Task<List<Cart>> GetCurrentUserCartAsync()
        {
            if (!IsAuthenticated)
                return new List<Cart>();

            return await _cartService.GetCurrentUserCartAsync();
        }

        public async Task<int> GetCartItemsCountAsync()
        {
            var cart = await GetCurrentUserCartAsync();
            return cart.Sum(c => c.Count);
        }

        public async Task<decimal> GetCartTotalPriceAsync()
        {
            var cart = await GetCurrentUserCartAsync();
            decimal total = 0;
            foreach (var item in cart)
            {
                var product = await _productRepo.GetByIdAsync(item.ProductId);
                if (product != null)
                    total += product.Price * item.Count;
            }
            return total;
        }

        public async Task<bool> IsFavoriteAsync(Guid productId)
        {
            return _favoritesList.Contains(productId);
        }

        public async Task ToggleFavoriteAsync(Guid productId)
        {
            if (!IsAuthenticated)
                throw new Exception("Войдите в аккаунт");

            await _favoriteService.ToggleFavoriteAsync(productId);
            if (_favoritesList.Contains(productId))
                _favoritesList.Remove(productId);
            else
                _favoritesList.Add(productId);
        }

        public async Task<Product> GetProductAsync(Guid productId)
        {
            return await _productRepo.GetByIdAsync(productId);
        }

        public async Task<List<Product>> GetMyProductsAsync()
        {
            if (!IsAuthenticated)
                return new List<Product>();

            var products = await _productRepo.GetAllAsync();
            return products.Where(p => p.SellerId == CurrentUser.Id).ToList();
        }

        public async Task<List<Product>> GetFavoriteProductsAsync()
        {
            if (!IsAuthenticated)
                return new List<Product>();

            var products = new List<Product>();
            foreach (var id in _favoritesList)
            {
                var product = await _productRepo.GetByIdAsync(id);
                if (product != null && product.IsApproved)
                    products.Add(product);
            }
            return products;
        }

        public void UpdateCurrentUserBalance(decimal newBalance)
        {
            if (_authService.currentUser != null)
            {
                _authService.currentUser.Balance = newBalance;
            }
        }



    }
}