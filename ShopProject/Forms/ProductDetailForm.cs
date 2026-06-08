using System;
using System.Drawing;
using System.Windows.Forms;
using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;

namespace ShopProject.Forms
{
    public class ProductDetailForm : Form
    {
        private readonly Product _product;
        private readonly AuthService _authService;
        private readonly AppDbContext _context;

        public ProductDetailForm(Product product, AuthService authService, AppDbContext context)
        {
            _product = product;
            _authService = authService;
            _context = context;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = _product.Name;
            this.Size = new Size(520, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            var picBox = new PictureBox
            {
                Location = new Point(0, 0),
                Size = new Size(520, 200),
                BackColor = Color.FromArgb(226, 232, 240)
            };
            var iconLbl = new Label
            {
                Text = "📦",
                Font = new Font("Segoe UI", 52),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            picBox.Controls.Add(iconLbl);

            var lblName = new Label
            {
                Text = _product.Name,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(20, 215),
                Size = new Size(480, 50),
                AutoEllipsis = true
            };

            var lblCategory = new Label
            {
                Text = $"📁 Категория: {_product.Category ?? "Без категории"}",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(20, 265),
                AutoSize = true
            };

            var lblAmount = new Label
            {
                Text = _product.Amount > 0 ? $"✓ В наличии: {_product.Amount} шт." : "✗ Нет в наличии",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = _product.Amount > 0 ? Color.FromArgb(22, 163, 74) : Color.Red,
                Location = new Point(20, 290),
                AutoSize = true
            };

            var lblPrice = new Label
            {
                Text = $"{_product.Price:N0} ₽",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                Location = new Point(20, 318),
                AutoSize = true
            };

            var lblDescTitle = new Label
            {
                Text = "Описание",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(20, 365),
                AutoSize = true
            };

            var txtDesc = new RichTextBox
            {
                Text = string.IsNullOrWhiteSpace(_product.Description)
                    ? "Описание не указано." : _product.Description,
                Location = new Point(20, 390),
                Size = new Size(470, 80),
                Font = new Font("Segoe UI", 10),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            var btnAddCart = new Button
            {
                Text = "🛒 В корзину",
                Location = new Point(20, 490),
                Size = new Size(220, 44),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = _product.Amount > 0 ? Color.FromArgb(37, 99, 235) : Color.LightGray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = _product.Amount > 0 ? Cursors.Hand : Cursors.Default,
                Enabled = _product.Amount > 0
            };
            btnAddCart.FlatAppearance.BorderSize = 0;
            btnAddCart.Click += (s, e) => AddToCart();

            var btnFavorite = new Button
            {
                Text = "❤ В избранное",
                Location = new Point(255, 490),
                Size = new Size(185, 44),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(244, 63, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnFavorite.FlatAppearance.BorderSize = 0;
            btnFavorite.Click += (s, e) => ToggleFavorite();

            this.Controls.AddRange(new Control[]
            {
                picBox, lblName, lblCategory, lblAmount,
                lblPrice, lblDescTitle, txtDesc,
                btnAddCart, btnFavorite
            });
        }

        private void AddToCart()
        {
            var user = _authService.currentUser;
            if (user == null)
            {
                MessageBox.Show("Войдите в аккаунт.", "Требуется вход",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var cartRepo = new CartRepository(_context);
            var existing = cartRepo.GetCartItem(user.Id, _product.Id);
            if (existing != null) existing.Count++;
            else cartRepo.Add(new Cart
            {
                Id = Guid.NewGuid(), UserId = user.Id, ProductId = _product.Id,
                Count = 1
            });
            cartRepo.Save();
            MessageBox.Show($"«{_product.Name}» добавлен в корзину!", "Корзина",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ToggleFavorite()
        {
            var user = _authService.currentUser;
            if (user == null)
            {
                MessageBox.Show("Войдите в аккаунт.", "Требуется вход",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var favRepo = new FavoriteRepository(_context);
            var existing = favRepo.GetFavoriteItem(user.Id, _product.Id);
            if (existing != null)
            {
                favRepo.Delete(existing.Id); favRepo.Save();
                MessageBox.Show("Убрано из избранного.", "Избранное",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                favRepo.Add(new Favorite
                {
                    Id = Guid.NewGuid(), UserId = user.Id, ProductId = _product.Id
                });
                favRepo.Save();
                MessageBox.Show("Добавлено в избранное!", "Избранное",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
