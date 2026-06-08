using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;

namespace ShopProject.Forms
{
    public class CartPanel : Panel, IRefreshable
    {
        private readonly AuthService _authService;
        private readonly AppDbContext _context;
        private Label lblTitle, lblTotal, lblEmpty;
        private ListView listCart;
        private Button btnClearCart, btnCheckout;

        public CartPanel(AuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.BackColor = Color.FromArgb(245, 247, 250);

            lblTitle = new Label
            {
                Text = "Корзина",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(25, 25),
                AutoSize = true
            };

            listCart = new ListView
            {
                Location = new Point(25, 70),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White
            };
            listCart.Columns.Add("Товар", 300);
            listCart.Columns.Add("Кол-во", 80);
            listCart.Columns.Add("Цена за шт.", 120);
            listCart.Columns.Add("Сумма", 120);
            listCart.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            lblEmpty = new Label
            {
                Text = "Корзина пуста",
                Font = new Font("Segoe UI", 13),
                ForeColor = Color.Gray,
                Location = new Point(25, 200),
                AutoSize = true,
                Visible = false
            };

            lblTotal = new Label
            {
                Text = "Итого: 0 ₽",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                Location = new Point(25, 520),
                AutoSize = true
            };

            btnClearCart = new Button
            {
                Text = "Очистить корзину",
                Location = new Point(25, 560),
                Size = new Size(180, 38),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClearCart.FlatAppearance.BorderSize = 0;
            btnClearCart.Click += (s, e) => ClearCart();

            btnCheckout = new Button
            {
                Text = "✓ Оформить заказ",
                Location = new Point(220, 560),
                Size = new Size(200, 38),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(22, 163, 74),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCheckout.FlatAppearance.BorderSize = 0;
            btnCheckout.Click += (s, e) => Checkout();

            this.Controls.AddRange(new Control[]
            {
                lblTitle, listCart, lblEmpty, lblTotal, btnClearCart, btnCheckout
            });

            this.Resize += (s, e) =>
            {
                listCart.Size = new Size(this.Width - 50, this.Height - 200);
                lblTotal.Location = new Point(25, this.Height - 120);
                btnClearCart.Location = new Point(25, this.Height - 75);
                btnCheckout.Location = new Point(220, this.Height - 75);
            };
        }

        public new void Refresh()
        {
            listCart.Items.Clear();
            var user = _authService.currentUser;
            if (user == null) { lblEmpty.Visible = true; return; }

            try
            {
                var cartRepo = new CartRepository(_context);
                var items = cartRepo.GetByUser(user.Id);

                if (!items.Any()) { lblEmpty.Visible = true; lblTotal.Text = "Итого: 0 ₽"; return; }
                lblEmpty.Visible = false;

                decimal total = 0;
                foreach (var item in items)
                {
                    var product = _context.products.Find(item.ProductId);
                    if (product == null) continue;

                    decimal sum = product.Price * item.Count;
                    total += sum;

                    var row = new ListViewItem(product.Name);
                    row.SubItems.Add(item.Count.ToString());
                    row.SubItems.Add($"{product.Price:N0} ₽");
                    row.SubItems.Add($"{product.Price * item.Count:N0} ₽");
                    row.Tag = item;
                    listCart.Items.Add(row);
                }
                lblTotal.Text = $"Итого: {total:N0} ₽";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки корзины: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ClearCart()
        {
            var user = _authService.currentUser;
            if (user == null) return;
            var cartRepo = new CartRepository(_context);
            var items = cartRepo.GetByUser(user.Id);
            foreach (var item in items) cartRepo.Delete(item.Id);
            cartRepo.Save();
            Refresh();
        }

        private void Checkout()
        {
            MessageBox.Show("Заказ оформлен!\n\nПодключите OrderService для полной обработки.",
                "Заказ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    public class FavoritesPanel : Panel, IRefreshable
    {
        private readonly AuthService _authService;
        private readonly AppDbContext _context;
        private Label lblTitle, lblEmpty;
        private FlowLayoutPanel flowFavorites;

        public FavoritesPanel(AuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.BackColor = Color.FromArgb(245, 247, 250);

            lblTitle = new Label
            {
                Text = "Избранное",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(25, 25),
                AutoSize = true
            };

            flowFavorites = new FlowLayoutPanel
            {
                Location = new Point(25, 70),
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.FromArgb(245, 247, 250)
            };
            flowFavorites.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            lblEmpty = new Label
            {
                Text = "❤  Здесь пока ничего нет.\nДобавляйте товары в избранное!",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.Gray,
                Location = new Point(25, 120),
                AutoSize = true,
                Visible = false
            };

            this.Controls.AddRange(new Control[] { lblTitle, flowFavorites, lblEmpty });
            this.Resize += (s, e) =>
                flowFavorites.Size = new Size(this.Width - 50, this.Height - 80);
        }

        public new void Refresh()
        {
            flowFavorites.Controls.Clear();
            var user = _authService.currentUser;
            if (user == null) { lblEmpty.Visible = true; return; }

            try
            {
                var favRepo = new FavoriteRepository(_context);
                var favIds = favRepo.GetByUser(user.Id).Select(f => f.ProductId).ToList();

                if (!favIds.Any()) { lblEmpty.Visible = true; return; }
                lblEmpty.Visible = false;

                foreach (var id in favIds)
                {
                    var product = _context.products.Find(id);
                    if (product == null) continue;
                    flowFavorites.Controls.Add(MakeFavCard(product, favRepo, user.Id));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private Panel MakeFavCard(Product product, FavoriteRepository favRepo, Guid userId)
        {
            var card = new Panel
            {
                Size = new Size(220, 120),
                BackColor = Color.White,
                Margin = new Padding(6)
            };
            card.Paint += (s, e) =>
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(220, 220, 220)), 0, 0, card.Width - 1, card.Height - 1);

            var lblName = new Label
            {
                Text = product.Name,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(200, 40),
                AutoEllipsis = true
            };

            var lblPrice = new Label
            {
                Text = $"{product.Price:N0} ₽",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                Location = new Point(10, 55),
                AutoSize = true
            };

            var btnRemove = new Button
            {
                Text = "✕ Убрать",
                Location = new Point(10, 85),
                Size = new Size(90, 26),
                Font = new Font("Segoe UI", 8),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRemove.FlatAppearance.BorderSize = 0;
            btnRemove.Click += (s, e) =>
            {
                var item = favRepo.GetFavoriteItem(userId, product.Id);
                if (item != null) { favRepo.Delete(item.Id); favRepo.Save(); }
                Refresh();
            };

            var btnCart = new Button
            {
                Text = "🛒",
                Location = new Point(110, 85),
                Size = new Size(50, 26),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCart.FlatAppearance.BorderSize = 0;

            card.Controls.AddRange(new Control[] { lblName, lblPrice, btnRemove, btnCart });
            return card;
        }
    }

    public class ProfilePanel : Panel, IRefreshable
    {
        private readonly AuthService _authService;
        private Label lblTitle, lblName, lblEmail, lblRole, lblBalance,
                      lblNameVal, lblEmailVal, lblRoleVal, lblBalanceVal,
                      lblStats, lblOrdersCount;
        private Button btnEditName, btnChangePassword;
        private GroupBox grpInfo, grpStats;

        public ProfilePanel(AuthService authService)
        {
            _authService = authService;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.BackColor = Color.FromArgb(245, 247, 250);

            lblTitle = new Label
            {
                Text = "Личный кабинет",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(25, 25),
                AutoSize = true
            };

            grpInfo = new GroupBox
            {
                Text = "Мои данные",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(25, 65),
                Size = new Size(420, 220),
                BackColor = Color.White
            };

            void AddInfoRow(string labelText, ref Label lbl, ref Label val, int y)
            {
                lbl = new Label { Text = labelText, Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, Location = new Point(15, y), Size = new Size(120, 24) };
                val = new Label { Text = "—", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(30, 30, 30), Location = new Point(140, y), Size = new Size(260, 24) };
                grpInfo.Controls.Add(lbl);
                grpInfo.Controls.Add(val);
            }

            AddInfoRow("Имя:", ref lblName, ref lblNameVal, 35);
            AddInfoRow("Email:", ref lblEmail, ref lblEmailVal, 70);
            AddInfoRow("Роль:", ref lblRole, ref lblRoleVal, 105);
            AddInfoRow("Баланс:", ref lblBalance, ref lblBalanceVal, 140);

            btnEditName = new Button
            {
                Text = "✏ Изменить имя",
                Location = new Point(15, 175),
                Size = new Size(160, 32),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnEditName.FlatAppearance.BorderSize = 0;
            btnEditName.Click += BtnEditName_Click;

            btnChangePassword = new Button
            {
                Text = "🔒 Сменить пароль",
                Location = new Point(185, 175),
                Size = new Size(160, 32),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(100, 116, 139),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnChangePassword.FlatAppearance.BorderSize = 0;
            btnChangePassword.Click += BtnChangePassword_Click;

            grpInfo.Controls.AddRange(new Control[] { btnEditName, btnChangePassword });

            grpStats = new GroupBox
            {
                Text = "Статистика",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(25, 305),
                Size = new Size(420, 100),
                BackColor = Color.White
            };

            lblOrdersCount = new Label
            {
                Text = "Заказов: —",
                Font = new Font("Segoe UI", 11),
                Location = new Point(15, 35),
                AutoSize = true
            };
            grpStats.Controls.Add(lblOrdersCount);

            this.Controls.AddRange(new Control[] { lblTitle, grpInfo, grpStats });
        }

        public new void Refresh()
        {
            var user = _authService.currentUser;
            if (user == null)
            {
                lblNameVal.Text = "Не авторизован";
                lblEmailVal.Text = "—";
                lblRoleVal.Text = "—";
                lblBalanceVal.Text = "—";
                return;
            }

            lblNameVal.Text    = user.Name;
            lblEmailVal.Text   = user.Email;
            lblRoleVal.Text    = user.Role.ToString();
            lblBalanceVal.Text = $"{user.Balance:N0} ₽";
        }

        private void BtnEditName_Click(object sender, EventArgs e)
        {
            var user = _authService.currentUser;
            if (user == null) return;

            var input = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите новое имя:", "Изменить имя", user.Name);

            if (!string.IsNullOrWhiteSpace(input) && input != user.Name)
            {
                user.Name = input;
                lblNameVal.Text = input;
                MessageBox.Show("Имя обновлено!", "Готово",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnChangePassword_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Функция смены пароля будет добавлена.\n(Подключить UserService.ChangePassword)",
                "В разработке", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
