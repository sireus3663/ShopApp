using System;
using System.Drawing;
using System.Windows.Forms;
using ShopProject.Db;
using ShopProject.Models;

namespace ShopProject.WinForms
{
    public class CreateProductForm : Form
    {
        private AppDbContext _context;
        private User _author;
        private TextBox txtName;
        private TextBox txtPrice;
        private TextBox txtCategory;
        private TextBox txtDescription;
        private Button btnSave;

        public CreateProductForm(AppDbContext context, User author)
        {
            _context = context;
            _author = author;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Создание товара";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int y = 30;

            var lblName = new Label
            {
                Text = "Название:",
                ForeColor = Color.FromArgb(60, 60, 60),
                Location = new Point(30, y),
                Size = new Size(100, 25)
            };
            txtName = new TextBox
            {
                Location = new Point(140, y),
                Size = new Size(300, 30),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(60, 60, 60),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(lblName);
            this.Controls.Add(txtName);
            y += 45;

            var lblPrice = new Label
            {
                Text = "Цена:",
                ForeColor = Color.FromArgb(60, 60, 60),
                Location = new Point(30, y),
                Size = new Size(100, 25)
            };
            txtPrice = new TextBox
            {
                Location = new Point(140, y),
                Size = new Size(300, 30),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(60, 60, 60),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(lblPrice);
            this.Controls.Add(txtPrice);
            y += 45;

            var lblCategory = new Label
            {
                Text = "Категория:",
                ForeColor = Color.FromArgb(60, 60, 60),
                Location = new Point(30, y),
                Size = new Size(100, 25)
            };
            txtCategory = new TextBox
            {
                Location = new Point(140, y),
                Size = new Size(300, 30),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(60, 60, 60),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(lblCategory);
            this.Controls.Add(txtCategory);
            y += 45;

            var lblDescription = new Label
            {
                Text = "Описание:",
                ForeColor = Color.FromArgb(60, 60, 60),
                Location = new Point(30, y),
                Size = new Size(100, 25)
            };
            txtDescription = new TextBox
            {
                Location = new Point(140, y),
                Size = new Size(300, 100),
                Multiline = true,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(60, 60, 60),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(lblDescription);
            this.Controls.Add(txtDescription);
            y += 120;

            btnSave = new Button
            {
                Text = "Сохранить",
                Location = new Point(180, y),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Укажите название товара", "Ошибка");
                    return;
                }

                if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
                {
                    MessageBox.Show("Укажите корректную цену", "Ошибка");
                    return;
                }

                var repo = new ProductRepository(_context);
                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = txtName.Text,
                    Price = price,
                    Category = txtCategory.Text,
                    Description = txtDescription.Text,
                    SellerId = _author?.Id,
                    IsApproved = false,
                    Amount = 1
                };
                repo.Add(product);

                MessageBox.Show("Товар создан и отправлен на модерацию", "Успешно");
                this.Close();
            }
            catch (Exception ex)
            {
                ErrorForm.Show(ex.Message, ex);
            }
        }
    }
}