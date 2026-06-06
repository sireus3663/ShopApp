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
        private Button btnSava;

        public CreateProductForm(AppDbContext context, User author)
        {
            _context = context;
            _author = author;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Создание товара";
            Size = new Size(450, 350);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(32, 32, 32);

            var lblName = new Label
            {
                Text = "Название:",
                ForeColor = Color.White,
                Location = new Point(20, 30),
                Size = new Size(100, 25),
            };
            txtName = new TextBox
            {
                Location = new Point(130, 30),
                Size = new Size(250, 25),
                BackColor = Color.FromArgb(64, 64, 64),
                ForeColor = Color.White,
            };

            var lblPrice = new Label
            {
                Text = "Цена:",
                ForeColor = Color.White,
                Location = new Point(20, 80),
                Size = new Size(100, 25),
            };
            txtPrice = new TextBox
            {
                Location = new Point(130, 80),
                Size = new Size(260, 25),
                BackColor = Color.FromArgb(64, 64, 64),
                ForeColor = Color.White,
            };

            var lblCat = new Label
            {
                Text = "Категория:",
                ForeColor = Color.White,
                Location = new Point(20, 130),
                Size = new Size(100, 25),
            };
            txtCategory = new TextBox
            {
                Location = new Point(130, 130),
                Size = new Size(260, 25),
                BackColor = Color.FromArgb(64, 64, 64),
                ForeColor = Color.White,
            };

            btnSava = new Button
            {
                Text = "Сохранить",
                Location = new Point(130, 190),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSava.Click += BtnSave_Click;


            Controls.Add(lblName);
            Controls.Add(txtName);
            Controls.Add(lblPrice);
            Controls.Add(txtPrice);
            Controls.Add(lblCat);
            Controls.Add(btnSava);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtName.Text))
                {
                    MessageBox.Show("Укажите название", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                decimal price = 0;
                decimal.TryParse(txtPrice.Text, out price);

                var repo = new ProductRepository(_context);
                var product = new Product
                {
                    Name = txtName.Text,
                    Price = price,
                    Category = txtCategory.Text,
                    SellerId = _author?.Id,
                    IsApproved = false,
                };
                repo.Add(product);

                MessageBox.Show("Товар создан (в черновике).", "Создано", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                ErrorDialogForm.ShowError("Ошибка создания товара: " + ex.Message);
            }
        }
    }
}
