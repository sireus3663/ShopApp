using ShopProject.Db;
using ShopProject.Forms.ViewModels;
using ShopProject.Models;
using ShopProject.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShopProject.Forms
{
    public class MainForm : Form
    {
        private Panel headerPanel = null!;
        private Panel catalogPanel = null!;
        private FlowLayoutPanel productsFlow = null!;
        private Label userInfoLabel = null!;
        private TextBox searchBox = null!;
        private ComboBox categoryFilter = null!;
        private TextBox txtPriceFrom = null!;
        private TextBox txtPriceTo = null!;
        private AppDbContext _context = null!;
        private Panel cartSidebar = null!;
        private Panel profilePanel = null!;
        private MainViewModel _viewModel = null!;
        private bool _isLoading = false;
        private System.Windows.Forms.Timer _searchTimer = null!;

        private Panel filterPanel = null!;
        private Button searchBtn = null!;
        private Label lblResultsCount = null!;


        private int _currentPage = 1;
        private int _pageSize = 12;
        private int _totalPages = 1;
        private Panel paginationPanel = null!;
        private Label pageInfoLabel = null!;

        private AuthService _authService = null!;
        private ProductRepository _productRepo = null!;
        private CartService _cartService = null!;
        private DiscountService _discountService = null!;
        private UserService _userService = null!;
        private ProductService _productService = null!;
        private Form? _currentEmbeddedForm = null!;
        private Form? _previousEmbeddedForm = null;

        private void ShowEmbeddedForm(Form form, Action? onReady = null)
        {
            if (_currentEmbeddedForm != null && !_currentEmbeddedForm.IsDisposed)
                _currentEmbeddedForm.Dispose();

            headerPanel.Visible = false;
            filterPanel.Visible = false;
            paginationPanel.Visible = false;
            productsFlow.Visible = false;

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.Visible = true;

            form.FormClosed += EmbeddedForm_FormClosed;

            catalogPanel.Controls.Add(form);
            form.BringToFront();

            _currentEmbeddedForm = form;
            onReady?.Invoke();
        }

        private void EmbeddedForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            if (_currentEmbeddedForm == sender)
            {
                _currentEmbeddedForm = null;
                if (_previousEmbeddedForm != null && !_previousEmbeddedForm.IsDisposed)
                {
                    _currentEmbeddedForm = _previousEmbeddedForm;
                    _previousEmbeddedForm = null;
                    _currentEmbeddedForm.Visible = true;
                    _currentEmbeddedForm.BringToFront();
                }
                else
                {
                    headerPanel.Visible = true;
                    filterPanel.Visible = true;
                    paginationPanel.Visible = true;
                    productsFlow.Visible = true;
                    productsFlow.BringToFront();
                }
            }
        }

        public void CloseEmbeddedForm()
        {
            if (_currentEmbeddedForm != null && !_currentEmbeddedForm.IsDisposed)
                _currentEmbeddedForm.Dispose();
            _currentEmbeddedForm = null;

            if (_previousEmbeddedForm != null && !_previousEmbeddedForm.IsDisposed)
                _previousEmbeddedForm.Dispose();
            _previousEmbeddedForm = null;

            headerPanel.Visible = true;
            filterPanel.Visible = true;
            paginationPanel.Visible = true;
            productsFlow.Visible = true;
            productsFlow.BringToFront();
        }

        public MainForm(AuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
            InitializeServices();
            InitializeComponent();
            SubscribeEvents();
            LoadData();

            this.FormClosing += MainForm_FormClosing!;
        }

        private void InitializeServices()
        {
            var configService = new AppConfigService();
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
                UpdateUserLabel();
            }
            catch (Exception ex)
            {
                ErrorForm.Show($"Ошибка загрузки данных: {ex.Message}", ex);
            }
        }

        private void UpdateUserLabel()
        {
            var user = _viewModel.CurrentUser;
            userInfoLabel.Text = _viewModel.IsAuthenticated && user != null
                ? $"\U0001f464 {user.Name}"
                : "\U0001f464 Вход";


        }

        private void SubscribeEvents()
        {
            searchBtn.Click += async (s, e) => await LoadProductsAsync();
            categoryFilter.SelectedIndexChanged += async (s, e) => await LoadProductsAsync();
        }

        private void InitializeComponent()
        {
            this.Text = "ShopApp";
            this.Size = new Size(1300, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = StyleHelper.BgPage;
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

            searchBox = new TextBox
            {
                Location = new Point(210, 16),
                Size = new Size(330, 34),
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.FromArgb(220, 220, 220),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle
            };
            headerPanel.Controls.Add(searchBox);

            _searchTimer = new System.Windows.Forms.Timer { Interval = 300 };
            _searchTimer.Tick += async (s, e) =>
            {
                _searchTimer.Stop();
                _currentPage = 1;
                await LoadProductsAsync();
            };
            searchBox.TextChanged += (s, e) =>
            {
                _searchTimer.Stop();
                _searchTimer.Start();
            };

            searchBtn = new Button
            {
                Text = "Найти",
                Location = new Point(550, 15),
                Size = new Size(70, 34),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            headerPanel.Controls.Add(searchBtn);

            userInfoLabel = new Label
            {
                Text = "\U0001f464 Вход",
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 10),
                AutoSize = false,
                Size = new Size(130, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                Padding = new Padding(3, 0, 0, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            userInfoLabel.Click += UserInfoLabel_Click!;
            headerPanel.Controls.Add(userInfoLabel);
            userInfoLabel.Location = new Point(headerPanel.Width - 160, 18);

            filterPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Color.White
            };

            var filterTitle = new Label
            {
                Text = "🔍 Фильтры",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = StyleHelper.TextPrimary,
                Location = new Point(16, 20),
                AutoSize = true
            };
            filterPanel.Controls.Add(filterTitle);

            var separator = new Label
            {
                Location = new Point(12, 48),
                Size = new Size(196, 1),
                BackColor = StyleHelper.Border
            };
            filterPanel.Controls.Add(separator);

            var catLabel = new Label
            {
                Text = "Категория",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = StyleHelper.TextMuted,
                Location = new Point(16, 62),
                AutoSize = true
            };
            filterPanel.Controls.Add(catLabel);

            categoryFilter = new ComboBox
            {
                Location = new Point(16, 82),
                Size = new Size(186, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                ForeColor = StyleHelper.TextPrimary,
                FlatStyle = FlatStyle.Flat
            };
            categoryFilter.Items.Add("Все категории");
            filterPanel.Controls.Add(categoryFilter);

            var priceLabel = new Label
            {
                Text = "Цена",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = StyleHelper.TextMuted,
                Location = new Point(16, 124),
                AutoSize = true
            };
            filterPanel.Controls.Add(priceLabel);

            txtPriceFrom = new TextBox
            {
                Text = "",
                Location = new Point(16, 146),
                Size = new Size(86, 25),
                BackColor = Color.White,
                ForeColor = StyleHelper.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "от"
            };
            filterPanel.Controls.Add(txtPriceFrom);

            txtPriceTo = new TextBox
            {
                Text = "",
                Location = new Point(112, 146),
                Size = new Size(86, 25),
                BackColor = Color.White,
                ForeColor = StyleHelper.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "до"
            };
            filterPanel.Controls.Add(txtPriceTo);

            var applyPriceBtn = StyleHelper.ModernBtn("Применить", 16, 186, 186, 34, StyleHelper.Accent, Color.White, true);
            applyPriceBtn.Click += async (s, e) =>
            {
                _currentPage = 1;
                await LoadProductsAsync();
            };
            filterPanel.Controls.Add(applyPriceBtn);

            lblResultsCount = new Label
            {
                Text = "Товары: —",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = StyleHelper.TextMuted,
                Location = new Point(16, 230),
                AutoSize = true
            };
            filterPanel.Controls.Add(lblResultsCount);

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

            profilePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Visible = false
            };

            catalogPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = StyleHelper.BgPage
            };

            productsFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24),
                WrapContents = true,
                AutoScroll = true
            };
            catalogPanel.Controls.Add(productsFlow);

            paginationPanel = CreatePaginationPanel();
            catalogPanel.Controls.Add(paginationPanel);

            this.Controls.Add(catalogPanel);
            this.Controls.Add(cartSidebar);
            this.Controls.Add(filterPanel);
            this.Controls.Add(headerPanel);
            this.Controls.Add(profilePanel);
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
            catch (Exception)
            {
            }
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
                Height = 56,
                BackColor = Color.White
            };
            panel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var pen = new Pen(StyleHelper.Border, 1);
                e.Graphics.DrawLine(pen, 0, 0, panel.Width, 0);
                using var shadow = new SolidBrush(Color.FromArgb(8, 0, 0, 0));
                e.Graphics.FillRectangle(shadow, 0, 2, panel.Width, 4);
            };

            Button MakeNavBtn(string text, bool isPrev)
            {
                var btn = new Button
                {
                    Text = text,
                    Size = new Size(110, 34),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(241, 245, 249),
                    ForeColor = StyleHelper.TextPrimary,
                    Font = new Font("Segoe UI", 10),
                    Cursor = Cursors.Hand,
                    TextAlign = isPrev ? ContentAlignment.MiddleLeft : ContentAlignment.MiddleRight,
                    Padding = isPrev ? new Padding(10, 0, 0, 0) : new Padding(0, 0, 10, 0)
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using var path = StyleHelper.GetRoundedPath(btn.ClientRectangle, 8);
                    using var brush = new SolidBrush(btn.BackColor);
                    e.Graphics.FillPath(brush, path);
                    TextRenderer.DrawText(e.Graphics, btn.Text, btn.Font, btn.ClientRectangle, btn.ForeColor,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                };
                btn.MouseEnter += (s, e) => { if (btn.Enabled) { btn.BackColor = Color.FromArgb(226, 232, 240); btn.Invalidate(); } };
                btn.MouseLeave += (s, e) => { btn.BackColor = Color.FromArgb(241, 245, 249); btn.Invalidate(); };
                return btn;
            }

            var prevBtn = MakeNavBtn("◀  Назад", true);
            prevBtn.Click += async (s, e) =>
            {
                if (_currentPage > 1) { _currentPage--; await LoadProductsAsync(); }
            };
            panel.Controls.Add(prevBtn);

            pageInfoLabel = new Label
            {
                Text = "Стр. 1 / 1",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = StyleHelper.TextPrimary,
                Font = new Font("Segoe UI", 10),
                AutoSize = true
            };
            panel.Controls.Add(pageInfoLabel);

            var nextBtn = MakeNavBtn("Вперед  ▶", false);
            nextBtn.Click += async (s, e) =>
            {
                if (_currentPage < _totalPages) { _currentPage++; await LoadProductsAsync(); }
            };
            panel.Controls.Add(nextBtn);

            var perPageLabel = new Label
            {
                Text = "Показать по:",
                ForeColor = StyleHelper.TextMuted,
                Font = new Font("Segoe UI", 9),
                AutoSize = true
            };
            panel.Controls.Add(perPageLabel);

            var sizeCombo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            sizeCombo.Items.AddRange(new object[] { 12, 24, 48, 96 });
            sizeCombo.SelectedItem = _pageSize;
            sizeCombo.SelectedIndexChanged += async (s, e) =>
            {
                _pageSize = (int)sizeCombo.SelectedItem;
                _currentPage = 1;
                await LoadProductsAsync();
            };
            panel.Controls.Add(sizeCombo);

            panel.Resize += (s, e) =>
            {
                int y = (panel.Height - 34) / 2;
                int leftGroupX = 24;

                prevBtn.Location = new Point(leftGroupX, y);
                int pageLabelW = TextRenderer.MeasureText(pageInfoLabel.Text, pageInfoLabel.Font).Width;
                pageInfoLabel.Location = new Point(leftGroupX + 120 + 12, y + 7);
                nextBtn.Location = new Point(leftGroupX + 120 + 12 + pageLabelW + 12, y);

                int comboW = 64;
                int perPageW = TextRenderer.MeasureText(perPageLabel.Text, perPageLabel.Font).Width;
                sizeCombo.Size = new Size(comboW, 24);
                sizeCombo.Location = new Point(panel.Width - 24 - comboW, y + 4);
                perPageLabel.Location = new Point(panel.Width - 24 - comboW - 8 - perPageW, y + 7);
            };

            return panel;
        }

        private async Task LoadProductsAsync()
        {
            if (_isLoading) return;
            _isLoading = true;
            productsFlow.Controls.Clear();

            try
            {
                var searchText = searchBox.Text;
                var allProducts = _viewModel.SearchProducts(searchText);
                allProducts = _viewModel.FilterByCategory(allProducts, categoryFilter.SelectedItem?.ToString());
                allProducts = FilterByPrice(allProducts);

                if (lblResultsCount != null)
                {
                    var totalAll = _viewModel.SearchProducts("").Count;
                    var filtered = allProducts.Count;
                    lblResultsCount.Text = filtered == totalAll
                        ? $"📦 {totalAll} товаров"
                        : $"📦 {filtered} из {totalAll}";
                }

                _totalPages = (int)Math.Ceiling((double)allProducts.Count / _pageSize);
                if (_totalPages == 0) _totalPages = 1;
                if (_currentPage > _totalPages) _currentPage = _totalPages;

                var products = allProducts
                    .Skip((_currentPage - 1) * _pageSize)
                    .Take(_pageSize)
                    .ToList();

                if (pageInfoLabel != null)
                    pageInfoLabel.Text = $"Стр. {_currentPage} / {_totalPages}";

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
            var card = StyleHelper.RoundedCard(200, 330, 10);
            card.Margin = new Padding(8);
            card.BackColor = Color.White;

            EventHandler openDetail = (s, e) =>
            {
                var detailForm = new ProductDetailForm(product, _authService, _context);
                ShowEmbeddedForm(detailForm);
            };

            var imageBox = StyleHelper.ProductImageBox(product.ProductImage, 176, 130);
            imageBox.Location = new Point(12, 10);
            StyleHelper.SetRoundedRegion(imageBox, 8);
            imageBox.Click += openDetail;
            card.Controls.Add(imageBox);

            var nameLabel = new Label
            {
                Text = product.Name,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = StyleHelper.TextPrimary,
                Location = new Point(12, 150),
                Size = new Size(176, 38),
                AutoEllipsis = true
            };
            nameLabel.Click += openDetail;
            card.Controls.Add(nameLabel);

            var catLabel = new Label
            {
                Text = product.Category ?? "",
                Font = new Font("Segoe UI", 8),
                ForeColor = StyleHelper.TextMuted,
                Location = new Point(12, 188),
                AutoSize = true
            };
            catLabel.Click += openDetail;
            card.Controls.Add(catLabel);

            var finalPrice = _discountService.CalculatePrice(product);
            int priceY = 208;

            if (finalPrice < product.Price)
            {
                var oldPrice = new Label
                {
                    Text = $"{product.Price:N0} ₽",
                    Location = new Point(12, priceY),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9, FontStyle.Strikeout),
                    ForeColor = Color.Gray
                };
                card.Controls.Add(oldPrice);

                var newPrice = new Label
                {
                    Text = $"{finalPrice:N0} ₽",
                    Location = new Point(12, priceY + 16),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    ForeColor = StyleHelper.Danger
                };
                newPrice.Click += openDetail;
                card.Controls.Add(newPrice);
            }
            else
            {
                var priceLabel = new Label
                {
                    Text = $"{product.Price:N0} ₽",
                    Location = new Point(12, priceY),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    ForeColor = StyleHelper.TextPrimary
                };
                priceLabel.Click += openDetail;
                card.Controls.Add(priceLabel);
            }

            var cartButton = StyleHelper.ModernBtn("🛒 Купить", 12, 280, 120, 34, StyleHelper.Accent, Color.White, true);
            cartButton.Click += async (s, e) =>
            {
                if (!_viewModel.IsAuthenticated) { ShowLoginForm(); return; }
                await AddToCartAsync(product.Id);
            };
            card.Controls.Add(cartButton);

            bool isFavorite = _viewModel.IsFavorite(product.Id);
            var favButton = new Button
            {
                Text = isFavorite ? "★" : "☆",
                Size = new Size(46, 34),
                Location = new Point(142, 280),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = isFavorite ? Color.FromArgb(234, 179, 8) : Color.FromArgb(156, 163, 175),
                Font = new Font("Segoe UI", 14),
                Cursor = Cursors.Hand
            };
            favButton.FlatAppearance.BorderSize = 0;
            favButton.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var b = (Button)s!;
                using var path = StyleHelper.GetRoundedPath(b.ClientRectangle, 8);
                e.Graphics.FillPath(new SolidBrush(b.BackColor), path);
                using var pen = new Pen(StyleHelper.Border, 1);
                e.Graphics.DrawPath(pen, path);
                TextRenderer.DrawText(e.Graphics, b.Text, b.Font,
                    b.ClientRectangle, b.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            favButton.Click += async (s, e) =>
            {
                if (!_viewModel.IsAuthenticated) { ShowLoginForm(); return; }
                await ToggleFavoriteAsync(product.Id, favButton);
            };
            card.Controls.Add(favButton);

            card.MouseEnter += (s, e) =>
            {
                card.BackColor = Color.FromArgb(248, 250, 252);
                card.Invalidate();
            };
            card.MouseLeave += (s, e) =>
            {
                card.BackColor = Color.White;
                card.Invalidate();
            };
            foreach (Control c in card.Controls)
            {
                c.MouseEnter += (s, e) =>
                {
                    card.BackColor = Color.FromArgb(248, 250, 252);
                    card.Invalidate();
                };
                c.MouseLeave += (s, e) =>
                {
                    card.BackColor = Color.White;
                    card.Invalidate();
                };
            }

            card.Click += openDetail;
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
                favButton.Text = isFavorite ? "★" : "☆";
                favButton.ForeColor = isFavorite ? Color.FromArgb(200, 200, 50) : Color.FromArgb(150, 150, 150);
            }
            catch (Exception ex)
            {
                ErrorForm.Show(ex.Message, ex);
            }
        }

        private void OpenConsole()
        {
            var user = _viewModel.CurrentUser;
            if (user == null)
            {
                MessageBox.Show("Сначала выполните вход", "Ошибка");
                return;
            }
            ConsoleHelper.OpenAdminConsole(user);
        }

        private void OpenCreateProductForm()
        {
            var productRepo = new ProductRepository(_context);
            var productService = new ProductService(productRepo, _authService);
            var form = new CreateProductForm(_authService, productService, _context);
            form.FormClosed += (s, e) => _ = RefreshAllData();
            ShowEmbeddedForm(form);
        }

        private void OpenModerationPanel()
        {
            var productRepo = new ProductRepository(_context);
            var productService = new ProductService(productRepo, _authService);
            var form = new ModerationForm(_authService, productService);
            form.FormClosed += (s, e) => _ = LoadProductsAsync();
            ShowEmbeddedForm(form);
        }

        private void OpenUserManagement()
        {
            var user = _viewModel.CurrentUser;
            if (user == null)
            {
                MessageBox.Show("Сначала выполните вход", "Ошибка");
                return;
            }
            var form = new AdminPanelForm(_context, user);
            ShowEmbeddedForm(form);
        }

        private void ShowProfileEmbedded()
        {
            var orderRepo = new OrderRepository(_context);
            var productRepo = new ProductRepository(_context);
            var userRepo = new UserRepository(_context);
            var orderService = new OrderService(_authService, _cartService, orderRepo, productRepo, userRepo, _discountService, _context);
            var profileForm = new ProfileForm(_authService, _userService, _context, _productService, orderService);
            profileForm.Owner = this;
            profileForm.ProductClicked = (product) => OpenProductDetail(product);
            profileForm.ProductClickedForModeration = (product, ps) => OpenProductDetail(product, ps);
            profileForm.OrderClicked = null;
            ShowEmbeddedForm(profileForm);
            profileForm.RefreshData();
        }

        private void UserInfoLabel_Click(object? sender, EventArgs e)
        {
            if (!_viewModel.IsAuthenticated)
            {
                ShowLoginForm();
            }
            else
            {
                ShowProfileEmbedded();
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
            var user = _viewModel.CurrentUser;
            if (user != null)
            {
                MessageBox.Show($"Добро пожаловать, {user.Name}!", "Успешный вход");
            }
        }

        public void Logout()
        {
            _viewModel.Logout();
            UpdateUserLabel();
            cartSidebar.Visible = false;
            CloseEmbeddedForm();
            _ = LoadProductsAsync();
        }

        public void OpenProductDetail(Product product, ProductService? productService = null)
        {
            var detailForm = new ProductDetailForm(product, _authService, _context, productService);

            detailForm.FormClosed += (s, e) =>
            {
                if (_previousEmbeddedForm != null && !_previousEmbeddedForm.IsDisposed)
                {
                    _currentEmbeddedForm = _previousEmbeddedForm;
                    _previousEmbeddedForm = null;
                    _currentEmbeddedForm.Visible = true;
                    _currentEmbeddedForm.BringToFront();
                }
                else
                {
                    _currentEmbeddedForm = null;
                    headerPanel.Visible = true;
                    filterPanel.Visible = true;
                    paginationPanel.Visible = true;
                    productsFlow.Visible = true;
                    productsFlow.BringToFront();
                }

                if (productService != null)
                {
                    _ = LoadProductsAsync();
                    if (_currentEmbeddedForm is ProfileForm pf)
                        pf.RefreshModerationList();
                }
            };

            if (_currentEmbeddedForm != null && !_currentEmbeddedForm.IsDisposed)
            {
                _previousEmbeddedForm = _currentEmbeddedForm;
                _previousEmbeddedForm.Visible = false;
                _currentEmbeddedForm = null;
            }
            else
            {
                _previousEmbeddedForm = null;
            }

            headerPanel.Visible = false;
            filterPanel.Visible = false;
            paginationPanel.Visible = false;
            productsFlow.Visible = false;

            detailForm.TopLevel = false;
            detailForm.FormBorderStyle = FormBorderStyle.None;
            detailForm.Dock = DockStyle.Fill;
            detailForm.Visible = true;

            catalogPanel.Controls.Add(detailForm);
            detailForm.BringToFront();
            _currentEmbeddedForm = detailForm;
        }

        public void SetSearchText(string searchText)
        {
            searchBox.Text = searchText;
            _ = LoadProductsAsync();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _authService?.Logout();
                _context?.Dispose();

                Application.Exit();
                Environment.Exit(0);
            }
            catch
            {
                Environment.Exit(0);
            }
        }
    }
}
