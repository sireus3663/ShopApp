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
        private Button btnCart, btnFavorites, btnProfile, btnLogout;
        private Label lblAppName, lblUserName, lblUserRole;

        private Panel pnlContent;
        private CartPanel cartPanel;
        private FavoritesPanel favoritesPanel;
        private ProfilePanel profilePanel;

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
            Text = "ShopProject — \u041B\u0438\u0447\u043D\u044B\u0439 \u043A\u0430\u0431\u0438\u043D\u0435\u0442";
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
                Text = "\uD83D\uDECD ShopProject",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 25),
                AutoSize = true
            };

            var user = _authService.currentUser;
            lblUserName = new Label
            {
                Text = user?.Name ?? "\u0413\u043E\u0441\u0442\u044C",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 75),
                Size = new Size(180, 22)
            };
            lblUserRole = new Label
            {
                Text = user != null ? $"[{user.Role}]  \uD83D\uDCB0 {user.Balance:N0} \u20BD" : "",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(20, 98),
                Size = new Size(180, 18)
            };

            var sep = new Label
            {
                Location = new Point(0, 125),
                Size = new Size(220, 1),
                BackColor = Color.FromArgb(51, 65, 85)
            };

            btnCart = NavButton("\uD83D\uDED2  \u041A\u043E\u0440\u0437\u0438\u043D\u0430", 150);
            btnFavorites = NavButton("\u2764  \u0418\u0437\u0431\u0440\u0430\u043D\u043D\u043E\u0435", 200);
            btnProfile = NavButton("\uD83D\uDC64  \u041F\u0440\u043E\u0444\u0438\u043B\u044C", 250);

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
                    Location = new Point(0, 295),
                    Size = new Size(220, 1),
                    BackColor = Color.FromArgb(51, 65, 85)
                };
                pnlNav.Controls.Add(roleSep);

                int roleY = 310;
                var role = user.Role;

                if (role == Role.Admin)
                {
                    AddRoleNavButton("\uD83D\uDC65  \u0423\u043F\u0440\u0430\u0432\u043B\u0435\u043D\u0438\u0435 \u043F\u043E\u043B\u044C\u0437\u043E\u0432\u0430\u0442\u0435\u043B\u044F\u043C\u0438", roleY, OpenAdminPanel); roleY += 52;
                    AddRoleNavButton("\uD83D\uDCDD  \u041C\u043E\u0434\u0435\u0440\u0430\u0446\u0438\u044F \u0442\u043E\u0432\u0430\u0440\u043E\u0432", roleY, OpenModeration); roleY += 52;
                    AddRoleNavButton("\uD83D\uDCCA  \u0421\u0442\u0430\u0442\u0438\u0441\u0442\u0438\u043A\u0430", roleY, OpenStatistics); roleY += 52;
                    AddRoleNavButton("\uD83D\uDDA5  \u041A\u043E\u043D\u0441\u043E\u043B\u044C", roleY, OpenConsole); roleY += 52;
                    AddRoleNavButton("\u2795  \u0421\u043E\u0437\u0434\u0430\u0442\u044C \u0442\u043E\u0432\u0430\u0440", roleY, OpenCreateProductForm);
                }
                else if (role == Role.Moderator)
                {
                    AddRoleNavButton("\uD83D\uDCDD  \u041C\u043E\u0434\u0435\u0440\u0430\u0446\u0438\u044F", roleY, OpenModeration); roleY += 52;
                    AddRoleNavButton("\uD83D\uDCCB  \u0421\u043F\u0438\u0441\u043E\u043A \u0436\u0430\u043B\u043E\u0431", roleY, OpenComplaints);
                }
                else if (role == Role.Seller)
                {
                    AddRoleNavButton("\uD83D\uDCE6  \u041C\u043E\u0438 \u0442\u043E\u0432\u0430\u0440\u044B", roleY, ShowMyProducts); roleY += 52;
                    AddRoleNavButton("\u2795  \u0421\u043E\u0437\u0434\u0430\u0442\u044C \u0442\u043E\u0432\u0430\u0440", roleY, OpenCreateProductForm); roleY += 52;
                    AddRoleNavButton("\uD83D\uDCCA  \u0421\u0442\u0430\u0442\u0438\u0441\u0442\u0438\u043A\u0430", roleY, OpenStatistics);
                }
            }

            btnLogout = new Button
            {
                Text = "\u23FB  \u0412\u044B\u0439\u0442\u0438 \u0438\u0437 \u0430\u043A\u043A\u0430\u0443\u043D\u0442\u0430",
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
                Size = new Size(220, 42),
                Font = new Font("Segoe UI", 10),
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

            cartPanel = new CartPanel(_authService, _context) { Dock = DockStyle.Fill, Visible = false };
            favoritesPanel = new FavoritesPanel(_authService, _context) { Dock = DockStyle.Fill, Visible = false };
            profilePanel = new ProfilePanel(_authService) { Dock = DockStyle.Fill, Visible = false };

            pnlContent.Controls.AddRange(new Control[]
            {
                cartPanel, favoritesPanel, profilePanel
            });
        }

        private void ShowPanel(Panel panel)
        {
            foreach (Control c in pnlContent.Controls)
                c.Visible = false;

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

        private async void BtnLogout_Click(object sender, EventArgs e)
        {
            await _authService.Logout();
            DialogResult = DialogResult.Abort;
            Close();
        }

        private void OpenAdminPanel()
        {
            var user = _authService.RequireUser();
            var form = new AdminPanelForm(_context, user);
            form.ShowDialog();
        }

        private void OpenModeration()
        {
            var form = new ModerationForm(_authService, _productService);
            form.ShowDialog();
        }

        private void OpenStatistics()
        {
            MessageBox.Show("Статистика маркетплейса в разработке", "Статистика");
        }

        private void OpenConsole()
        {
            var user = _authService.RequireUser();
            var form = new AdminConsoleForm(user);
            form.ShowDialog();
        }

        private void OpenCreateProductForm()
        {
            var form = new CreateProductForm(_authService, _productService, _context);
            form.ShowDialog();
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
    }
}
