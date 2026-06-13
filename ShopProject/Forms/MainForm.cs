using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;
using ShopProject.Forms.ViewModels;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShopProject.Forms
{
    public class MainForm : Form
    {
        private Panel headerPanel;
        private Panel catalogPanel;
        private FlowLayoutPanel productsFlow;
        private Label cartCountLabel;
        private Label userInfoLabel;
        private TextBox searchBox;
        private ComboBox categoryFilter;
        private TextBox txtPriceFrom, txtPriceTo;
        private AppDbContext _context;
        private bool isSidebarVisible = false;
        private Panel cartSidebar;
        private Panel roleSidebar;
        private Panel profilePanel;
        private MainViewModel _viewModel;
        private bool _isLoading = false;

        private Button searchBtn;
        private Label favLabel;

        private int _currentPage = 1;
        private int _pageSize = 12;
        private int _totalPages = 1;
        private Panel paginationPanel;
        private Label pageInfoLabel;

        private AuthService _authService;
        private ProductRepository _productRepo;
        private CartService _cartService;
        private DiscountService _discountService;
        private UserService _userService;
        private ProductService _productService;

        public MainForm()
        {
            CheckDatabaseConnection();
            InitializeServices();
            InitializeComponent();
            SubscribeEvents();
            LoadData();
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
            _productRepo = new ProductRepository(_context);
            var cartRepo = new CartRepository(_context);
            _cartService = new CartService(cartRepo, _authService);
            var favoriteRepo = new FavoriteRepository(_context);
            var favoriteService = new FavoriteService(favoriteRepo, _authService);

            var discountRepo = new DiscountRepository(_context);
            _discountService = new DiscountService(discountRepo, _authService, _productRepo);
            _userService = new UserService(_context, _authService, new LoggerService());

            _viewModel = new MainViewModel(_authService, _productRepo, _cartService, favoriteService);
            _productService = new ProductService(_productRepo, _authService);
        }

        private void LoadData()
        {
            try
            {
                _viewModel.LoadFavorites();
                LoadCategories();
                _ = LoadProductsAsync();
                UpdateCartCount();
                UpdateRoleSidebarContent();
                UpdateUserLabel();
            }
            catch (Exception ex)
            {
                ErrorForm.Show($"Ошибка загрузки данных: {ex.Message}", ex);
            }
        }

        private void UpdateUserLabel()
        {
            userInfoLabel.Text = _viewModel.IsAuthenticated
                ? $"\U0001f464 {_viewModel.CurrentUser.Name}"
                : "\U0001f464 Вход";
        }

        private void SubscribeEvents()
        {
            searchBtn.Click += async (s, e) => await LoadProductsAsync();
            favLabel.Click += (s, e) => ShowFavorites();
            categoryFilter.SelectedIndexChanged += async (s, e) => await LoadProductsAsync();
        }

        private void InitializeComponent()
        {
            this.Text = "ShopApp";
            this.Size = new Size(1300, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.MinimumSize = new Size(1024, 600);

            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.FromArgb(45, 45, 48),
                Padding = new Padding(10, 0, 10, 0)
            };

            var logoLabel = new Label
            {
                Text = "ShopApp",
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(20, 18),
                AutoSize = true
            };
            headerPanel.Controls.Add(logoLabel);

            var homeButton = new Button
            {
                Text = "Главная",
                Location = new Point(115, 16),
                Size = new Size(80, 32),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            homeButton.Click += async (s, e) =>
            {
                roleSidebar.Visible = false;
                cartSidebar.Visible = false;
                profilePanel.Visible = false;

                searchBox.Text = "";
                categoryFilter.SelectedIndex = 0;
                if (txtPriceFrom != null) txtPriceFrom.Text = "";
                if (txtPriceTo != null) txtPriceTo.Text = "";
                _currentPage = 1;
                try
                {
                    LoadCategories();
                    if (_viewModel.IsAuthenticated)
                    {
                        _viewModel.LoadFavorites();
                    }
                    await LoadProductsAsync();
                    UpdateCartCount();
                }
                catch (Exception ex)
                {
                    ErrorForm.Show($"Ошибка обновления: {ex.Message}", ex);
                }
            };
            headerPanel.Controls.Add(homeButton);

            searchBox = new TextBox
            {
                Location = new Point(210, 16),
                Size = new Size(370, 32),
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.FromArgb(220, 220, 220),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle
            };
            headerPanel.Controls.Add(searchBox);

            searchBtn = new Button
            {
                Text = "Найти",
                Location = new Point(590, 15),
                Size = new Size(70, 34),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            headerPanel.Controls.Add(searchBtn);

            var rightPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 350, 
                Height = 65,
                BackColor = Color.Transparent
            };

            var flowRightPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 18, 20, 0) 
            };

            favLabel = new Label
            {
                Text = "Избранное",
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 10),
                Size = new Size(85, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            favLabel.Click += (s, e) => ShowFavorites();
            flowRightPanel.Controls.Add(favLabel);

            cartCountLabel = new Label
            {
                Text = "Корзина (0)",
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 10),
                Size = new Size(120, 30), 
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                AutoSize = false
            };
            cartCountLabel.Click += ToggleCartSidebar;
            flowRightPanel.Controls.Add(cartCountLabel);

            userInfoLabel = new Label
            {
                Text = "\U0001f464 Вход",
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 10),
                Size = new Size(140, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                Padding = new Padding(3, 0, 0, 0)
            };
            userInfoLabel.Click += UserInfoLabel_Click;
            flowRightPanel.Controls.Add(userInfoLabel);

            rightPanel.Controls.Add(flowRightPanel);
            headerPanel.Controls.Add(rightPanel);

            var filterPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Color.FromArgb(250, 250, 250)
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
            filterPanel.Controls.Add(categoryFilter);

            var priceLabel = new Label
            {
                Text = "Цена (руб.):",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(100, 100, 100),
                Location = new Point(15, 130),
                AutoSize = true
            };
            filterPanel.Controls.Add(priceLabel);

            txtPriceFrom = new TextBox
            {
                Text = "",
                Location = new Point(15, 155),
                Size = new Size(80, 25),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(60, 60, 60),
                BorderStyle = BorderStyle.FixedSingle
            };
            filterPanel.Controls.Add(txtPriceFrom);

            txtPriceTo = new TextBox
            {
                Text = "",
                Location = new Point(105, 155),
                Size = new Size(80, 25),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(60, 60, 60),
                BorderStyle = BorderStyle.FixedSingle
            };
            filterPanel.Controls.Add(txtPriceTo);

            var applyPriceBtn = new Button
            {
                Text = "Применить",
                Location = new Point(15, 190),
                Size = new Size(170, 30),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9)
            };
            applyPriceBtn.Click += async (s, e) =>
            {
                _currentPage = 1;
                await LoadProductsAsync();
            };
            filterPanel.Controls.Add(applyPriceBtn);

            cartSidebar = new Panel
            {
                Dock = DockStyle.Right,
                Width = 350,
                BackColor = Color.White,
                Visible = false
            };

            var cartHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(45, 45, 48)
            };
            var cartTitle = new Label
            {
                Text = "Корзина",
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 12),
                AutoSize = true
            };
            cartHeader.Controls.Add(cartTitle);
            cartSidebar.Controls.Add(cartHeader);

            roleSidebar = new Panel
            {
                Dock = DockStyle.Right,
                Width = 280,
                BackColor = Color.FromArgb(248, 248, 248),
                Visible = false
            };

            var roleHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(45, 45, 48)
            };
            var roleTitle = new Label
            {
                Text = "Панель управления",
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 12),
                AutoSize = true
            };
            roleHeader.Controls.Add(roleTitle);
            roleSidebar.Controls.Add(roleHeader);

            profilePanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 350,
                BackColor = Color.White,
                Visible = false
            };

            var profileHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(45, 45, 48)
            };
            var profileTitle = new Label
            {
                Text = "Мой профиль",
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 12),
                AutoSize = true
            };
            profileHeader.Controls.Add(profileTitle);
            profilePanel.Controls.Add(profileHeader);

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

            paginationPanel = CreatePaginationPanel();

            this.Controls.Add(paginationPanel);
            this.Controls.Add(catalogPanel);
            this.Controls.Add(profilePanel);
            this.Controls.Add(roleSidebar);
            this.Controls.Add(cartSidebar);
            this.Controls.Add(filterPanel);
            this.Controls.Add(headerPanel);
        }

        private void LoadCategories()
        {
            try
            {
                var currentCategory = categoryFilter.SelectedItem?.ToString();

                categoryFilter.Items.Clear();
                categoryFilter.Items.Add("Все категории");

                foreach (var cat in _viewModel.GetCategories())
                {
                    categoryFilter.Items.Add(cat);
                }

                if (!string.IsNullOrEmpty(currentCategory) && categoryFilter.Items.Contains(currentCategory))
                    categoryFilter.SelectedItem = currentCategory;
                else
                    categoryFilter.SelectedIndex = 0;
            }
            catch { }
        }

        private List<Product> FilterByPrice(List<Product> products)
        {
            if (txtPriceFrom != null &&
                !string.IsNullOrWhiteSpace(txtPriceFrom.Text) &&
                decimal.TryParse(txtPriceFrom.Text, out decimal priceFrom) &&
                priceFrom > 0)
            {
                products = products.Where(p => p.Price >= priceFrom).ToList();
            }

            if (txtPriceTo != null &&
                !string.IsNullOrWhiteSpace(txtPriceTo.Text) &&
                decimal.TryParse(txtPriceTo.Text, out decimal priceTo) &&
                priceTo > 0)
            {
                products = products.Where(p => p.Price <= priceTo).ToList();
            }

            return products;
        }

        private async Task RefreshAllData()
        {
            try
            {
                LoadCategories();
                if (_viewModel.IsAuthenticated)
                {
                    _viewModel.LoadFavorites();
                }
                _currentPage = 1;
                await LoadProductsAsync();
                UpdateCartCount();
            }
            catch (Exception ex)
            {
                ErrorForm.Show($"Ошибка обновления: {ex.Message}", ex);
            }
        }

        private Panel CreatePaginationPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 1,
                Padding = new Padding(10, 5, 10, 5)
            };

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));  
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));  
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); 
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F)); 
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));  

            var prevBtn = new Button
            {
                Text = "◀ Назад",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            prevBtn.Click += async (s, e) =>
            {
                if (_currentPage > 1)
                {
                    _currentPage--;
                    await LoadProductsAsync();
                }
            };
            tableLayout.Controls.Add(prevBtn, 0, 0);

            pageInfoLabel = new Label
            {
                Text = "Страница 1 из 1",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(60, 60, 60),
                Font = new Font("Segoe UI", 10)
            };
            tableLayout.Controls.Add(pageInfoLabel, 1, 0);

            var nextBtn = new Button
            {
                Text = "Вперед ▶",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            nextBtn.Click += async (s, e) =>
            {
                if (_currentPage < _totalPages)
                {
                    _currentPage++;
                    await LoadProductsAsync();
                }
            };
            tableLayout.Controls.Add(nextBtn, 2, 0);

            var sizeCombo = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.White
            };
            sizeCombo.Items.AddRange(new object[] { 12, 24, 48, 96 });
            sizeCombo.SelectedItem = _pageSize;
            sizeCombo.SelectedIndexChanged += async (s, e) =>
            {
                _pageSize = (int)sizeCombo.SelectedItem;
                _currentPage = 1;
                await LoadProductsAsync();
            };
            tableLayout.Controls.Add(sizeCombo, 3, 0);

            var perPageLabel = new Label
            {
                Text = "на странице",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(60, 60, 60),
                Font = new Font("Segoe UI", 9)
            };
            tableLayout.Controls.Add(perPageLabel, 4, 0);

            panel.Controls.Add(tableLayout);

            return panel;
        }

        private async Task LoadProductsAsync()
        {
            if (_isLoading) return;
            _isLoading = true;
            productsFlow.Controls.Clear();

            try
            {
                var allProducts = _viewModel.SearchProducts(searchBox.Text);
                allProducts = _viewModel.FilterByCategory(allProducts, categoryFilter.SelectedItem?.ToString());
                allProducts = FilterByPrice(allProducts);

                _totalPages = (int)Math.Ceiling((double)allProducts.Count / _pageSize);
                if (_totalPages == 0) _totalPages = 1;
                if (_currentPage > _totalPages) _currentPage = _totalPages;

                var products = allProducts
                    .Skip((_currentPage - 1) * _pageSize)
                    .Take(_pageSize)
                    .ToList();

                if (pageInfoLabel != null)
                    pageInfoLabel.Text = $"Страница {_currentPage} из {_totalPages}";

                foreach (var product in products)
                {
                    var card = CreateProductCard(product);
                    productsFlow.Controls.Add(card);
                }

                if (products.Count == 0)
                {
                    var emptyLabel = new Label
                    {
                        Text = "Товары не найдены",
                        ForeColor = Color.Gray,
                        Font = new Font("Segoe UI", 14),
                        AutoSize = true
                    };
                    productsFlow.Controls.Add(emptyLabel);
                }
            }
            catch (Exception ex)
            {
                ErrorForm.Show($"Ошибка загрузки товаров: {ex.Message}", ex);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private Panel CreateProductCard(Product product)
        {
            var card = new Panel
            {
                Width = 147,
                Height = 254,
                BackColor = Color.White,
                Margin = new Padding(7),
                BorderStyle = BorderStyle.FixedSingle
            };

            var imageBox = new PictureBox
            {
                Width = 145,
                Height = 107,
                BackColor = Color.FromArgb(245, 245, 245),
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(0, 0)
            };

            if (product.ProductImage != null && product.ProductImage.Length > 0)
            {
                var ms = new MemoryStream(product.ProductImage);
                imageBox.Image = Image.FromStream(ms);
            }
            else
            {
                var placeholder = new Label
                {
                    Text = "Нет фото",
                    Font = new Font("Segoe UI", 8),
                    ForeColor = Color.FromArgb(120, 120, 120),
                    Location = new Point(49, 42),
                    AutoSize = true
                };
                imageBox.Controls.Add(placeholder);
            }
            card.Controls.Add(imageBox);

            EventHandler openDetail = (s, e) =>
            {
                var detailForm = new ProductDetailForm(product, _authService, _context);
                detailForm.ShowDialog();
            };
            card.Click += openDetail;
            imageBox.Click += openDetail;

            var nameLabel = new Label
            {
                Text = product.Name.Length > 20 ? product.Name.Substring(0, 17) + "..." : product.Name,
                Location = new Point(7, 113),
                Size = new Size(133, 30),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60)
            };
            nameLabel.Click += openDetail;
            card.Controls.Add(nameLabel);

            var finalPrice = _discountService.CalculatePrice(product);

            if (finalPrice < product.Price)
            {
                var priceLabel = new Label
                {
                    Text = $"{product.Price:N0} \u20BD",
                    Location = new Point(7, 147),
                    Size = new Size(133, 17),
                    Font = new Font("Segoe UI", 9, FontStyle.Strikeout),
                    ForeColor = Color.Gray
                };
                card.Controls.Add(priceLabel);

                var discountPriceLabel = new Label
                {
                    Text = $"{finalPrice:N0} \u20BD",
                    Location = new Point(7, 164),
                    Size = new Size(133, 17),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(200, 50, 50)
                };
                discountPriceLabel.Click += openDetail;
                card.Controls.Add(discountPriceLabel);
            }
            else
            {
                var priceLabel = new Label
                {
                    Text = $"{product.Price:N0} \u20BD",
                    Location = new Point(7, 150),
                    Size = new Size(133, 20),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(80, 80, 80)
                };
                priceLabel.Click += openDetail;
                card.Controls.Add(priceLabel);
            }

            var cartButton = new Button
            {
                Text = "\uD83D\uDED2",
                Location = new Point(7, 193),
                Size = new Size(63, 21),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8)
            };
            cartButton.Click += async (s, e) =>
            {
                if (!_viewModel.IsAuthenticated)
                {
                    ShowLoginForm();
                    return;
                }
                await AddToCartAsync(product.Id);
            };
            card.Controls.Add(cartButton);

            bool isFavorite = _viewModel.IsFavorite(product.Id);
            var favButton = new Button
            {
                Text = isFavorite ? "\u2605" : "\u2606",
                Location = new Point(77, 193),
                Size = new Size(63, 21),
                BackColor = Color.White,
                ForeColor = isFavorite ? Color.FromArgb(200, 200, 50) : Color.FromArgb(150, 150, 150),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9)
            };
            favButton.Click += async (s, e) =>
            {
                if (!_viewModel.IsAuthenticated)
                {
                    ShowLoginForm();
                    return;
                }
                await ToggleFavoriteAsync(product.Id, favButton);
            };
            card.Controls.Add(favButton);

            cartButton.BackColor = Color.FromArgb(80, 80, 85);
            favButton.BackColor = card.BackColor;

            return card;
        }

        private async Task AddToCartAsync(Guid productId)
        {
            if (!_viewModel.IsAuthenticated)
            {
                MessageBox.Show("Войдите в аккаунт", "Требуется авторизация");
                return;
            }

            try
            {
                _viewModel.AddToCart(productId);
                UpdateCartCount();
                MessageBox.Show("Товар добавлен в корзину", "Успешно");
            }
            catch (Exception ex)
            {
                ErrorForm.Show(ex.Message, ex);
            }
        }

        private async Task ToggleFavoriteAsync(Guid productId, Button favButton)
        {
            if (!_viewModel.IsAuthenticated)
            {
                MessageBox.Show("Войдите в аккаунт", "Требуется авторизация");
                return;
            }

            try
            {
                _viewModel.ToggleFavorite(productId);
                bool isFavorite = _viewModel.IsFavorite(productId);
                favButton.Text = isFavorite ? "\u2605" : "\u2606";
                favButton.ForeColor = isFavorite ? Color.FromArgb(200, 200, 50) : Color.FromArgb(150, 150, 150);
            }
            catch (Exception ex)
            {
                ErrorForm.Show(ex.Message, ex);
            }
        }

        private void UpdateCartCount()
        {
            cartCountLabel.Text = $"Корзина ({_viewModel.GetCartItemsCount()})";
        }

        private async void ToggleCartSidebar(object sender, EventArgs e)
        {
            roleSidebar.Visible = false;
            profilePanel.Visible = false;
            isSidebarVisible = !isSidebarVisible;
            cartSidebar.Visible = isSidebarVisible;
            if (isSidebarVisible)
            {
                await LoadCartContentAsync();
            }
        }

        private async Task LoadCartContentAsync()
        {
            for (int i = cartSidebar.Controls.Count - 1; i >= 1; i--)
            {
                cartSidebar.Controls[i].Dispose();
            }

            if (!_viewModel.IsAuthenticated)
            {
                var emptyLabel = new Label
                {
                    Text = "Войдите в аккаунт\nчтобы увидеть корзину",
                    ForeColor = Color.Gray,
                    Location = new Point(80, 150),
                    Size = new Size(200, 50),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                cartSidebar.Controls.Add(emptyLabel);
                return;
            }

            var cartItems = _viewModel.GetCurrentUserCart();
            if (cartItems.Count == 0)
            {
                var emptyLabel = new Label
                {
                    Text = "Корзина пуста",
                    ForeColor = Color.Gray,
                    Location = new Point(120, 150),
                    Size = new Size(150, 30),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                cartSidebar.Controls.Add(emptyLabel);
                return;
            }

            int y = 70;
            decimal total = 0;

            foreach (var item in cartItems)
            {
                var product = _viewModel.GetProduct(item.ProductId);
                if (product == null) continue;

                decimal itemTotal = product.Price * item.Count;
                total += itemTotal;

                var itemPanel = new Panel
                {
                    Location = new Point(15, y),
                    Size = new Size(320, 65),
                    BackColor = Color.FromArgb(250, 250, 250),
                    BorderStyle = BorderStyle.FixedSingle
                };

                var nameLabel = new Label
                {
                    Text = product.Name.Length > 20 ? product.Name.Substring(0, 17) + "..." : product.Name,
                    Location = new Point(10, 10),
                    Size = new Size(180, 20),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(60, 60, 60)
                };
                itemPanel.Controls.Add(nameLabel);

                var priceLabel = new Label
                {
                    Text = $"{product.Price:N0} руб. x {item.Count} = {itemTotal:N0} руб.",
                    Location = new Point(10, 35),
                    Size = new Size(180, 20),
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.FromArgb(100, 100, 100)
                };
                itemPanel.Controls.Add(priceLabel);

                var removeBtn = new Button
                {
                    Text = "Удалить",
                    Location = new Point(220, 18),
                    Size = new Size(80, 30),
                    BackColor = Color.FromArgb(200, 100, 100),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9)
                };
                removeBtn.Click += async (s, e) => await RemoveFromCartAsync(item.ProductId);
                itemPanel.Controls.Add(removeBtn);

                cartSidebar.Controls.Add(itemPanel);
                y += 75;
            }

            var totalLabel = new Label
            {
                Text = $"Итого: {_viewModel.GetCartTotalPrice():N0} руб.",
                Location = new Point(20, y + 10),
                Size = new Size(200, 30),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60)
            };
            cartSidebar.Controls.Add(totalLabel);

            var buyButton = new Button
            {
                Text = "Оформить заказ",
                Location = new Point(20, y + 50),
                Size = new Size(310, 45),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            buyButton.Click += async (s, e) => await BuyCartAsync();
            cartSidebar.Controls.Add(buyButton);
        }

        private async Task BuyCartAsync()
        {
            if (!_viewModel.IsAuthenticated)
            {
                MessageBox.Show("Войдите в аккаунт", "Требуется авторизация");
                return;
            }

            var cart = _viewModel.GetCurrentUserCart();
            if (cart.Count == 0)
            {
                MessageBox.Show("Корзина пуста", "Ошибка");
                return;
            }

            var total = _viewModel.GetCartTotalPrice();

            var confirm = MessageBox.Show(
                $"Подтвердите покупку на сумму {total:N0} руб.\n\nТекущий баланс: {_viewModel.CurrentUser.Balance:N0} руб.",
                "Оформление заказа",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                var orderService = new OrderService(_authService, _cartService,
                    new OrderRepository(_context), _productRepo,
                    new UserRepository(_context),
                    new DiscountService(new DiscountRepository(_context), _authService, _productRepo),
                    _context);

                orderService.BuyCart();

                MessageBox.Show("Покупка успешно оформлена! Спасибо за покупку!", "Успешно");

                UpdateCartCount();
                await LoadCartContentAsync();
                await LoadProductsAsync();
                LoadProfileContent();
            }
            catch (Exception ex)
            {
                ErrorForm.Show(ex.Message, ex);
            }
        }

        private async Task RemoveFromCartAsync(Guid productId)
        {
            try
            {
                _viewModel.RemoveFromCart(productId);
                UpdateCartCount();
                await LoadCartContentAsync();
            }
            catch (Exception ex)
            {
                ErrorForm.Show(ex.Message, ex);
            }
        }

        private void UpdateRoleSidebarContent()
        {
            for (int i = roleSidebar.Controls.Count - 1; i >= 1; i--)
            {
                roleSidebar.Controls[i].Dispose();
            }

            if (!_viewModel.IsAuthenticated)
            {
                return;
            }

            var role = _viewModel.CurrentUser.Role;
            int y = 70;

            if (role == Role.Buyer)
            {
                AddRoleButton("Мои заказы", y, () => MessageBox.Show("Список заказов", "Заказы"));
                y += 55;
                AddRoleButton("Профиль", y, () => ShowProfilePanel());
            }
            else if (role == Role.Seller)
            {
                AddRoleButton("Мои товары", y, () => ShowMyProducts());
                y += 55;
                AddRoleButton("Создать товар", y, () => OpenCreateProductForm());
                y += 55;
                AddRoleButton("Статистика", y, () => MessageBox.Show("Статистика продаж", "Статистика"));
                y += 55;
                AddRoleButton("Профиль", y, () => ShowProfilePanel());
            }
            else if (role == Role.Moderator)
            {
                AddRoleButton("Модерация", y, () => OpenModerationPanel());
                y += 55;
                AddRoleButton("Список жалоб", y, () => MessageBox.Show("Список жалоб", "Модерация"));
                y += 55;
                AddRoleButton("Профиль", y, () => ShowProfilePanel());
            }
            else if (role == Role.Admin)
            {
                AddRoleButton("Управление пользователями", y, () => OpenUserManagement());
                y += 55;
                AddRoleButton("Модерация товаров", y, () => OpenModerationPanel());
                y += 55;
                AddRoleButton("Статистика", y, () => OpenStatistics());
                y += 55;
                AddRoleButton("Консоль", y, () => OpenConsole());
                y += 55;
                AddRoleButton("Создать товар", y, () => OpenCreateProductForm());
                y += 55;
                AddRoleButton("Профиль", y, () => ShowProfilePanel());
            }

            AddRoleButton("Выйти", y + 20, () => Logout());
        }

        private void AddRoleButton(string text, int y, Action onClick)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(15, y),
                Size = new Size(250, 45),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft
            };
            btn.Click += (s, e) => onClick();
            roleSidebar.Controls.Add(btn);
        }

        private void ShowProfilePanel()
        {
            cartSidebar.Visible = false;
            roleSidebar.Visible = false;
            profilePanel.Visible = true;
            LoadProfileContent();
        }

        private void LoadProfileContent()
        {
            for (int i = profilePanel.Controls.Count - 1; i >= 1; i--)
            {
                profilePanel.Controls[i].Dispose();
            }

            if (!_viewModel.IsAuthenticated)
            {
                return;
            }

            var user = _viewModel.CurrentUser;
            int y = 80;

            var avatarPanel = new Panel
            {
                Location = new Point(125, y),
                Size = new Size(100, 100),
                BackColor = Color.FromArgb(200, 200, 200),
                BorderStyle = BorderStyle.FixedSingle
            };
            var avatarLabel = new Label
            {
                Text = user.Name.Substring(0, 1).ToUpper(),
                Font = new Font("Segoe UI", 36, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(35, 25),
                AutoSize = true
            };
            avatarPanel.Controls.Add(avatarLabel);
            profilePanel.Controls.Add(avatarPanel);
            y += 110;

            var nameLabel = new Label
            {
                Text = user.Name,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                Location = new Point(90, y),
                Size = new Size(200, 35),
                TextAlign = ContentAlignment.MiddleCenter
            };
            profilePanel.Controls.Add(nameLabel);
            y += 45;

            var roleLabel = new Label
            {
                Text = $"Роль: {user.Role}",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(100, 100, 100),
                Location = new Point(30, y),
                Size = new Size(290, 30)
            };
            profilePanel.Controls.Add(roleLabel);
            y += 35;

            var emailLabel = new Label
            {
                Text = $"Email: {user.Email}",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(100, 100, 100),
                Location = new Point(30, y),
                Size = new Size(290, 30)
            };
            profilePanel.Controls.Add(emailLabel);
            y += 35;

            var balanceLabel = new Label
            {
                Text = $"Баланс: {user.Balance:N0} руб.",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 120, 80),
                Location = new Point(30, y),
                Size = new Size(290, 30)
            };
            profilePanel.Controls.Add(balanceLabel);
            y += 35;

            var statusLabel = new Label
            {
                Text = $"Статус: {(user.IsBlocked ? "Заблокирован" : "Активен")}",
                Font = new Font("Segoe UI", 12),
                ForeColor = user.IsBlocked ? Color.FromArgb(200, 80, 80) : Color.FromArgb(80, 120, 80),
                Location = new Point(30, y),
                Size = new Size(290, 30)
            };
            profilePanel.Controls.Add(statusLabel);
            y += 50;

            var separator = new Label
            {
                Text = new string('-', 30),
                ForeColor = Color.FromArgb(200, 200, 200),
                Location = new Point(30, y),
                Size = new Size(290, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            profilePanel.Controls.Add(separator);
            y += 30;

            var ordersBtn = new Button
            {
                Text = "Мои заказы",
                Location = new Point(30, y),
                Size = new Size(290, 40),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            ordersBtn.Click += (s, e) => MessageBox.Show("Список заказов", "Заказы");
            profilePanel.Controls.Add(ordersBtn);
            y += 50;

            var historyBtn = new Button
            {
                Text = "История заказов",
                Location = new Point(30, y),
                Size = new Size(290, 40),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            historyBtn.Click += (s, e) => ShowOrdersHistory();
            profilePanel.Controls.Add(historyBtn);
            y += 50;

            if (user.Role == Role.Seller)
            {
                var myProductsBtn = new Button
                {
                    Text = "Мои товары",
                    Location = new Point(30, y),
                    Size = new Size(290, 40),
                    BackColor = Color.FromArgb(80, 80, 85),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10)
                };
                myProductsBtn.Click += (s, e) => ShowMyProducts();
                profilePanel.Controls.Add(myProductsBtn);
                y += 50;

                var createProductBtn = new Button
                {
                    Text = "Создать товар",
                    Location = new Point(30, y),
                    Size = new Size(290, 40),
                    BackColor = Color.FromArgb(80, 80, 85),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10)
                };
                createProductBtn.Click += (s, e) => OpenCreateProductForm();
                profilePanel.Controls.Add(createProductBtn);
                y += 50;
            }

            if (user.Role == Role.Moderator || user.Role == Role.Admin)
            {
                var moderationBtn = new Button
                {
                    Text = "Модерация",
                    Location = new Point(30, y),
                    Size = new Size(290, 40),
                    BackColor = Color.FromArgb(80, 80, 85),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10)
                };
                moderationBtn.Click += (s, e) => OpenModerationPanel();
                profilePanel.Controls.Add(moderationBtn);
                y += 50;
            }

            if (user.Role == Role.Admin)
            {
                var adminBtn = new Button
                {
                    Text = "Админ панель",
                    Location = new Point(30, y),
                    Size = new Size(290, 40),
                    BackColor = Color.FromArgb(200, 80, 80),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10)
                };
                adminBtn.Click += (s, e) => OpenUserManagement();
                profilePanel.Controls.Add(adminBtn);
                y += 50;
            }

            var logoutBtn = new Button
            {
                Text = "Выйти из аккаунта",
                Location = new Point(30, y + 20),
                Size = new Size(290, 40),
                BackColor = Color.FromArgb(180, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            logoutBtn.Click += (s, e) => Logout();
            profilePanel.Controls.Add(logoutBtn);

            var closeBtn = new Button
            {
                Text = "Закрыть",
                Location = new Point(30, y + 70),
                Size = new Size(290, 40),
                BackColor = Color.FromArgb(180, 180, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            closeBtn.Click += (s, e) => profilePanel.Visible = false;
            profilePanel.Controls.Add(closeBtn);
        }

        private void ShowMyProducts()
        {
            try
            {
                var products = _viewModel.GetMyProducts();
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

        private void ShowOrdersHistory()
        {
            var orderRepo = new OrderRepository(_context);
            var orders = orderRepo.GetByUser(_viewModel.CurrentUser.Id);

            if (orders.Count == 0)
            {
                MessageBox.Show("У вас пока нет заказов", "История заказов");
                return;
            }

            var ordersForm = new Form
            {
                Text = $"Мои заказы - {_viewModel.CurrentUser.Name}",
                Size = new Size(1000, 550),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            var grid = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(940, 400),
                BackgroundColor = Color.White,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            var data = orders.Select(o => new
            {
                o.Id,
                Дата = o.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                Название_товара = _viewModel.GetProduct(o.ProductId)?.Name ?? "Неизвестно",
                Количество = o.Count,
                Сумма = $"{o.Price:N0} руб.",
                Статус = "Выполнен"
            }).ToList();

            grid.DataSource = data;

            var totalSpent = orders.Sum(o => o.Price);
            var infoPanel = new Panel
            {
                Location = new Point(20, 435),
                Size = new Size(940, 50),
                BackColor = Color.FromArgb(250, 250, 250)
            };

            var totalLabel = new Label
            {
                Text = $"Всего потрачено: {totalSpent:N0} руб.",
                Location = new Point(20, 10),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 120, 80)
            };
            infoPanel.Controls.Add(totalLabel);

            var countLabel = new Label
            {
                Text = $"Всего заказов: {orders.Count}",
                Location = new Point(350, 10),
                Size = new Size(250, 30),
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(60, 60, 60)
            };
            infoPanel.Controls.Add(countLabel);

            var returnBtn = new Button
            {
                Text = "Вернуть выбранный заказ",
                Location = new Point(20, 490),
                Size = new Size(200, 35),
                BackColor = Color.FromArgb(180, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            returnBtn.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Выберите заказ для возврата", "Ошибка");
                    return;
                }
                var orderId = (Guid)grid.SelectedRows[0].Cells["Id"].Value;
                ReturnOrder(orderId);
                ordersForm.Close();
                ShowOrdersHistory();
            };

            var closeBtn = new Button
            {
                Text = "Закрыть",
                Location = new Point(860, 490),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            closeBtn.Click += (s, e) => ordersForm.Close();

            ordersForm.Controls.Add(grid);
            ordersForm.Controls.Add(infoPanel);
            ordersForm.Controls.Add(returnBtn);
            ordersForm.Controls.Add(closeBtn);
            ordersForm.ShowDialog();
        }

        private void ReturnOrder(Guid orderId)
        {
            try
            {
                var orderRepo = new OrderRepository(_context);
                var userRepo = new UserRepository(_context);

                var order = orderRepo.GetById(orderId);
                if (order == null)
                {
                    MessageBox.Show("Заказ не найден", "Ошибка");
                    return;
                }

                if (order.UserId != _viewModel.CurrentUser.Id)
                {
                    MessageBox.Show("Вы можете вернуть только свои заказы", "Ошибка");
                    return;
                }

                var user = userRepo.GetById(order.UserId);
                user.Balance += order.Price;
                userRepo.Update(user);

                orderRepo.Delete(orderId);

                MessageBox.Show($"Заказ возвращён. Вам возвращено {order.Price:N0} руб.", "Успешно");

                var authService = _viewModel.GetAuthService();
                authService.LoginById(user);

                UpdateCartCount();
                LoadProfileContent();
            }
            catch (Exception ex)
            {
                ErrorForm.Show(ex.Message, ex);
            }
        }

        private void OpenCreateProductForm()
        {
            var productRepo = new ProductRepository(_context);
            var productService = new ProductService(productRepo, _authService);
            var form = new CreateProductForm(_authService, productService, _context);
            form.ShowDialog();

            _ = RefreshAllData();
        }

        private void OpenProfileForm()
        {
            var form = new ProfileForm(_authService, _userService, _context, _productService);
            form.ShowDialog();
            if (form.DialogResult == DialogResult.Abort)
            {
                UpdateUserLabel();
                _ = RefreshAllData();
            }
            else
            {
                UpdateUserLabel();
                _ = RefreshAllData();
            }
        }

        private void OpenModerationPanel()
        {
            var productRepo = new ProductRepository(_context);
            var productService = new ProductService(productRepo, _authService);
            var form = new ModerationForm(_authService, productService);
            form.ShowDialog();
            _ = LoadProductsAsync();
        }

        private void OpenUserManagement()
        {
            var form = new AdminPanelForm(_context, _viewModel.CurrentUser);
            form.ShowDialog();
        }

        private void OpenStatistics()
        {
            MessageBox.Show("Статистика маркетплейса в разработке", "Статистика");
        }

        private void OpenConsole()
        {
            var consoleForm = new AdminConsoleForm(_viewModel.CurrentUser);
            consoleForm.ShowDialog();
        }

        private void ShowFavorites()
        {
            roleSidebar.Visible = false;
            cartSidebar.Visible = false;
            profilePanel.Visible = false;
            productsFlow.Controls.Clear();

            try
            {
                if (!_viewModel.IsAuthenticated)
                {
                    MessageBox.Show("Войдите в аккаунт", "Требуется авторизация");
                    _ = LoadProductsAsync();
                    return;
                }

                var products = _viewModel.GetFavoriteProducts();
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

        private void UserInfoLabel_Click(object sender, EventArgs e)
        {
            if (!_viewModel.IsAuthenticated)
            {
                ShowLoginForm();
            }
            else
            {
                OpenProfileForm();
            }
        }

        private void ShowLoginForm()
        {
            var loginForm = new LoginForm(_authService, _userService);
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                UpdateUIAfterLogin(loginForm);
            }
        }

        private void UpdateUIAfterLogin(Form loginForm)
        {
            UpdateUserLabel();
            loginForm.Close();
            LoadData();
            MessageBox.Show($"Добро пожаловать, {_viewModel.CurrentUser.Name}!", "Успешный вход");
        }

        private void Logout()
        {
            _viewModel.Logout();
            UpdateUserLabel();
            roleSidebar.Visible = false;
            cartSidebar.Visible = false;
            profilePanel.Visible = false;
            _ = LoadProductsAsync();
            UpdateCartCount();
        }
    }
}