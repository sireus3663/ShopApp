using Microsoft.EntityFrameworkCore;
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
            InitializeComponent();
            _context = context;
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
                ofd.Title = "Выберите фото товара";

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
                    throw new Exception("Введите название");

                if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
                    throw new Exception("Введите цену");

                if (!int.TryParse(txtAmount.Text, out int amount) || amount < 0)
                    throw new Exception("Введите количество");

                if (string.IsNullOrWhiteSpace(txtCategory.Text))
                    throw new Exception("Введите категорию");

                var currentUser = _authService.RequireUser();

                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = txtName.Text,
                    Description = txtDescription.Text,
                    Price = price,
                    Category = txtCategory.Text,
                    Amount = amount,
                    SellerId = currentUser.Id,
                    IsApproved = false,
                    ProductImage = ConvertImageToBytes()
                };

                var productRepo = new ProductRepository(_context);
                productRepo.Add(product);

                MessageBox.Show("Товар создан", "Успех");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}