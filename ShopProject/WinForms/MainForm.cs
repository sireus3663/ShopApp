using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ShopProject.Db;
using ShopProject.Services;
using ShopProject.Models;

namespace ShopProject.WinForms
{
    public class MainForm : Form
    {
        private Panel rightPanel;
        private Panel contentPanel;
        private Label userInfoLabel;
        private AuthService _authService;
        private AppDbContext _context;

        public MainForm()
        {
            CheckDatabaseConnection();
            InitializeServices();
            InitializeComponent();
            CreateRightPanel();
            UpdateRightPanelByRole();
        }

        private void CheckDatabaseConnection()
        {
            var logger = new LoggerService();
            var configService = new AppConfigService();
            var startup = new StartupService(logger, configService);

            _context = startup.TryConnect();

            if (_context == null)
            {
                var dbForm = new DbConnectionForm();
                if (dbForm.ShowDialog() == DialogResult.OK)
                {
                    _context = dbForm.ConnectedContext;
                }
                else
                {
                    MessageBox.Show("Невозможно продолжить без подключения к БД", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
            }
        }

        private void InitializeServices()
        {
            var configService = new AppConfigService();
            _authService = new AuthService(_context, configService);
        }

        private void InitializeComponent()
        {
            Text = "ShopProject Market";
            Size = new Size(1000, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(32, 32, 32);

            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            rightPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 250,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(25, 25, 28)
            };

            userInfoLabel = new Label
            {
                Text = "Не авторизован",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Location = new Point(10, 15),
                AutoSize = true
            };

            var loginButton = new Button
            {
                Text = "Войти",
                Location = new Point(200, 10),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            loginButton.Click += LoginButton_Click;

            topPanel.Controls.Add(userInfoLabel);
            topPanel.Controls.Add(loginButton);

            Controls.Add(contentPanel);
            Controls.Add(rightPanel);
            Controls.Add(topPanel);

            ShowWelcomeMessage();
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            var loginForm = new Form
            {
                Text = "Вход",
                Size = new Size(350, 250),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(32, 32, 32),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };

            var lblEmail = new Label { Text = "Email:", Location = new Point(30, 30), Size = new Size(80, 25), ForeColor = Color.White };
            var txtEmail = new TextBox { Location = new Point(120, 30), Size = new Size(180, 25), BackColor = Color.FromArgb(64, 64, 64), ForeColor = Color.White };

            var lblPassword = new Label { Text = "Пароль:", Location = new Point(30, 70), Size = new Size(80, 25), ForeColor = Color.White };
            var txtPassword = new TextBox { Location = new Point(120, 70), Size = new Size(180, 25), BackColor = Color.FromArgb(64, 64, 64), ForeColor = Color.White, PasswordChar = '*' };

            var btnOk = new Button { Text = "Войти", Location = new Point(120, 120), Size = new Size(80, 30), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var btnCancel = new Button { Text = "Отмена", Location = new Point(210, 120), Size = new Size(80, 30), BackColor = Color.FromArgb(64, 64, 64), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            btnOk.Click += (s, ev) =>
            {
                try
                {
                    _authService.Login(txtEmail.Text, txtPassword.Text);
                    userInfoLabel.Text = _authService.currentUser?.Name ?? "Не авторизован";
                    loginForm.DialogResult = DialogResult.OK;
                    loginForm.Close();
                    UpdateRightPanelByRole();
                    ShowWelcomeMessage();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnCancel.Click += (s, ev) => loginForm.Close();

            loginForm.Controls.Add(lblEmail);
            loginForm.Controls.Add(txtEmail);
            loginForm.Controls.Add(lblPassword);
            loginForm.Controls.Add(txtPassword);
            loginForm.Controls.Add(btnOk);
            loginForm.Controls.Add(btnCancel);

            loginForm.ShowDialog();
        }

        private void ShowWelcomeMessage()
        {
            contentPanel.Controls.Clear();

            string userName = _authService.currentUser?.Name ?? "Гость";
            string roleName = _authService.currentUser?.Role.ToString() ?? "";

            var welcomeLabel = new Label
            {
                Text = $"Добро пожаловать, {userName}!",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(300, 200),
                Size = new Size(500, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var roleLabel = new Label
            {
                Text = roleName,
                ForeColor = Color.Cyan,
                Font = new Font("Segoe UI", 12),
                Location = new Point(350, 250),
                Size = new Size(400, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            contentPanel.Controls.Add(welcomeLabel);
            contentPanel.Controls.Add(roleLabel);
        }

        private void CreateRightPanel()
        {
            int y = 20;

            var titleLabel = new Label
            {
                Text = "Меню",
                ForeColor = Color.Cyan,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(15, y),
                Size = new Size(220, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            rightPanel.Controls.Add(titleLabel);
            y += 40;

            var productsButton = new Button
            {
                Text = "Товары",
                Location = new Point(15, y),
                Size = new Size(220, 35),
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            productsButton.Click += (s, e) => ShowProducts();
            rightPanel.Controls.Add(productsButton);
        }

        private void UpdateRightPanelByRole()
        {
            if (rightPanel == null) return;
            var title = rightPanel.Controls.OfType<Label>().FirstOrDefault();
            var productsBtn = rightPanel.Controls.OfType<Button>().FirstOrDefault(b => b.Text == "Товары");
            rightPanel.Controls.Clear();
            if (title != null) rightPanel.Controls.Add(title);
            if (productsBtn != null) rightPanel.Controls.Add(productsBtn);
            if (_authService.currentUser == null)
            {
                userInfoLabel.Text = "Не авторизован";
                return;
            }

            var userRole = _authService.currentUser.Role;
            int y = rightPanel.Controls.Count * 45 + 20;

            if (userRole == Role.Buyer)
            {
                var cartButton = new Button
                {
                    Text = "Корзина",
                    Location = new Point(15, y),
                    Size = new Size(220, 35),
                    BackColor = Color.FromArgb(0, 120, 215),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                cartButton.Click += (s, e) => MessageBox.Show("Открытие корзины (placeholder)", "Корзина");
                rightPanel.Controls.Add(cartButton);
            }
            else if (userRole == Role.Seller)
            {
                var myProductsButton = new Button
                {
                    Text = "Мои товары",
                    Location = new Point(15, y),
                    Size = new Size(220, 35),
                    BackColor = Color.FromArgb(0, 120, 215),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                myProductsButton.Click += (s, e) =>
                {
                    var sellerForm = new SellerPanelForm(_context, _authService.currentUser);
                    sellerForm.ShowDialog();
                };
                rightPanel.Controls.Add(myProductsButton);

                var createButton = new Button
                {
                    Text = "Создать товар",
                    Location = new Point(15, y + 45),
                    Size = new Size(220, 35),
                    BackColor = Color.FromArgb(0, 100, 180),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                createButton.Click += (s, e) =>
                {
                    var createForm = new CreateProductForm(_context, _authService.currentUser);
                    createForm.ShowDialog();
                };
                rightPanel.Controls.Add(createButton);
            }
            else if (userRole == Role.Moderator)
            {
                var moderateButton = new Button
                {
                    Text = "Модерация",
                    Location = new Point(15, y),
                    Size = new Size(220, 35),
                    BackColor = Color.FromArgb(0, 120, 215),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                moderateButton.Click += (s, e) =>
                {
                    var modForm = new ModeratorPanelForm(_context, _authService.currentUser);
                    modForm.ShowDialog();
                };
                rightPanel.Controls.Add(moderateButton);
            }
            else if (userRole == Role.Admin)
            {
                var adminButton = new Button
                {
                    Text = "Админ панель",
                    Location = new Point(15, y),
                    Size = new Size(220, 35),
                    BackColor = Color.FromArgb(200, 50, 50),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                adminButton.Click += (s, e) =>
                {
                    var adminForm = new AdminPanelForm(_context, _authService.currentUser);
                    adminForm.ShowDialog();
                };
                rightPanel.Controls.Add(adminButton);
            }

            // Кнопка выхода
            if (!rightPanel.Controls.OfType<Button>().Any(b => b.Text == "Выйти"))
            {
                var logoutButton = new Button
                {
                    Text = "Выйти",
                    Location = new Point(15, rightPanel.Controls.Count * 45 + 20),
                    Size = new Size(220, 35),
                    BackColor = Color.FromArgb(200, 50, 50),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                logoutButton.Click += (s, e) =>
                {
                    _authService.Logout();
                    userInfoLabel.Text = "Не авторизован";
                    UpdateRightPanelByRole();
                    ShowWelcomeMessage();
                };
                rightPanel.Controls.Add(logoutButton);
            }

            userInfoLabel.Text = _authService.currentUser?.Name ?? "Не авторизован";
        }

        private void ShowProducts()
        {
            contentPanel.Controls.Clear();

            var titleLabel = new Label
            {
                Text = "Список товаров",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(350, 20),
                Size = new Size(300, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(titleLabel);

            var productRepo = new ProductRepository(_context);
            var products = productRepo.GetAll().Where(p => p.IsApproved).ToList();

            int y = 80;
            foreach (var product in products)
            {
                var productPanel = new Panel
                {
                    Location = new Point(50, y),
                    Size = new Size(600, 60),
                    BackColor = Color.FromArgb(45, 45, 48),
                    BorderStyle = BorderStyle.FixedSingle
                };

                var nameLabel = new Label
                {
                    Text = product.Name,
                    Location = new Point(10, 10),
                    Size = new Size(250, 20),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };

                var priceLabel = new Label
                {
                    Text = product.Price + " руб.",
                    Location = new Point(270, 10),
                    Size = new Size(100, 20),
                    ForeColor = Color.LightGreen
                };

                var categoryLabel = new Label
                {
                    Text = product.Category,
                    Location = new Point(380, 10),
                    Size = new Size(150, 20),
                    ForeColor = Color.LightGray
                };

                productPanel.Controls.Add(nameLabel);
                productPanel.Controls.Add(priceLabel);
                productPanel.Controls.Add(categoryLabel);
                contentPanel.Controls.Add(productPanel);

                y += 70;
            }

            if (products.Count == 0)
            {
                var emptyLabel = new Label
                {
                    Text = "Нет доступных товаров",
                    ForeColor = Color.LightGray,
                    Location = new Point(350, 150),
                    Size = new Size(300, 30),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                contentPanel.Controls.Add(emptyLabel);
            }
        }
    }
}