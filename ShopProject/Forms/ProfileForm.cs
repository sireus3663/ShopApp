using System;
using System.Drawing;
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
        private Panel pnlNav;
        private Button btnCart, btnFavorites, btnProfile, btnLogout, btnUserManagement, btnModeration, btnStatistics, btnConsole, btnCreateProduct, btnMyProducts, btnMyOrders, btnComplaints;
        private Label lblAppName, lblUserName, lblUserRole;
     
        private Panel pnlContent;
        private CartPanel cartPanel;
        private FavoritesPanel favoritesPanel;
        private ProfilePanel profilePanel;
        private Panel searchPanel;
        private TextBox searchBox;
        private Button searchBtn;

        public ProfileForm(AuthService authService, UserService userService, AppDbContext context, ProductService productService)
        {
            _authService = authService;
            _userService = userService;
            _context = context;
            _productService = productService;
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

            lblAppName = new Label
            {
                Text = "🛍 ShopApp",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 25),
                AutoSize = true
            };

            var homeNavButton = NavButton("🏠 Главная", 75);
            homeNavButton.Click += (s, e) =>
            {
                var mainForm = this.Owner as MainForm;
                if (mainForm != null)
                {
                    mainForm.CloseProfilePanel();
                }
                this.Close();
            };
            pnlNav.Controls.Add(homeNavButton);

            var user = _authService.currentUser;
            lblUserName = new Label
            {
                Text = user?.Name ?? "Гость",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 125),
                Size = new Size(180, 22)
            };

            lblUserRole = new Label
            {
                Text = user != null ? $"[{user.Role}]  💰 {user.Balance:N0} ₽" : "",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(20, 148),
                Size = new Size(180, 18)
            };

            var sep = new Label
            {
                Location = new Point(0, 175),
                Size = new Size(220, 1),
                BackColor = Color.FromArgb(51, 65, 85)
            };

            btnCart = NavButton("🛒 Корзина", 200);
            btnFavorites = NavButton("❤ Избранное", 250);
            btnProfile = NavButton("👤 Профиль", 300);

            btnCart.Click += (s, e) => ShowPanel(cartPanel);
            btnFavorites.Click += (s, e) => ShowPanel(favoritesPanel);
            btnProfile.Click += (s, e) => ShowPanel(profilePanel);

            pnlNav.Controls.AddRange(new Control[]
            {
                lblAppName, lblUserName, lblUserRole, sep,
                btnCart, btnFavorites, btnProfile
            });

            if (user != null)
            {
                var roleSep = new Label
                {
                    Location = new Point(0, 345),
                    Size = new Size(220, 1),
                    BackColor = Color.FromArgb(51, 65, 85)
                };
                pnlNav.Controls.Add(roleSep);

                int roleY = 360;
                var role = user.Role;

                if (role == Role.Admin)
                {
                    AddRoleNavButton("👥 Управление пользователями", roleY, OpenAdminPanel); roleY += 45;
                    AddRoleNavButton("📝 Модерация товаров", roleY, OpenModeration); roleY += 45;
                    AddRoleNavButton("📊 Статистика", roleY, OpenStatistics); roleY += 45;
                    AddRoleNavButton("🖥 Консоль", roleY, OpenConsole); roleY += 45;
                    AddRoleNavButton("➕ Создать товар", roleY, OpenCreateProductForm);
                }
                else if (role == Role.Moderator)
                {
                    AddRoleNavButton("📝 Модерация", roleY, OpenModeration); roleY += 45;
                    AddRoleNavButton("📋 Список жалоб", roleY, OpenComplaints);
                }
                else if (role == Role.Seller)
                {
                    AddRoleNavButton("📦 Мои товары", roleY, ShowMyProducts); roleY += 45;
                    AddRoleNavButton("➕ Создать товар", roleY, OpenCreateProductForm); roleY += 45;
                    AddRoleNavButton("📊 Статистика", roleY, OpenStatistics);
                }
            }

            btnLogout = new Button
            {
                Text = "🚪 Выйти из аккаунта",
                Location = new Point(0, 590),
                Size = new Size(220, 45),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(127, 29, 29),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0)
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += BtnLogout_Click;

            pnlNav.Controls.Add(btnLogout);
        }

        private void AddRoleNavButton(string text, int y, Action onClick)
        {
            var btn = NavButton(text, y);
            btn.Click += (s, e) => onClick();
            pnlNav.Controls.Add(btn);
        }

        private Button NavButton(string text, int y)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(0, y),
                Size = new Size(220, 40),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.FromArgb(203, 213, 225),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => { if (btn.BackColor != Color.FromArgb(37, 99, 235)) btn.BackColor = Color.FromArgb(30, 41, 59); };
            btn.MouseLeave += (s, e) => { if (btn.BackColor != Color.FromArgb(37, 99, 235)) btn.BackColor = Color.FromArgb(15, 23, 42); };
            return btn;
        }

        private void BuildContent()
        {
            pnlContent = new Panel
            {
                Location = new Point(220, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(245, 247, 250)
            };
            pnlContent.Size = new Size(ClientSize.Width - 220, ClientSize.Height);
            Resize += (s, e) =>
                pnlContent.Size = new Size(ClientSize.Width - 220, ClientSize.Height);

            searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var searchLabel = new Label
            {
                Text = "Поиск товаров:",
                Location = new Point(10, 18),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 10)
            };
            searchPanel.Controls.Add(searchLabel);

            searchBox = new TextBox
            {
                Location = new Point(115, 15),
                Size = new Size(250, 30),
                Font = new Font("Segoe UI", 11)
            };
            searchPanel.Controls.Add(searchBox);

            searchBtn = new Button
            {
                Text = "Найти",
                Location = new Point(375, 13),
                Size = new Size(80, 32),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            searchBtn.Click += SearchBtn_Click;
            searchPanel.Controls.Add(searchBtn);

            cartPanel = new CartPanel(_authService, _context) { Dock = DockStyle.Fill, Visible = false };
            favoritesPanel = new FavoritesPanel(_authService, _context) { Dock = DockStyle.Fill, Visible = false };
            profilePanel = new ProfilePanel(_authService) { Dock = DockStyle.Fill, Visible = false };

            pnlContent.Controls.AddRange(new Control[]
            {
                cartPanel,
                favoritesPanel,
                profilePanel,
                searchPanel
            });
        }

        private void SearchBtn_Click(object sender, EventArgs e)
        {
            var searchText = searchBox.Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show("Введите текст для поиска", "Поиск");
                return;
            }

            var mainForm = this.Owner as MainForm;
            if (mainForm != null)
            {
                mainForm.CloseProfilePanel();
                mainForm.SetSearchText(searchText);
            }
            this.Close();
        }

        private void ShowPanel(Panel panel)
        {
            foreach (Control c in pnlContent.Controls)
            {
                if (c != searchPanel)
                    c.Visible = false;
            }

            foreach (Control c in pnlNav.Controls)
                if (c is Button b && b != btnLogout)
                    b.BackColor = Color.FromArgb(15, 23, 42);

            panel.Visible = true;

            if (panel == cartPanel) HighlightNav(btnCart);
            if (panel == favoritesPanel) HighlightNav(btnFavorites);
            if (panel == profilePanel) HighlightNav(btnProfile);

            if (panel is IRefreshable r) r.Refresh();
        }

        private void HighlightNav(Button btn)
        {
            btn.BackColor = Color.FromArgb(37, 99, 235);
            btn.ForeColor = Color.White;
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            _authService.Logout();
            DialogResult = DialogResult.Abort;
            Close();
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
            ShowInnerForm(form);
        }

        private void OpenStatistics()
        {
            var form = new Form
            {
                Text = "Статистика",
                Size = new Size(800, 500),
                BackColor = Color.White
            };
            var label = new Label
            {
                Text = "Статистика маркетплейса в разработке",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 14)
            };
            form.Controls.Add(label);
            ShowInnerForm(form);
        }

        private void OpenConsole()
        {
            var user = _authService.RequireUser();
            var form = new AdminConsoleForm(user);
            ShowInnerForm(form);
        }

        private void OpenCreateProductForm()
        {
            var form = new CreateProductForm(_authService, _productService, _context);
            ShowInnerForm(form);
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
            var user = _authService.currentUser;
            if (user != null)
            {
                lblUserName.Text = user.Name;
                lblUserRole.Text = $"[{user.Role}]  💰 {user.Balance:N0} ₽";
            }

            ShowPanel(profilePanel);

            if (profilePanel is IRefreshable refreshable)
                refreshable.Refresh();
        }
        private void ShowInnerForm(Form form)
        {
            if (cartPanel != null) cartPanel.Visible = false;
            if (favoritesPanel != null) favoritesPanel.Visible = false;
            if (profilePanel != null) profilePanel.Visible = false;

            foreach (Control c in pnlContent.Controls)
            {
                if (c is Form && c != searchPanel)
                {
                    c.Dispose();
                }
            }

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.Visible = true;

            pnlContent.Controls.Add(form);
            form.BringToFront();
        }
    }
}