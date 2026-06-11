using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;

namespace ShopProject.Forms
{
    public partial class ProductDetailForm : Form
    {
        private readonly Product _product;
        private readonly AuthService _authService;
        private readonly CartService _cartService;
        private readonly DiscountService _discountService;

        public ProductDetailForm(Product product, AuthService authService, AppDbContext context)
        {
            _product = product;
            _authService = authService;
            InitializeComponent();

            var cartRepo = new CartRepository(context);
            var discountRepo = new DiscountRepository(context);
            var productRepo = new ProductRepository(context);

            _cartService = new CartService(cartRepo, authService);
            _discountService = new DiscountService(discountRepo, authService, productRepo);

            LoadProductDetails();
        }

        private void LoadProductDetails()
        {
            this.Text = _product.Name;
            lblName.Text = _product.Name;
            lblCategory.Text = $"Категория: {_product.Category}";
            lblAmount.Text = $"В наличии: {_product.Amount} шт.";
            lblDescription.Text = _product.Description;

            decimal finalPrice = _discountService.CalculatePrice(_product);

            if (finalPrice < _product.Price)
            {
                lblPrice.Text = $"{finalPrice:N0} ₽";
                lblOldPrice.Text = $"{_product.Price:N0} ₽";
                lblOldPrice.Visible = true;
            }
            else
            {
                lblPrice.Text = $"{_product.Price:N0} ₽";
                lblOldPrice.Visible = false;
            }

            if (_product.ProductImage != null && _product.ProductImage.Length > 0)
            {
                using (var ms = new MemoryStream(_product.ProductImage))
                {
                    pbImage.Image = Image.FromStream(ms);
                }
            }

            btnAddToCart.Click += (s, e) => AddToCart();
        }

        private void AddToCart()
        {
            try
            {
                _cartService.AddToCart(_product.Id);
                MessageBox.Show("Товар добавлен в корзину", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }
    }
}   