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
            Text = _product.Name;
            lblName.Text = _product.Name;
            lblCategory.Text = $"\u041A\u0430\u0442\u0435\u0433\u043E\u0440\u0438\u044F: {_product.Category}";
            lblDescription.Text = _product.Description;

            decimal finalPrice = _discountService.CalculatePrice(_product);

            if (finalPrice < _product.Price)
            {
                lblPrice.Text = $"{finalPrice:N0} \u20BD";
                lblOldPrice.Text = $"{_product.Price:N0} \u20BD";
                lblOldPrice.Visible = true;
            }
            else
            {
                lblPrice.Text = $"{_product.Price:N0} \u20BD";
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
                MessageBox.Show("\u0422\u043E\u0432\u0430\u0440 \u0434\u043E\u0431\u0430\u0432\u043B\u0435\u043D \u0432 \u043A\u043E\u0440\u0437\u0438\u043D\u0443", "\u0423\u0441\u043F\u0435\u0445");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "\u041E\u0448\u0438\u0431\u043A\u0430");
            }
        }
    }
}
