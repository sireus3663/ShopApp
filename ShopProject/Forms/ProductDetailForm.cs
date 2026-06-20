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
        private Label lblDiscountBadge = null!;
        private Label lblSeller = null!;
        private Label lblSeparator1 = null!;
        private Label lblSeparator2 = null!;
        private Panel pnlDescriptionWrapper = null!;
        private RichTextBox rtbDescription = null!;
        private FlowLayoutPanel rightFlow = null!;
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

            var imageCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(241, 245, 249),
                Margin = new Padding(0),
                Padding = new Padding(2)
            };
            imageCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, imageCard.Width - 1, imageCard.Height - 1);
                using var path = StyleHelper.GetRoundedPath(rect, 12);
                using var fill = new SolidBrush(Color.FromArgb(241, 245, 249));
                e.Graphics.FillPath(fill, path);
                using var pen = new Pen(StyleHelper.Border, 1);
                e.Graphics.DrawPath(pen, path);
                using var shadow = new SolidBrush(Color.FromArgb(16, 0, 0, 0));
                e.Graphics.FillRectangle(shadow, 4, imageCard.Height - 6, imageCard.Width - 8, 6);
            };

            pbImage = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            imageCard.Controls.Add(pbImage);
            leftPanel.Controls.Add(imageCard);

            rightFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(40, 0, 10, 16),
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            btnBack = new Button
            {
                Text = "← Назад к каталогу",
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = StyleHelper.TextPrimary,
                Cursor = Cursors.Hand,
                Padding = new Padding(14, 7, 14, 7),
                Margin = new Padding(0, 0, 0, 8)
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
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = StyleHelper.TextPrimary,
                AutoSize = false,
                Height = 70,
                Margin = new Padding(0, 4, 0, 4)
            };

            lblPrice = new Label
            {
                Font = new Font("Segoe UI", 26, FontStyle.Bold),
                ForeColor = StyleHelper.Accent,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 0)
            };

            lblOldPrice = new Label
            {
                Font = new Font("Segoe UI", 14, FontStyle.Strikeout),
                ForeColor = Color.Gray,
                AutoSize = true,
                Margin = new Padding(0, 2, 0, 0),
                Visible = false
            };

            lblDiscountBadge = new Label
            {
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = StyleHelper.Danger,
                AutoSize = true,
                Padding = new Padding(8, 3, 8, 3),
                Margin = new Padding(12, 8, 0, 0),
                Visible = false
            };

            lblCategory = new Label
            {
                Font = new Font("Segoe UI", 11),
                ForeColor = StyleHelper.TextMuted,
                AutoSize = true,
                Margin = new Padding(0, 8, 0, 0)
            };

            lblSeller = new Label
            {
                Font = new Font("Segoe UI", 10),
                ForeColor = StyleHelper.TextMuted,
                AutoSize = true,
                Margin = new Padding(0, 2, 0, 0)
            };

            lblSeparator1 = new Label
            {
                Text = "",
                BackColor = StyleHelper.Border,
                Height = 1,
                Margin = new Padding(0, 14, 0, 10),
                AutoSize = false
            };

            var descHeader = new Label
            {
                Text = "📄 Описание",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = StyleHelper.TextPrimary,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 4)
            };

            pnlDescriptionWrapper = new Panel
            {
                BackColor = Color.FromArgb(248, 250, 252),
                Height = 140,
                Margin = new Padding(0, 0, 0, 6),
                AutoSize = false
            };
            pnlDescriptionWrapper.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, pnlDescriptionWrapper.Width - 1, pnlDescriptionWrapper.Height - 1);
                using var path = StyleHelper.GetRoundedPath(rect, 8);
                using var fill = new SolidBrush(pnlDescriptionWrapper.BackColor);
                e.Graphics.FillPath(fill, path);
                using var pen = new Pen(StyleHelper.Border, 1);
                e.Graphics.DrawPath(pen, path);
            };

            rtbDescription = new RichTextBox
            {
                BackColor = Color.FromArgb(248, 250, 252),
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = StyleHelper.TextPrimary,
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 8, 12, 8),
                ScrollBars = RichTextBoxScrollBars.Vertical,
                DetectUrls = false
            };
            pnlDescriptionWrapper.Controls.Add(rtbDescription);

            lblSeparator2 = new Label
            {
                Text = "",
                BackColor = StyleHelper.Border,
                Height = 1,
                Margin = new Padding(0, 8, 0, 12),
                AutoSize = false
            };

            bool isModeration = _productService != null;

            btnAddToCart = new Button
            {
                Text = "🛒 Добавить в корзину",
                Size = new Size(220, 50),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = StyleHelper.Accent,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 0, 0),
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
                Size = new Size(50, 50),
                Font = new Font("Segoe UI", 20),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(156, 163, 175),
                Cursor = Cursors.Hand,
                Margin = new Padding(12, 0, 0, 0),
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
                Size = new Size(180, 50),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(22, 163, 74),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 12, 0),
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
                Size = new Size(180, 50),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 38, 38),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 0, 0),
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

            var actionsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.White,
                Margin = new Padding(0, 4, 0, 0)
            };

            if (isModeration)
            {
                actionsPanel.Controls.Add(btnApprove);
                actionsPanel.Controls.Add(btnDecline);
            }
            else
            {
                actionsPanel.Controls.Add(btnAddToCart);
                actionsPanel.Controls.Add(btnFavorite);
            }

            rightFlow.Controls.Add(btnBack);
            rightFlow.Controls.Add(lblName);
            rightFlow.Controls.Add(lblPrice);
            rightFlow.Controls.Add(lblCategory);
            rightFlow.Controls.Add(lblSeller);
            rightFlow.Controls.Add(lblSeparator1);
            rightFlow.Controls.Add(descHeader);
            rightFlow.Controls.Add(pnlDescriptionWrapper);
            rightFlow.Controls.Add(lblSeparator2);
            rightFlow.Controls.Add(actionsPanel);

            rightFlow.Resize += (s, e) =>
            {
                int w = rightFlow.ClientSize.Width - rightFlow.Padding.Horizontal;
                if (w > 0)
                {
                    lblName.Width = w;
                    lblSeparator1.Width = w;
                    lblSeparator2.Width = w;
                    pnlDescriptionWrapper.Width = w;
                }
            };

            mainPanel.Controls.Clear();
            mainPanel.Controls.Add(leftPanel, 0, 0);
            mainPanel.Controls.Add(rightFlow, 1, 0);

            this.Controls.Clear();
            this.Controls.Add(mainPanel);
        }

        private void LoadProductDetails()
        {
            lblName.Text = _product.Name ?? "Без названия";
            lblCategory.Text = $"📂 {_product.Category ?? "Не указана"}";
            LoadSellerInfo();

            rtbDescription.Text = _product.Description ?? "Описание отсутствует";

            decimal finalPrice = _discountService.CalculatePrice(_product);

            int priceIndex = rightFlow.Controls.GetChildIndex(lblPrice);

            if (finalPrice < _product.Price)
            {
                lblPrice.Text = $"{finalPrice:N0} ₽";
                lblPrice.ForeColor = StyleHelper.Danger;

                var discountRow = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    BackColor = Color.White,
                    Margin = new Padding(0, 2, 0, 0),
                    Name = "discountRow"
                };

                lblOldPrice.Text = $"{_product.Price:N0} ₽";
                lblOldPrice.Visible = true;
                discountRow.Controls.Add(lblOldPrice);

                var disc = _discountService.GetByProduct(_product.Id);
                if (disc != null)
                {
                    lblDiscountBadge.Text = $"  -{disc.Percent:F0}%  ";
                    lblDiscountBadge.Visible = true;
                    discountRow.Controls.Add(lblDiscountBadge);
                }

                var existing = rightFlow.Controls["discountRow"];
                if (existing != null)
                    rightFlow.Controls.Remove(existing);

                rightFlow.Controls.Add(discountRow);
                rightFlow.Controls.SetChildIndex(discountRow, priceIndex + 1);
            }
            else
            {
                lblPrice.Text = $"{_product.Price:N0} ₽";
                lblPrice.ForeColor = StyleHelper.Accent;
                lblOldPrice.Visible = false;
                lblDiscountBadge.Visible = false;

                var existing = rightFlow.Controls["discountRow"];
                if (existing != null)
                    rightFlow.Controls.Remove(existing);
            }

            UpdateFavoriteState();

            if (_product.ProductImage != null && _product.ProductImage.Length > 0)
            {
                try
                {
                    using var ms = new MemoryStream(_product.ProductImage);
                    var img = Image.FromStream(ms);
                    pbImage.Image = (Image)img.Clone();
                    img.Dispose();
                }
                catch { }
            }
        }

        private void LoadSellerInfo()
        {
            if (_product.SellerId == null)
            {
                lblSeller.Text = "";
                return;
            }
            try
            {
                var seller = _context.users.Find(_product.SellerId.Value);
                lblSeller.Text = seller != null
                    ? $"👤 Продавец: {seller.Name}"
                    : "";
            }
            catch
            {
                lblSeller.Text = "";
            }
        }

        private void UpdateFavoriteState()
        {
            if (_favoriteService == null) return;
            var user = _authService.currentUser;
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
            if (_authService.currentUser == null)
            {
                MessageBox.Show("Сначала выполните вход", "Ошибка");
                return;
            }
            _favoriteService.ToggleFavorite(_product.Id);
            UpdateFavoriteState();
        }

        private void AddToCart()
        {
            var user = _authService.currentUser;
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
