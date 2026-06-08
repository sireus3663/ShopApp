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
        private Panel headerPanel;
        private Panel catalogPanel;
        private Panel rightSidebar;
        private Panel contentPanel;
        private FlowLayoutPanel productsFlow;
        private Label userInfoLabel;
        private Label cartCountLabel;
        private TextBox searchBox;
        private ComboBox categoryFilter;
        private AuthService _authService;
        private AppDbContext _context;
        private ProductRepository _productRepo;
        private CartService _cartService;
        private FavoriteService _favoriteService;
        private bool isSidebarVisible = false;
        private List<Guid> favoritesList = new List<Guid>();
        private User _currentUser;

        public MainForm()
        {
            CheckDatabaseConnection();
            InitializeServices();
            InitializeComponent();
            LoadFavorites();
            LoadProducts();
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
            _currentUser = _authService.currentUser;
            _productRepo = new ProductRepository(_context);
            var cartRepo = new CartRepository(_context);
            _cartService = new CartService(cartRepo, _authService);
            var favoriteRepo = new FavoriteRepository(_context);
            _favoriteService = new FavoriteService(favoriteRepo, _authService);
        }

        private void LoadFavorites()
        {
            try
            {
                if (_authService.currentUser != null)
                {
                    favoritesList = _favoriteService.GetFavorites();
                }
            }
            catch { }
        }

        private void InitializeComponent()
        {
            this.Text = "ShopProject - Маркетплейс";
            this.Size = new Size(1300, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.MinimumSize = new Size(1024, 600);

            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            var logoLabel = new Label
            {
                Text = "ShopProject",
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(30, 15),
                AutoSize = true
            };
            headerPanel.Controls.Add(logoLabel);

            searchBox = new TextBox
            {
                Location = new Point(200, 15),
                Size = new Size(350, 30),
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.FromArgb(220, 220, 220),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle
            };
            searchBox.TextChanged += (s, e) => LoadProducts();
            headerPanel.Controls.Add(searchBox);

            userInfoLabel = new Label
            {
                Text = _authService.currentUser?.Name ?? "Войти",
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 10),
                Location = new Point(900, 20),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            userInfoLabel.Click += UserInfoLabel_Click;
            headerPanel.Controls.Add(userInfoLabel);

            cartCountLabel = new Label
            {
                Text = "Корзина (0)",
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 10),
                Location = new Point(1020, 20),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            cartCountLabel.Click += ToggleRightSidebar;
            headerPanel.Controls.Add(cartCountLabel);

            var favLabel = new Label
            {
                Text = "Избранное",
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 10),
                Location = new Point(960, 20),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            favLabel.Click += (s, e) => ShowFavorites();
            headerPanel.Controls.Add(favLabel);

            var adminLabel = new Label
            {
                Text = "Админ",
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 10),
                Location = new Point(1080, 20),
                AutoSize = true,
                Cursor = Cursors.Hand,
                Visible = _authService.currentUser?.Role == Role.Admin
            };
            adminLabel.Click += (s, e) => OpenAdminPanel();
            headerPanel.Controls.Add(adminLabel);

            var filterPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Color.FromArgb(250, 250, 250),
                Padding = new Padding(15)
            };

            var filterTitle = new Label
            {
                Text = "Фильтры",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                Location = new Point(15, 20),
                AutoSize = true
            };
            filterPanel.Controls.Add(filterTitle);

            var catLabel = new Label
            {
                Text = "Категория:",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(100, 100, 100),
                Location = new Point(15, 60),
                AutoSize = true
            };
            filterPanel.Controls.Add(catLabel);

            categoryFilter = new ComboBox
            {
                Location = new Point(15, 85),
                Size = new Size(180, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(60, 60, 60)
            };
            categoryFilter.Items.Add("Все категории");
            categoryFilter.SelectedIndexChanged += (s, e) => LoadProducts();
            filterPanel.Controls.Add(categoryFilter);
            LoadCategories();

            rightSidebar = new Panel
            {
                Dock = DockStyle.Right,
                Width = 300,
                BackColor = Color.White,
                Visible = false
            };

            var sidebarHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(45, 45, 48)
            };
            var sidebarTitle = new Label
            {
                Text = "Панель",
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 12),
                AutoSize = true
            };
            sidebarHeader.Controls.Add(sidebarTitle);
            rightSidebar.Controls.Add(sidebarHeader);

            catalogPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 240, 240),
                AutoScroll = true
            };

            productsFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                WrapContents = true,
                AutoScroll = true
            };
            catalogPanel.Controls.Add(productsFlow);

            this.Controls.Add(catalogPanel);
            this.Controls.Add(rightSidebar);
            this.Controls.Add(filterPanel);
            this.Controls.Add(headerPanel);

            UpdateCartCount();
            UpdateRightSidebarContent();
        }

        private void LoadCategories()
        {
            try
            {
                var categories = _productRepo.GetAll()
                    .Where(p => p.IsApproved)
                    .Select(p => p.Category)
                    .Distinct()
                    .ToList();
                foreach (var cat in categories)
                {
                    categoryFilter.Items.Add(cat);
                }
            }
            catch { }
        }

        private void LoadProducts()
        {
            productsFlow.Controls.Clear();

            try
            {
                var products = _productRepo.GetAll().Where(p => p.IsApproved).AsQueryable();

                string searchText = searchBox.Text.Trim();
                if (!string.IsNullOrEmpty(searchText))
                {
                    products = products.Where(p => p.Name.ToLower().Contains(searchText.ToLower()));
                }

                if (categoryFilter.SelectedItem != null && categoryFilter.SelectedItem.ToString() != "Все категории")
                {
                    products = products.Where(p => p.Category == categoryFilter.SelectedItem.ToString());
                }

                var productList = products.ToList();

                foreach (var product in productList)
                {
                    var card = CreateProductCard(product);
                    productsFlow.Controls.Add(card);
                }

                if (productList.Count == 0)
                {
                    var emptyLabel = new Label
                    {
                        Text = "Товары не найдены",
                        ForeColor = Color.Gray,
                        Font = new Font("Segoe UI", 14),
                        Location = new Point(400, 200),
                        AutoSize = true
                    };
                    productsFlow.Controls.Add(emptyLabel);
                }
            }
            catch (Exception ex)
            {
                ErrorForm.Show($"Ошибка загрузки товаров: {ex.Message}", ex);
            }
        }

        private Panel CreateProductCard(Product product)
        {
            var card = new Panel
            {
                Width = 220,
                Height = 320,
                BackColor = Color.White,
                Margin = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };

            var imageBox = new Panel
            {
                Width = 218,
                Height = 160,
                BackColor = Color.FromArgb(245, 245, 245)
            };
            var imageLabel = new Label
            {
                Text = "Изображение",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(120, 120, 120),
                Location = new Point(70, 65),
                AutoSize = true
            };
            imageBox.Controls.Add(imageLabel);
            card.Controls.Add(imageBox);

            var nameLabel = new Label
            {
                Text = product.Name.Length > 30 ? product.Name.Substring(0, 27) + "..." : product.Name,
                Location = new Point(10, 170),
                Size = new Size(200, 45),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60)
            };
            card.Controls.Add(nameLabel);

            var priceLabel = new Label
            {
                Text = $"{product.Price:N0} руб.",
                Location = new Point(10, 220),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 80, 80)
            };
            card.Controls.Add(priceLabel);

            var cartButton = new Button
            {
                Text = "В корзину",
                Location = new Point(10, 255),
                Size = new Size(95, 32),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9)
            };
            cartButton.Click += (s, e) => AddToCart(product.Id);
            card.Controls.Add(cartButton);

            bool isFavorite = favoritesList.Contains(product.Id);
            var favButton = new Button
            {
                Text = isFavorite ? "В избранном" : "В избранное",
                Location = new Point(115, 255),
                Size = new Size(95, 32),
                BackColor = Color.White,
                ForeColor = isFavorite ? Color.FromArgb(180, 100, 100) : Color.FromArgb(100, 100, 100),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8)
            };
            favButton.Click += (s, e) => ToggleFavorite(product.Id, favButton);
            card.Controls.Add(favButton);

            return card;
        }

        private void AddToCart(Guid productId)
        {
            if (_authService.currentUser == null)
            {
                MessageBox.Show("Войдите в аккаунт", "Требуется авторизация");
                return;
            }

            try
            {
                _cartService.AddToCart(productId);
                UpdateCartCount();
                MessageBox.Show("Товар добавлен в корзину", "Успешно");
            }
            catch (Exception ex)
            {
                ErrorForm.Show(ex.Message, ex);
            }
        }

        private void ToggleFavorite(Guid productId, Button favButton)
        {
            if (_authService.currentUser == null)
            {
                MessageBox.Show("Войдите в аккаунт", "Требуется авторизация");
                return;
            }

            try
            {
                _favoriteService.ToggleFavorite(productId);
                if (favoritesList.Contains(productId))
                {
                    favoritesList.Remove(productId);
                    favButton.Text = "В избранное";
                    favButton.ForeColor = Color.FromArgb(100, 100, 100);
                }
                else
                {
                    favoritesList.Add(productId);
                    favButton.Text = "В избранном";
                    favButton.ForeColor = Color.FromArgb(180, 100, 100);
                }
            }
            catch (Exception ex)
            {
                ErrorForm.Show(ex.Message, ex);
            }
        }

        private void UpdateCartCount()
        {
            try
            {
                if (_authService.currentUser != null)
                {
                    var cart = _cartService.GetCurrentUserCart();
                    cartCountLabel.Text = $"Корзина ({cart.Sum(c => c.Count)})";
                }
                else
                {
                    cartCountLabel.Text = "Корзина (0)";
                }
            }
            catch { }
        }

        private void ToggleRightSidebar(object sender, EventArgs e)
        {
            isSidebarVisible = !isSidebarVisible;
            rightSidebar.Visible = isSidebarVisible;
            if (isSidebarVisible)
            {
                UpdateRightSidebarContent();
            }
        }

        private void UpdateRightSidebarContent()
        {
            for (int i = rightSidebar.Controls.Count - 1; i >= 1; i--)
            {
                rightSidebar.Controls[i].Dispose();
            }

            if (_authService.currentUser == null)
            {
                var guestLabel = new Label
                {
                    Text = "Войдите в аккаунт\nдля доступа к функциям",
                    ForeColor = Color.Gray,
                    Location = new Point(50, 100),
                    Size = new Size(200, 50),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                rightSidebar.Controls.Add(guestLabel);
                return;
            }

            var role = _authService.currentUser.Role;
            int y = 70;

            if (role == Role.Buyer)
            {
                AddSidebarButton("Мои заказы", y, () => MessageBox.Show("Список заказов", "Заказы"));
                y += 50;
                AddSidebarButton("Профиль", y, () => ShowProfile());
            }
            else if (role == Role.Seller)
            {
                AddSidebarButton("Мои товары", y, () => ShowMyProducts());
                y += 50;
                AddSidebarButton("Создать товар", y, () => OpenCreateProductForm());
                y += 50;
                AddSidebarButton("Статистика продаж", y, () => MessageBox.Show("Статистика продаж", "Статистика"));
            }
            else if (role == Role.Moderator)
            {
                AddSidebarButton("Модерация товаров", y, () => OpenModerationPanel());
                y += 50;
                AddSidebarButton("Список жалоб", y, () => MessageBox.Show("Список жалоб", "Модерация"));
            }
            else if (role == Role.Admin)
            {
                AddSidebarButton("Управление пользователями", y, () => OpenUserManagement());
                y += 50;
                AddSidebarButton("Модерация товаров", y, () => OpenModerationPanel());
                y += 50;
                AddSidebarButton("Статистика", y, () => OpenStatistics());
                y += 50;
                AddSidebarButton("Консоль", y, () => OpenConsole());
                y += 50;
                AddSidebarButton("Создать товар", y, () => OpenCreateProductForm());
            }
        }

        private void AddSidebarButton(string text, int y, Action onClick)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(20, y),
                Size = new Size(260, 40),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft
            };
            btn.Click += (s, e) => onClick();
            rightSidebar.Controls.Add(btn);
        }

        private void ShowMyProducts()
        {
            try
            {
                var products = _productRepo.GetAll()
                    .Where(p => p.SellerId == _authService.currentUser.Id)
                    .ToList();

                if (products.Count == 0)
                {
                    MessageBox.Show("У вас нет товаров", "Мои товары");
                    return;
                }

                var text = string.Join(Environment.NewLine, products.Select(p => $"{p.Name} - {p.Price} руб. ({(p.IsApproved ? "Одобрен" : "На модерации")})"));
                MessageBox.Show(text, "Мои товары");
            }
            catch (Exception ex)
            {
                ErrorForm.Show(ex.Message, ex);
            }
        }

        private void OpenCreateProductForm()
        {
            var form = new CreateProductForm(_context, _authService.currentUser);
            form.ShowDialog();
            LoadProducts();
        }

        private void OpenModerationPanel()
        {
            var form = new ModeratorPanelForm(_context, _authService.currentUser);
            form.ShowDialog();
            LoadProducts();
        }

        private void OpenUserManagement()
        {
            var form = new AdminPanelForm(_context, _authService.currentUser);
            form.ShowDialog();
        }

        private void OpenStatistics()
        {
            MessageBox.Show("Статистика маркетплейса в разработке", "Статистика");
        }

        private void OpenConsole()
        {
            var consoleForm = new Form
            {
                Text = "Консоль",
                Size = new Size(800, 500),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.Black
            };

            var consoleOutput = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                ForeColor = Color.FromArgb(0, 255, 0),
                Font = new Font("Consolas", 10),
                ReadOnly = true
            };

            var inputBox = new TextBox
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };
            inputBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    consoleOutput.AppendText($"> {inputBox.Text}\n");
                    consoleOutput.AppendText($"Выполнено: {inputBox.Text}\n\n");
                    inputBox.Clear();
                }
            };

            consoleForm.Controls.Add(consoleOutput);
            consoleForm.Controls.Add(inputBox);
            consoleForm.ShowDialog();
        }

        private void ShowFavorites()
        {
            productsFlow.Controls.Clear();

            try
            {
                if (_authService.currentUser == null)
                {
                    MessageBox.Show("Войдите в аккаунт", "Требуется авторизация");
                    return;
                }

                var favorites = _favoriteService.GetUserFavorites(_authService.currentUser.Id);
                var products = favorites
                    .Select(f => _productRepo.GetById(f.ProductId))
                    .Where(p => p != null && p.IsApproved)
                    .ToList();

                foreach (var product in products)
                {
                    var card = CreateProductCard(product);
                    productsFlow.Controls.Add(card);
                }

                if (products.Count == 0)
                {
                    var emptyLabel = new Label
                    {
                        Text = "Избранное пусто",
                        ForeColor = Color.Gray,
                        Font = new Font("Segoe UI", 14),
                        Location = new Point(400, 200),
                        AutoSize = true
                    };
                    productsFlow.Controls.Add(emptyLabel);
                }
            }
            catch (Exception ex)
            {
                ErrorForm.Show(ex.Message, ex);
            }
        }

        private void ShowProfile()
        {
            var user = _authService.currentUser;
            MessageBox.Show($"Профиль:" +
                $"\nИмя: {user.Name}" +
                $"\nEmail: {user.Email}" +
                $"\nРоль: {user.Role}" +
                $"\nБаланс: {user.Balance:N0} руб.",
                "Мой профиль");
        }

        private void OpenAdminPanel()
        {
            var adminForm = new AdminPanelForm(_context, _authService.currentUser);
            adminForm.ShowDialog();
        }

        private void UserInfoLabel_Click(object sender, EventArgs e)
        {
            if (_authService.currentUser == null)
            {
                ShowLoginForm();
            }
            else
            {
                var menu = new ContextMenuStrip();
                menu.Items.Add($"Профиль: {_authService.currentUser.Name}", null, (s, ev) => ShowProfile());
                menu.Items.Add($"Баланс: {_authService.currentUser.Balance:N0} руб.", null, null);
                menu.Items.Add("-");
                menu.Items.Add("Выйти", null, (s, ev) => Logout());
                menu.Show(userInfoLabel, new Point(0, userInfoLabel.Height));
            }
        }

        private void ShowLoginForm()
        {
            var loginForm = new Form
            {
                Text = "Вход",
                Size = new Size(350, 220),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(240, 240, 240),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };

            var lblEmail = new Label 
            { 
                Text = "Email:",
                Location = new Point(30, 30),
                Size = new Size(80, 25),
                ForeColor = Color.FromArgb(60, 60, 60) 
            };
            var txtEmail = new TextBox 
            { 
                Location = new Point(120, 30),
                Size = new Size(180, 25),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(60, 60, 60) 
            };

            var lblPassword = new Label 
            { 
                Text = "Пароль:",
                Location = new Point(30, 70),
                Size = new Size(80, 25), 
                ForeColor = Color.FromArgb(60, 60, 60) 
            };
            var txtPassword = new TextBox 
            { 
                Location = new Point(120, 70),
                Size = new Size(180, 25), 
                BackColor = Color.White, 
                ForeColor = Color.FromArgb(60, 60, 60), 
                PasswordChar = '*' 
            };

            var btnOk = new Button 
            { 
                Text = "Войти",
                Location = new Point(120, 120),
                Size = new Size(80, 30), 
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat };
            var btnCancel = new Button 
            { 
                Text = "Отмена",
                Location = new Point(210, 120),
                Size = new Size(80, 30), 
                BackColor = Color.FromArgb(180, 180, 180), 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat 
            };

            btnOk.Click += (s, ev) =>
            {
                try
                {
                    _authService.Login(txtEmail.Text, txtPassword.Text);
                    userInfoLabel.Text = _authService.currentUser.Name;
                    _currentUser = _authService.currentUser;
                    loginForm.Close();
                    LoadFavorites();
                    LoadProducts();
                    UpdateCartCount();
                    UpdateRightSidebarContent();
                }
                catch (Exception ex)
                {
                    ErrorForm.Show(ex.Message, ex);
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

        private void Logout()
        {
            _authService.Logout();
            userInfoLabel.Text = "Войти";
            favoritesList.Clear();
            LoadProducts();
            UpdateCartCount();
            UpdateRightSidebarContent();
        }
    }
}