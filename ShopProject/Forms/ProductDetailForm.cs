using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;

namespace ShopProject.Forms
{
    public partial class ProductDetailForm : Form
    {
        private readonly Product _product;
        private readonly AuthService _authService;
        private readonly CartService _cartService;
        private readonly DiscountService _discountService;
        private readonly FavoriteService _favoriteService;
        private readonly AppDbContext _context;
        private readonly ProductService? _productService;

        private Button btnBack = null!;
        private Button btnFavorite = null!;
        private Button btnApprove = null!;
        private Button btnDecline = null!;
        private bool _isFavorite;

        public ProductDetailForm(Product product, AuthService authService, AppDbContext context, ProductService? productService = null)
        {
            _product = product ?? throw new ArgumentNullException(nameof(product));
            _authService = authService;
            _context = context;
            _productService = productService;
            InitializeComponent();

            var cartRepo = new CartRepository(context);
            var discountRepo = new DiscountRepository(context);
            var productRepo = new ProductRepository(context);
            var favoriteRepo = new FavoriteRepository(context);

            _cartService = new CartService(cartRepo, authService);
            _discountService = new DiscountService(discountRepo, authService, productRepo);
            _favoriteService = new FavoriteService(favoriteRepo, authService);

            SetupLayout();
            LoadProductDetails();
        }

        private void SetupLayout()
        {
            this.BackColor = StyleHelper.BgPage;

            mainPanel.BackColor = Color.White;
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.ColumnCount = 2;
            mainPanel.RowCount = 1;
            mainPanel.ColumnStyles.Clear();
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            mainPanel.RowStyles.Clear();
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainPanel.Padding = new Padding(40);
            mainPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;

            var leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            pbImage = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(241, 245, 249)
            };
            leftPanel.Controls.Add(pbImage);

            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(40, 0, 0, 0)
            };

            btnBack = new Button
            {
                Text = "← Назад",
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = StyleHelper.TextPrimary,
                Cursor = Cursors.Hand,
                Padding = new Padding(12, 6, 12, 6)
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = StyleHelper.GetRoundedPath(btnBack.ClientRectangle, 8);
                using var brush = new SolidBrush(btnBack.BackColor);
                e.Graphics.FillPath(brush, path);
                TextRenderer.DrawText(e.Graphics, btnBack.Text, btnBack.Font,
                    btnBack.ClientRectangle, btnBack.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            btnBack.MouseEnter += (s, e) => { btnBack.BackColor = Color.FromArgb(226, 232, 240); btnBack.Invalidate(); };
            btnBack.MouseLeave += (s, e) => { btnBack.BackColor = Color.FromArgb(241, 245, 249); btnBack.Invalidate(); };
            btnBack.Click += (s, e) => this.Close();

            lblName = new Label
            {
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = StyleHelper.TextPrimary,
                AutoSize = false,
                Size = new Size(0, 60)
            };

            lblPrice = new Label
            {
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = StyleHelper.Accent,
                AutoSize = true
            };

            lblOldPrice = new Label
            {
                Font = new Font("Segoe UI", 14, FontStyle.Strikeout),
                ForeColor = Color.Gray,
                AutoSize = true,
                Visible = false
            };

            lblCategory = new Label
            {
                Font = new Font("Segoe UI", 11),
                ForeColor = StyleHelper.TextMuted,
                AutoSize = true
            };

            lblDescription = new Label
            {
                Font = new Font("Segoe UI", 12),
                ForeColor = StyleHelper.TextPrimary,
                AutoSize = false,
                AutoEllipsis = true
            };

            bool isModeration = _productService != null;

            btnAddToCart = new Button
            {
                Text = "🛒 Добавить в корзину",
                Size = new Size(220, 48),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = StyleHelper.Accent,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Visible = !isModeration
            };
            btnAddToCart.FlatAppearance.BorderSize = 0;
            btnAddToCart.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = StyleHelper.GetRoundedPath(btnAddToCart.ClientRectangle, 10);
                using var brush = new SolidBrush(btnAddToCart.BackColor);
                e.Graphics.FillPath(brush, path);
                TextRenderer.DrawText(e.Graphics, btnAddToCart.Text, btnAddToCart.Font,
                    btnAddToCart.ClientRectangle, btnAddToCart.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            btnAddToCart.MouseEnter += (s, e) =>
            {
                btnAddToCart.BackColor = ControlPaint.Dark(StyleHelper.Accent, 0.1f);
                btnAddToCart.Invalidate();
            };
            btnAddToCart.MouseLeave += (s, e) =>
            {
                btnAddToCart.BackColor = StyleHelper.Accent;
                btnAddToCart.Invalidate();
            };
            btnAddToCart.Click += (s, e) => AddToCart();

            btnFavorite = new Button
            {
                Size = new Size(48, 48),
                Font = new Font("Segoe UI", 18),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(156, 163, 175),
                Cursor = Cursors.Hand,
                Visible = !isModeration
            };
            btnFavorite.FlatAppearance.BorderSize = 0;
            btnFavorite.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = StyleHelper.GetRoundedPath(btnFavorite.ClientRectangle, 10);
                using var brush = new SolidBrush(btnFavorite.BackColor);
                e.Graphics.FillPath(brush, path);
                using var pen = new Pen(StyleHelper.Border, 1);
                e.Graphics.DrawPath(pen, path);
                TextRenderer.DrawText(e.Graphics, btnFavorite.Text, btnFavorite.Font,
                    btnFavorite.ClientRectangle, btnFavorite.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            btnFavorite.MouseEnter += (s, e) => { btnFavorite.BackColor = Color.FromArgb(248, 250, 252); btnFavorite.Invalidate(); };
            btnFavorite.MouseLeave += (s, e) => { btnFavorite.BackColor = Color.White; btnFavorite.Invalidate(); };
            btnFavorite.Click += (s, e) => ToggleFavorite();

            btnApprove = new Button
            {
                Text = "✓ Одобрить",
                Size = new Size(180, 48),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(22, 163, 74),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Visible = isModeration
            };
            btnApprove.FlatAppearance.BorderSize = 0;
            btnApprove.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = StyleHelper.GetRoundedPath(btnApprove.ClientRectangle, 10);
                using var brush = new SolidBrush(btnApprove.BackColor);
                e.Graphics.FillPath(brush, path);
                TextRenderer.DrawText(e.Graphics, btnApprove.Text, btnApprove.Font,
                    btnApprove.ClientRectangle, btnApprove.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            btnApprove.MouseEnter += (s, e) =>
            {
                btnApprove.BackColor = ControlPaint.Dark(Color.FromArgb(22, 163, 74), 0.1f);
                btnApprove.Invalidate();
            };
            btnApprove.MouseLeave += (s, e) =>
            {
                btnApprove.BackColor = Color.FromArgb(22, 163, 74);
                btnApprove.Invalidate();
            };
            btnApprove.Click += (s, e) => ApproveProduct();

            btnDecline = new Button
            {
                Text = "✕ Отклонить",
                Size = new Size(180, 48),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 38, 38),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Visible = isModeration
            };
            btnDecline.FlatAppearance.BorderSize = 0;
            btnDecline.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = StyleHelper.GetRoundedPath(btnDecline.ClientRectangle, 10);
                using var brush = new SolidBrush(btnDecline.BackColor);
                e.Graphics.FillPath(brush, path);
                TextRenderer.DrawText(e.Graphics, btnDecline.Text, btnDecline.Font,
                    btnDecline.ClientRectangle, btnDecline.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            btnDecline.MouseEnter += (s, e) =>
            {
                btnDecline.BackColor = ControlPaint.Dark(Color.FromArgb(220, 38, 38), 0.1f);
                btnDecline.Invalidate();
            };
            btnDecline.MouseLeave += (s, e) =>
            {
                btnDecline.BackColor = Color.FromArgb(220, 38, 38);
                btnDecline.Invalidate();
            };
            btnDecline.Click += (s, e) => DeclineProduct();

            int y = 0;
            btnBack.Location = new Point(0, y);
            rightPanel.Controls.Add(btnBack);

            y += 50;
            lblName.Location = new Point(0, y);
            lblName.Width = rightPanel.Width - 40;
            rightPanel.Controls.Add(lblName);

            y += 70;
            lblPrice.Location = new Point(0, y);
            rightPanel.Controls.Add(lblPrice);

            y += 40;
            lblOldPrice.Location = new Point(0, y);
            rightPanel.Controls.Add(lblOldPrice);

            if (lblOldPrice.Visible)
                y += 30;

            lblCategory.Location = new Point(0, y + 10);
            rightPanel.Controls.Add(lblCategory);

            y += 40;
            lblDescription.Location = new Point(0, y);
            lblDescription.Size = new Size(rightPanel.Width - 40, 100);
            rightPanel.Controls.Add(lblDescription);

            y += 120;

            btnAddToCart.Location = new Point(0, y);
            rightPanel.Controls.Add(btnAddToCart);

            btnFavorite.Location = new Point(230, y);
            rightPanel.Controls.Add(btnFavorite);

            btnApprove.Location = new Point(0, y);
            rightPanel.Controls.Add(btnApprove);

            btnDecline.Location = new Point(190, y);
            rightPanel.Controls.Add(btnDecline);

            rightPanel.Resize += (s, e) =>
            {
                lblName.Width = rightPanel.Width - 40;
                lblDescription.Width = rightPanel.Width - 40;
            };

            mainPanel.Controls.Clear();
            mainPanel.Controls.Add(leftPanel, 0, 0);
            mainPanel.Controls.Add(rightPanel, 1, 0);

            this.Controls.Clear();
            this.Controls.Add(mainPanel);
        }

        private void LoadProductDetails()
        {
            lblName.Text = _product.Name ?? "Без названия";
            lblCategory.Text = $"Категория: {_product.Category ?? "Не указана"}";
            lblDescription.Text = _product.Description ?? "Описание отсутствует";

            decimal finalPrice = _discountService.CalculatePrice(_product);

            if (finalPrice < _product.Price)
            {
                lblPrice.Text = $"{finalPrice:N0} ₽";
                lblOldPrice.Text = $"{_product.Price:N0} ₽";
                lblOldPrice.Visible = true;
            }
            else
            {
                lblPrice.Text = $"{_product.Price:N0} ₽";
                lblOldPrice.Visible = false;
            }

            UpdateFavoriteState();

            if (_product.ProductImage != null && _product.ProductImage.Length > 0)
            {
                try
                {
                    using var ms = new MemoryStream(_product.ProductImage);
                    pbImage.Image = Image.FromStream(ms);
                }
                catch { }
            }
        }

        private void UpdateFavoriteState()
        {
            if (_favoriteService == null) return;
            var user = _authService.CurrentUser;
            if (user != null)
            {
                var favs = _favoriteService.GetUserFavorites(user.Id);
                _isFavorite = favs.Any(f => f.ProductId == _product.Id);
            }
            else
            {
                _isFavorite = false;
            }
            btnFavorite.Text = _isFavorite ? "★" : "☆";
            btnFavorite.ForeColor = _isFavorite ? Color.FromArgb(234, 179, 8) : Color.FromArgb(156, 163, 175);
        }

        private void ToggleFavorite()
        {
            if (_authService.CurrentUser == null)
            {
                MessageBox.Show("Сначала выполните вход", "Ошибка");
                return;
            }
            _favoriteService.ToggleFavorite(_product.Id);
            UpdateFavoriteState();
        }

        private void AddToCart()
        {
            var user = _authService.CurrentUser;
            if (user == null)
            {
                MessageBox.Show("Сначала выполните вход", "Ошибка");
                return;
            }

            try
            {
                _cartService.AddToCart(_product.Id);
                MessageBox.Show("Товар добавлен в корзину", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void ApproveProduct()
        {
            if (_productService == null) return;
            try
            {
                _productService.Approve(_product.Id);
                MessageBox.Show($"Товар «{_product.Name}» одобрен", "Успех");
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void DeclineProduct()
        {
            if (_productService == null) return;
            try
            {
                _productService.Decline(_product.Id);
                MessageBox.Show($"Товар «{_product.Name}» отклонён", "Успех");
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

    }
}
