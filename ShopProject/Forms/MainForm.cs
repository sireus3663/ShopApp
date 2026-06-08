using System;
using System.Drawing;
using System.Windows.Forms;
using ShopProject.Services;
using ShopProject.Db;

namespace ShopProject.Forms
{
    public class MainForm : Form
    {
        private readonly AuthService _authService;
        private readonly UserService _userService;
        private readonly AppDbContext _context;
        private Panel pnlNav;
        private Button btnCart, btnFavorites, btnProfile, btnLogout;
        private Label lblAppName, lblUserName, lblUserRole;

        private Panel pnlContent;
        private CartPanel cartPanel;
        private FavoritesPanel favoritesPanel;
        private ProfilePanel profilePanel;

        private Panel _activePanel;

        public MainForm(AuthService authService, UserService userService, AppDbContext context)
        {
            _authService = authService;
            _userService = userService;
            _context = context;
            InitializeComponents();
            ShowPanel(cartPanel);
        }

        private void InitializeComponents()
        {
            this.Text = "ShopProject";
            this.Size = new Size(1100, 700);
            this.MinimumSize = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);

            BuildNav();
            BuildContent();

            this.Controls.Add(pnlNav);
            this.Controls.Add(pnlContent);
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
                Text = "🛍 ShopProject",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 25),
                AutoSize = true
            };

            var user = _authService.currentUser;
            lblUserName = new Label
            {
                Text = user?.Name ?? "Гость",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 75),
                Size = new Size(180, 22)
            };
            lblUserRole = new Label
            {
                Text = user != null ? $"[{user.Role}]  💰 {user.Balance:N0} ₽" : "",
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

            btnCart = NavButton("🛒  Корзина", 150);
            btnFavorites = NavButton("❤  Избранное", 200);
            btnProfile = NavButton("👤  Профиль", 250);

            btnCart.Click += (s, e) => ShowPanel(cartPanel);
            btnFavorites.Click += (s, e) => ShowPanel(favoritesPanel);
            btnProfile.Click += (s, e) => ShowPanel(profilePanel);

            btnLogout = new Button
            {
                Text = "⏻  Выйти из аккаунта",
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

            pnlNav.Controls.AddRange(new Control[]
            {
                lblAppName, lblUserName, lblUserRole, sep,
                btnCart, btnFavorites, btnProfile,
                btnLogout
            });
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
            pnlContent.Size = new Size(this.ClientSize.Width - 220, this.ClientSize.Height);
            this.Resize += (s, e) =>
                pnlContent.Size = new Size(this.ClientSize.Width - 220, this.ClientSize.Height);

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
            _activePanel = panel;

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
            this.Close();
            var login = new LoginForm(_authService, _userService);
            if (login.ShowDialog() == DialogResult.OK)
            {
                var main = new MainForm(_authService, _userService, _context);
                main.Show();
            }
        }
    }

    public interface IRefreshable
    {
        void Refresh();
    }
}