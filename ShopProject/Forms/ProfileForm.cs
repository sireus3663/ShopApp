using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using ShopProject.Db;
using ShopProject.Services;
using ShopProject.Models;

namespace ShopProject.Forms
{
    public class ProfileForm : Form
    {
        private readonly AuthService _authService;
        private readonly UserService _userService;
        private readonly AppDbContext _context;
        private readonly ProductService _productService;
        private readonly OrderService _orderService;
        private Panel pnlNav = null!;
        private Button btnCart = null!;
        private Button btnFavorites = null!;
        private Button btnProfile = null!;
        private Button btnOrders = null!;
        private Button btnLogout = null!;
        private Label lblAppName = null!;
        private Label lblUserName = null!;
        private Label lblUserRole = null!;

        private Panel pnlContent = null!;
        private CartPanel cartPanel = null!;
        private FavoritesPanel favoritesPanel = null!;
        private ProfilePanel profilePanel = null!;
        private StatisticPanel statisticPanel = null!;
        private OrdersPanel ordersPanel = null!;
        private CreateProductPanel createProductPanel = null!;

        private List<Button> _navButtons = new List<Button>();
        private Button? _activeNavButton = null;

        public Action<Product>? ProductClicked { get; set; }
        public Action<Product, ProductService>? ProductClickedForModeration { get; set; }
        public Action<Order>? OrderClicked { get; set; }

        public ProfileForm(AuthService authService, UserService userService, AppDbContext context,
            ProductService productService, OrderService orderService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            InitializeComponents();
            ShowPanel(profilePanel);
        }

        private void InitializeComponents()
        {
            Text = "ShopApp — Личный кабинет";
            Size = new Size(1100, 700);
            MinimumSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 247, 250);

            BuildNav();
            BuildContent();

            Controls.Add(pnlNav);
            Controls.Add(pnlContent);
        }

        private void BuildNav()
        {
            pnlNav = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Color.FromArgb(15, 23, 42)
            };

            btnLogout = new Button
            {
                Text = "🚪 Выйти из аккаунта",
                Dock = DockStyle.Bottom,
                Height = 45,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(127, 29, 29),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(22, 0, 0, 0)
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var brush = new SolidBrush(btnLogout.BackColor);
                e.Graphics.FillRectangle(brush, btnLogout.ClientRectangle);
                TextRenderer.DrawText(e.Graphics, btnLogout.Text, btnLogout.Font,
                    new Point(22, (btnLogout.Height - TextRenderer.MeasureText(btnLogout.Text, btnLogout.Font).Height) / 2),
                    btnLogout.ForeColor, TextFormatFlags.Default);
            };
            btnLogout.MouseEnter += (s, e) => { btnLogout.BackColor = Color.FromArgb(153, 27, 27); btnLogout.Invalidate(); };
            btnLogout.MouseLeave += (s, e) => { btnLogout.BackColor = Color.FromArgb(127, 29, 29); btnLogout.Invalidate(); };
            btnLogout.Click += BtnLogout_Click;
            pnlNav.Controls.Add(btnLogout);

            var pnlNavScroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(15, 23, 42)
            };
            pnlNav.Controls.Add(pnlNavScroll);

            int y = 24;
            var user = _authService.CurrentUser;

            lblAppName = new Label
            {
                Text = "🛍 ShopApp",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, y),
                AutoSize = true
            };
            pnlNavScroll.Controls.Add(lblAppName);

            y += 38;
            pnlNavScroll.Controls.Add(new Label
            {
                Location = new Point(16, y),
                Size = new Size(188, 1),
                BackColor = Color.FromArgb(51, 65, 85)
            });

            y += 24;
            var pbAvatar = new Panel
            {
                Size = new Size(36, 36),
                Location = new Point(20, y),
                BackColor = Color.FromArgb(37, 99, 235)
            };
            pbAvatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(37, 99, 235)), 0, 0, 35, 35);
                var initial = (user?.Name?.Length > 0 ? user.Name[0].ToString() : "?").ToUpper();
                TextRenderer.DrawText(e.Graphics, initial, new Font("Segoe UI", 14, FontStyle.Bold),
                    pbAvatar.ClientRectangle, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            pnlNavScroll.Controls.Add(pbAvatar);

            lblUserName = new Label
            {
                Text = user?.Name ?? "Гость",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(66, y),
                AutoSize = true
            };
            pnlNavScroll.Controls.Add(lblUserName);

            lblUserRole = new Label
            {
                Text = user != null ? $"[{user.Role}]" : "",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(66, y + 20),
                AutoSize = true
            };
            pnlNavScroll.Controls.Add(lblUserRole);

            var lblBalance = new Label
            {
                Text = user != null ? $"💰 {user.Balance:N0} ₽" : "",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 211, 153),
                Location = new Point(20, y + 46),
                AutoSize = true
            };
            pnlNavScroll.Controls.Add(lblBalance);

            y += 76;
            pnlNavScroll.Controls.Add(new Label
            {
                Location = new Point(16, y),
                Size = new Size(188, 1),
                BackColor = Color.FromArgb(51, 65, 85)
            });

            y += 14;
            var homeBtn = NavButton("🏠 Главная", y);
            homeBtn.Click += (s, e) =>
            {
                var mainForm = this.Owner as MainForm;
                if (mainForm != null)
                    mainForm.CloseEmbeddedForm();
            };
            pnlNavScroll.Controls.Add(homeBtn);
            _navButtons.Add(homeBtn);
            y += 45;

            btnCart = NavButton("🛒 Корзина", y); y += 45;
            btnFavorites = NavButton("❤ Избранное", y); y += 45;
            btnProfile = NavButton("👤 Профиль", y); y += 45;
            btnOrders = NavButton("📦 Заказы", y); y += 45;

            btnCart.Click += (s, e) => ShowPanel(cartPanel);
            btnFavorites.Click += (s, e) => ShowPanel(favoritesPanel);
            btnProfile.Click += (s, e) => ShowPanel(profilePanel);
            btnOrders.Click += (s, e) => ShowPanel(ordersPanel);

            pnlNavScroll.Controls.Add(btnCart);
            pnlNavScroll.Controls.Add(btnFavorites);
            pnlNavScroll.Controls.Add(btnProfile);
            pnlNavScroll.Controls.Add(btnOrders);
            _navButtons.Add(btnCart);
            _navButtons.Add(btnFavorites);
            _navButtons.Add(btnProfile);
            _navButtons.Add(btnOrders);

            if (user != null)
            {
                y += 6;
                pnlNavScroll.Controls.Add(new Label
                {
                    Location = new Point(16, y),
                    Size = new Size(188, 1),
                    BackColor = Color.FromArgb(51, 65, 85)
                });

                y += 14;
                var role = user.Role;
                if (role == Role.Admin)
                {
                    AddRoleNavButton(pnlNavScroll, "👥 Управление пользователями", y, OpenAdminPanel); y += 45;
                    AddRoleNavButton(pnlNavScroll, "📝 Модерация товаров", y, OpenModeration); y += 45;
                    AddRoleNavButton(pnlNavScroll, "📊 Статистика", y, OpenStatistics); y += 45;
                    AddRoleNavButton(pnlNavScroll, "🖥 Консоль", y, OpenConsole); y += 45;
                    AddRoleNavButton(pnlNavScroll, "➕ Создать товар", y, OpenCreateProductForm);
                }
                else if (role == Role.Moderator)
                {
                    AddRoleNavButton(pnlNavScroll, "📝 Модерация", y, OpenModeration); y += 45;
                    AddRoleNavButton(pnlNavScroll, "📋 Список жалоб", y, OpenComplaints);
                }
                else if (role == Role.Seller)
                {
                    AddRoleNavButton(pnlNavScroll, "📦 Мои товары", y, ShowMyProducts); y += 45;
                    AddRoleNavButton(pnlNavScroll, "➕ Создать товар", y, OpenCreateProductForm); y += 45;
                    AddRoleNavButton(pnlNavScroll, "📊 Статистика", y, OpenStatistics);
                }
            }
        }

        private void AddRoleNavButton(Panel parent, string text, int y, Action onClick)
        {
            var btn = NavButton(text, y);
            btn.Click += (s, e) =>
            {
                _activeNavButton = btn;
                HighlightNav(btn);
                onClick();
            };
            parent.Controls.Add(btn);
            _navButtons.Add(btn);
        }

        private Button NavButton(string text, int y)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(0, y),
                Size = new Size(220, 40),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.FromArgb(203, 213, 225),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var b = (Button)s!;
                using var bgBrush = new SolidBrush(b.BackColor);
                e.Graphics.FillRectangle(bgBrush, b.ClientRectangle);

                if (b == _activeNavButton)
                {
                    using var accentBrush = new SolidBrush(Color.FromArgb(37, 99, 235));
                    e.Graphics.FillRectangle(accentBrush, 0, 0, 3, b.Height);
                }

                var txt = b.Text;
                var sz = TextRenderer.MeasureText(txt, b.Font);
                var tx = 22;
                if (txt.Length > 0 && txt[0] >= 0x1F300)
                    tx = 16;
                TextRenderer.DrawText(e.Graphics, txt, b.Font,
                    new Point(tx, (b.Height - sz.Height) / 2),
                    b.ForeColor, TextFormatFlags.Default);
            };
            btn.MouseEnter += (s, e) =>
            {
                if (btn != _activeNavButton)
                    btn.BackColor = Color.FromArgb(30, 41, 59);
                btn.Invalidate();
            };
            btn.MouseLeave += (s, e) =>
            {
                if (btn != _activeNavButton)
                    btn.BackColor = Color.FromArgb(15, 23, 42);
                btn.Invalidate();
            };
            return btn;
        }

        private void BuildContent()
        {
            pnlContent = new Panel
            {
                Location = new Point(220, 0),
                Size = new Size(ClientSize.Width - 220, ClientSize.Height),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(245, 247, 250)
            };
            Resize += (s, e) =>
                pnlContent.Size = new Size(ClientSize.Width - 220, ClientSize.Height);

            cartPanel = new CartPanel(_authService, _context, _orderService) { Dock = DockStyle.Fill, Visible = false };
            cartPanel.ProductClicked = (product) => ProductClicked?.Invoke(product);
            favoritesPanel = new FavoritesPanel(_authService, _context) { Dock = DockStyle.Fill, Visible = false };
            favoritesPanel.ProductClicked = (product) => ProductClicked?.Invoke(product);
            profilePanel = new ProfilePanel(_authService, _context) { Dock = DockStyle.Fill, Visible = false };
            statisticPanel = new StatisticPanel(_context, _authService) { Dock = DockStyle.Fill, Visible = false };
            ordersPanel = new OrdersPanel(_authService, _context, _productService, _orderService) { Dock = DockStyle.Fill, Visible = false };
            ordersPanel.ProductClicked = (product) => ProductClicked?.Invoke(product);
            ordersPanel.OrderProductClicked = (order) => OrderClicked?.Invoke(order);
            createProductPanel = new CreateProductPanel(_authService, _productService, _context) { Dock = DockStyle.Fill, Visible = false };

            pnlContent.Controls.AddRange(new Control[]
            {
                cartPanel,
                favoritesPanel,
                profilePanel,
                statisticPanel,
                ordersPanel,
                createProductPanel
            });
        }

        private void ShowPanel(Panel panel)
        {
            if (panel == null) return;

            foreach (Control c in pnlContent.Controls)
            {
                c.Visible = false;
            }

            ResetAllButtons();

            panel.Visible = true;
            panel.BringToFront();

            if (panel == cartPanel)
                _activeNavButton = btnCart;
            else if (panel == favoritesPanel)
                _activeNavButton = btnFavorites;
            else if (panel == profilePanel)
                _activeNavButton = btnProfile;
            else if (panel == ordersPanel)
                _activeNavButton = btnOrders;
            else if (panel == createProductPanel)
                _activeNavButton = null;

            if (_activeNavButton != null)
                HighlightNav(_activeNavButton);

            if (panel is IRefreshable r)
                r.Refresh();

            pnlNav.Invalidate();
            pnlNav.Update();
        }

        private void ResetAllButtons()
        {
            foreach (var b in _navButtons)
            {
                if (b != null && b != btnLogout)
                {
                    b.BackColor = Color.FromArgb(15, 23, 42);
                    b.ForeColor = Color.FromArgb(203, 213, 225);
                    b.Invalidate();
                }
            }
        }

        private void HighlightNav(Button btn)
        {
            if (btn == null) return;
            _activeNavButton = btn;
            foreach (var b in _navButtons)
            {
                if (b == btn)
                {
                    b.BackColor = Color.FromArgb(30, 41, 59);
                    b.ForeColor = Color.White;
                }
                else
                {
                    b.BackColor = Color.FromArgb(15, 23, 42);
                    b.ForeColor = Color.FromArgb(203, 213, 225);
                }
                b.Invalidate();
            }
        }

        private void BtnLogout_Click(object? sender, EventArgs e)   
        {
            var main = (MainForm)Owner;
            main.Logout();
        }

        private void OpenAdminPanel()
        {
            var user = _authService.RequireUser();
            var form = new AdminPanelForm(_context, user);
            ShowInnerForm(form);
        }

        private void OpenModeration()
        {
            var form = new ModerationForm(_authService, _productService);
            form.OnProductClick = (product, ps) => ProductClickedForModeration?.Invoke(product, ps);
            ShowInnerForm(form);
        }

        private void OpenStatistics()
        {
            ShowPanel(statisticPanel);
            statisticPanel.Refresh();
        }

        private void OpenConsole()
        {
            var user = _authService.RequireUser();
            var form = new AdminConsoleForm(user);
            ShowInnerForm(form);
            form.Start();
        }

        private void OpenCreateProductForm()
        {
            ShowPanel(createProductPanel);
        }

        private void OpenComplaints()
        {
            MessageBox.Show("Список жалоб", "Модерация");
        }

        private void ShowMyProducts()
        {
            try
            {
                var user = _authService.RequireUser();
                var productRepo = new ProductRepository(_context);
                var products = productRepo.GetAll().Where(p => p.SellerId == user.Id).ToList();

                if (products.Count == 0)
                {
                    MessageBox.Show("У вас нет товаров", "Мои товары");
                    return;
                }

                var text = string.Join(Environment.NewLine,
                    products.Select(p => $"{p.Name} - {p.Price} руб. ({(p.IsApproved ? "Одобрен" : "На модерации")})"));
                MessageBox.Show(text, "Мои товары");
            }
            catch (Exception ex)
            {
                ErrorForm.Show(ex.Message, ex);
            }
        }

        public void RefreshData()
        {
            var user = _authService.CurrentUser;
            if (user != null)
            {
                lblUserName.Text = user.Name;
                lblUserRole.Text = $"[{user.Role}]  💰 {user.Balance:N0} ₽";
            }

            ShowPanel(profilePanel);

            if (profilePanel is IRefreshable refreshable)
                refreshable.Refresh();
        }
        public void RefreshModerationList()
        {
            foreach (Control c in pnlContent.Controls)
            {
                if (c is ModerationForm mf)
                {
                    mf.LoadProducts();
                    break;
                }
            }
        }

        public void RefreshOrdersPanel()
        {
            if (ordersPanel != null)
            {
                ordersPanel.Visible = true;
                ordersPanel.Refresh();
            }
        }

        private void ShowInnerForm(Form form)
        {
            if (cartPanel != null) cartPanel.Visible = false;
            if (favoritesPanel != null) favoritesPanel.Visible = false;
            if (profilePanel != null) profilePanel.Visible = false;
            if (ordersPanel != null) ordersPanel.Visible = false;
            if (createProductPanel != null) createProductPanel.Visible = false;

            foreach (Control c in pnlContent.Controls)
            {
                if (c is Form)
                {
                    c.Dispose();
                }
            }

            ResetAllButtons();
            if (_activeNavButton != null)
                HighlightNav(_activeNavButton);

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.Visible = true;

            pnlContent.Controls.Add(form);
            form.BringToFront();
        }
    }
}