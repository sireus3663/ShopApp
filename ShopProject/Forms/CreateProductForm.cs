using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ShopProject.Forms
{
    public partial class CreateProductForm : Form
    {
        private readonly AuthService _authService;
        private readonly ProductService _productService;
        private readonly AppDbContext _context;

        private string? selectedImagePath = null;

        public CreateProductForm(AuthService authService, ProductService productService, AppDbContext context)
        {
            _authService = authService;
            _productService = productService;
            _context = context;
            InitializeComponent();
        }

        private byte[]? ConvertImageToBytes()
        {
            if (string.IsNullOrEmpty(selectedImagePath)) return null;
            return File.ReadAllBytes(selectedImagePath);
        }

        private void btnSelectImage_Click_1(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                ofd.Title = "\u0412\u044B\u0431\u0435\u0440\u0438\u0442\u0435 \u0444\u043E\u0442\u043E \u0442\u043E\u0432\u0430\u0440\u0430";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    selectedImagePath = ofd.FileName;
                    pbPreview.Image = Image.FromFile(selectedImagePath);
                }
            }
        }

        private void btnCreate_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                    throw new Exception("\u0412\u0432\u0435\u0434\u0438\u0442\u0435 \u043D\u0430\u0437\u0432\u0430\u043D\u0438\u0435");

                if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
                    throw new Exception("\u0412\u0432\u0435\u0434\u0438\u0442\u0435 \u0446\u0435\u043D\u0443");

                if (string.IsNullOrWhiteSpace(txtCategory.Text))
                    throw new Exception("\u0412\u0432\u0435\u0434\u0438\u0442\u0435 \u043A\u0430\u0442\u0435\u0433\u043E\u0440\u0438\u044E");

                var currentUser = _authService.RequireUser();

                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = txtName.Text,
                    Description = txtDescription.Text,
                    Price = price,
                    Category = txtCategory.Text,
                    SellerId = currentUser.Id,
                    IsApproved = false,
                    ProductImage = ConvertImageToBytes()
                };

                var productRepo = new ProductRepository(_context);
                productRepo.Add(product);

                MessageBox.Show("\u0422\u043E\u0432\u0430\u0440 \u0441\u043E\u0437\u0434\u0430\u043D", "\u0423\u0441\u043F\u0435\u0445");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "\u041E\u0448\u0438\u0431\u043A\u0430");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
