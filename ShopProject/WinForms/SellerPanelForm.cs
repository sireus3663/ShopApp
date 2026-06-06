using System;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Windows.Forms;
using ShopProject.Db;
using ShopProject.Models;

namespace ShopProject.WinForms
{
    public class SellerPanelForm : Form
    {
        private AppDbContext _context;
        private User _seller;

        public SellerPanelForm(AppDbContext context, User seller)
        {
            _context = context;
            _seller = seller;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Панель Продавца";
            Size = new Size(700, 500);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(32, 32, 32);

            var title = new Label
            {
                Text = $"Продавец - {_seller.Name}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
            };
            Controls.Add(title);

            var statsBtn = new Button
            {
                Text = "Статистика",
                Location = new Point(20, 70),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            statsBtn.Click += (s, e) => MessageBox.Show("Статистика продавца", "Статистика");
            Controls.Add(statsBtn);

            var myProductsBtn = new Button
            {
                Text = "Мои товары",
                Location = new Point(20, 130),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            myProductsBtn.Click += (s, e) => ShowMyProducts();
            Controls.Add(myProductsBtn);

            var createBtn = new Button
            {
                Text = "Добавить товар",
                Location = new Point(20, 190),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(0, 100, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            createBtn.Click += (s, e) =>
            {
                var createForm = new CreateProductForm(_context, _seller);
                createForm.ShowDialog();
            };
            Controls.Add(createBtn);
        }
        private void ShowMyProducts()
        {
            try
            {
                var repo = new ProductRepository(_context);
                var products = repo.GetAll().Where(p => p.SellerId == _seller.Id).ToList();
                if (products.Count == 0)
                {
                    MessageBox.Show("У вас нет товаров", "Мои товары");
                    return;
                }

                var text = string.Join(Environment.NewLine, products.Select(p => $"{p.Name} — {p.Price} руб. — {(p.IsApproved ? "одобрен" : "в ожидании")}"));
                MessageBox.Show(text, "Мои товары");
            }
            catch (Exception ex)
            {
                ErrorDialogForm.ShowError($"Ошибка получения товаров: " + ex.Message);
            }
        }
    }
}