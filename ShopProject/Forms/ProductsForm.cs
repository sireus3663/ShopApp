using Microsoft.EntityFrameworkCore;
using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShopProject.Forms
{
    public partial class ProductsForm : Form
    {
        private readonly AuthService _authService;
        private readonly ProductService _productService;
        private readonly CartService _cartService;
        private readonly DiscountService _discountService;
        private readonly AppDbContext _context;

        public ProductsForm(AuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;

            var productRepo = new ProductRepository(context);
            var cartRepo = new CartRepository(context);
            var discountRepo = new DiscountRepository(context);

            _productService = new ProductService(productRepo, authService);
            _cartService = new CartService(cartRepo, authService);
            _discountService = new DiscountService(discountRepo, authService, productRepo);

            InitializeComponent();

            if (flowProducts == null)
            {
                flowProducts = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    Padding = new Padding(10)
                };
                this.Controls.Add(flowProducts);
            }

            var currentUser = _authService.currentUser;
            if (!PermissionService.CanSell(currentUser?.Role ?? Role.Buyer))
            {
                btnAddProduct.Visible = false;
            }

            LoadProducts();
        }

        private void LoadProducts()
        {
            flowProducts.Controls.Clear();

            var products = _productService.GetAllApproved();

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                products = products.Where(p => p.Name.ToLower().Contains(txtSearch.Text.ToLower())).ToList();
            }

            switch (cmbSort.SelectedIndex)
            {
                case 1: products = products.OrderBy(p => p.Price).ToList(); break;
                case 2: products = products.OrderByDescending(p => p.Price).ToList(); break;
                case 3: products = products.OrderBy(p => p.Name).ToList(); break;
                case 4: products = products.OrderByDescending(p => p.Name).ToList(); break;
            }

            foreach (var product in products)
            {
                var card = new ProductCard();
                var finalPrice = _discountService.CalculatePrice(product);
                card.SetProduct(product, finalPrice);

                card.AddToCartClicked += (id) =>
                {
                    try
                    {
                        _cartService.AddToCart(id);
                        MessageBox.Show("Товар добавлен в корзину", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                card.ProductClicked += (product) =>
                {
                    var detailForm = new ProductDetailForm(product, _authService, _context);
                    detailForm.ShowDialog();
                };

                flowProducts.Controls.Add(card);
            }
        }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            var productRepo = new ProductRepository(_context);
            var productService = new ProductService(productRepo, _authService);

            var createForm = new CreateProductForm(_authService, productService, _context);
            if (createForm.ShowDialog() == DialogResult.OK)
            {
                LoadProducts();
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadProducts();
        }
    }
}
