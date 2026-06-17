using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;
using ShopProject.Services.Interfaces;

namespace ShopProject.Forms
{
    public interface IRefreshable
    {
        void Refresh();
    }

    internal static class StyleHelper
    {
        public static Color BgPage = Color.FromArgb(245, 247, 250);
        public static Color BgCard = Color.White;
        public static Color Accent = Color.FromArgb(37, 99, 235);
        public static Color Danger = Color.FromArgb(239, 68, 68);
        public static Color Success = Color.FromArgb(22, 163, 74);
        public static Color TextPrimary = Color.FromArgb(15, 23, 42);
        public static Color TextMuted = Color.FromArgb(100, 116, 139);
        public static Color Border = Color.FromArgb(226, 232, 240);

        public static GraphicsPath GetRoundedPath(Rectangle rect, int r)
        {
            var p = new GraphicsPath();
            p.AddArc(rect.X, rect.Y, r * 2, r * 2, 180, 90);
            p.AddArc(rect.Right - r * 2, rect.Y, r * 2, r * 2, 270, 90);
            p.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
            p.AddArc(rect.X, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
            p.CloseFigure();
            return p;
        }

        public static void SetRoundedRegion(Control c, int r)
        {
            c.Region = new Region(GetRoundedPath(c.ClientRectangle, r));
        }

        public static PictureBox ProductImageBox(byte[]? imageData, int w, int h)
        {
            var pb = new PictureBox
            {
                Size = new Size(w, h),
                BackColor = Color.FromArgb(241, 245, 249),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            if (imageData != null && imageData.Length > 0)
            {
                try
                {
                    using var ms = new MemoryStream(imageData);
                    pb.Image = Image.FromStream(ms);
                }
                catch { }
            }
            if (pb.Image == null)
            {
                pb.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using var brush = new SolidBrush(Color.FromArgb(226, 232, 240));
                    e.Graphics.FillEllipse(brush, pb.Width / 2 - 12, pb.Height / 2 - 12, 24, 24);
                    TextRenderer.DrawText(e.Graphics, "📷", new Font("Segoe UI", 10),
                        pb.ClientRectangle, Color.FromArgb(148, 163, 184),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                };
            }
            return pb;
        }

        public static Button ModernBtn(string text, int x, int y, int w, int h, Color bg, Color fg, bool bold = false)
        {
            var b = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, h),
                Font = new Font("Segoe UI", 9, bold ? FontStyle.Bold : FontStyle.Regular),
                BackColor = bg,
                ForeColor = fg,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var path = GetRoundedPath(b.ClientRectangle, 8);
                using var brush = new SolidBrush(b.BackColor);
                e.Graphics.FillPath(brush, path);
                TextRenderer.DrawText(e.Graphics, b.Text, b.Font, b.ClientRectangle, b.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            b.MouseEnter += (s, e) =>
            {
                b.BackColor = ControlPaint.Dark(bg, 0.1f);
                b.Invalidate();
            };
            b.MouseLeave += (s, e) =>
            {
                b.BackColor = bg;
                b.Invalidate();
            };
            return b;
        }

        public static Label CardTitle(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        public static Panel RoundedCard(int w, int h, int radius = 10)
        {
            var card = new Panel
            {
                Size = new Size(w, h),
                BackColor = BgCard
            };
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var path = GetRoundedPath(card.ClientRectangle, radius);
                e.Graphics.FillPath(new SolidBrush(BgCard), path);
                e.Graphics.DrawPath(new Pen(Border, 1), path);
            };
            return card;
        }
    }

    public class CartPanel : Panel, IRefreshable
    {
        private readonly AuthService _authService;
        private readonly AppDbContext _context;
        private readonly OrderService _orderService;
        private Label lblTitle = null!;
        private Label lblCount = null!;
        private Label lblSubTotal = null!;
        private Label lblEmpty = null!;
        private FlowLayoutPanel flowCart = null!;
        private Button btnClearCart = null!;
        private Button btnCheckout = null!;
        private CheckBox cbSelectAll = null!;
        private HashSet<Guid> _selectedProductIds = new HashSet<Guid>();

        public Action<Product>? ProductClicked { get; set; }

        public CartPanel(AuthService authService, AppDbContext context, OrderService orderService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            BackColor = StyleHelper.BgPage;

            lblTitle = StyleHelper.CardTitle("🛒 Корзина", 24, 0);

            lblCount = new Label
            {
                Font = new Font("Segoe UI", 11),
                ForeColor = StyleHelper.TextMuted,
                Location = new Point(26, 38),
                AutoSize = true
            };

            lblEmpty = new Label
            {
                Text = "🛒  Ваша корзина пуста",
                Font = new Font("Segoe UI", 14),
                ForeColor = StyleHelper.TextMuted,
                AutoSize = true,
                Visible = false
            };

            flowCart = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = StyleHelper.BgPage
            };
            flowCart.HandleCreated += (s, e) =>
            {
                flowCart.HorizontalScroll.Enabled = false;
                flowCart.HorizontalScroll.Visible = false;
            };
            flowCart.Layout += (s, e) =>
            {
                int cw = Math.Max(flowCart.ClientSize.Width, flowCart.Width - SystemInformation.VerticalScrollBarWidth);
                if (cw <= 0) return;
                foreach (Control c in flowCart.Controls)
                    if (c is Panel p) p.Width = cw;
            };

            var summaryCard = new Panel
            {
                BackColor = StyleHelper.BgCard,
                Height = 90
            };
            summaryCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var path = StyleHelper.GetRoundedPath(summaryCard.ClientRectangle, 10);
                e.Graphics.FillPath(new SolidBrush(StyleHelper.BgCard), path);
                using var pen = new Pen(StyleHelper.Border, 1);
                e.Graphics.DrawPath(pen, path);
            };

            var lblSubTotalLabel = new Label
            {
                Text = "Сумма заказа",
                Font = new Font("Segoe UI", 11),
                ForeColor = StyleHelper.TextMuted,
                Location = new Point(16, 12),
                AutoSize = true
            };

            lblSubTotal = new Label
            {
                Text = "0 ₽",
                Font = new Font("Segoe UI", 11),
                ForeColor = StyleHelper.TextPrimary,
                Location = new Point(16, 36),
                AutoSize = true
            };

            cbSelectAll = new CheckBox
            {
                Text = "Выбрать все",
                Font = new Font("Segoe UI", 9),
                ForeColor = StyleHelper.TextMuted,
                Location = new Point(16, 54),
                AutoSize = true,
                Checked = true
            };
            cbSelectAll.CheckedChanged += (s, e) =>
            {
                _selectedProductIds.Clear();
                if (cbSelectAll.Checked)
                {
                    foreach (Control c in flowCart.Controls)
                        if (c.Tag is Guid pid)
                            _selectedProductIds.Add(pid);
                }
                foreach (Control c in flowCart.Controls)
                    if (c.Controls[0] is CheckBox cb)
                        cb.Checked = cbSelectAll.Checked;
                UpdateSelectedTotal();
            };

            btnCheckout = StyleHelper.ModernBtn("✓ Оформить заказ", 0, 0, 200, 44, StyleHelper.Success, Color.White, true);
            btnCheckout.Click += (s, e) => Checkout();

            btnClearCart = new Button
            {
                Text = "🗑",
                Size = new Size(44, 44),
                Font = new Font("Segoe UI", 14),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = StyleHelper.TextMuted,
                Cursor = Cursors.Hand
            };
            btnClearCart.FlatAppearance.BorderSize = 0;
            btnClearCart.MouseEnter += (s, e) => btnClearCart.ForeColor = StyleHelper.Danger;
            btnClearCart.MouseLeave += (s, e) => btnClearCart.ForeColor = StyleHelper.TextMuted;
            btnClearCart.Click += (s, e) => ClearCart();

            summaryCard.Controls.AddRange(new Control[] { lblSubTotalLabel, lblSubTotal, btnCheckout, btnClearCart });
            Controls.AddRange(new Control[] { lblTitle, lblCount, cbSelectAll, flowCart, lblEmpty, summaryCard });
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            const int margin = 24;
            cbSelectAll.Location = new Point(margin, 60);
            flowCart.Location = new Point(margin, 84);
            flowCart.Size = new Size(Width - margin * 2, Height - 249);

            int scMargin = margin;
            if (lblEmpty != null)
                lblEmpty.Location = new Point(margin + 15, Math.Max(80, Height / 2 - 20));

            var summaryCard = Controls[^1] as Panel;
            if (summaryCard != null)
            {
                summaryCard.Location = new Point(scMargin, Height - 140);
                summaryCard.Width = Width - scMargin * 2;

                foreach (Control c in summaryCard.Controls)
                {
                    if (c == btnClearCart)
                        c.Location = new Point(summaryCard.Width - 56, 18);
                    else if (c == btnCheckout)
                        c.Location = new Point(summaryCard.Width - 268, 18);
                }
            }
        }

        public new void Refresh()
        {
            flowCart.Controls.Clear();
            _selectedProductIds.Clear();
            if (cbSelectAll != null) cbSelectAll.Checked = true;
            var user = _authService.CurrentUser;
            if (user == null) { lblEmpty.Visible = true; lblCount.Text = ""; UpdateTotals(0, 0, lblSubTotal, lblCount); return; }

            try
            {
                var cartRepo = new CartRepository(_context);
                var items = cartRepo.GetByUser(user.Id);

                if (!items.Any())
                {
                    lblEmpty.Visible = true;
                    lblCount.Text = "";
                    UpdateTotals(0, 0, lblSubTotal, lblCount);
                    return;
                }
                lblEmpty.Visible = false;

                int totalCount = items.Sum(i => i.Count);
                lblCount.Text = $"{items.Count} {GetItemWord(items.Count)} ({totalCount} шт.)";

                decimal total = 0;
                foreach (var item in items)
                {
                    var product = _context.products.Find(item.ProductId);
                    if (product == null) continue;
                    var sum = product.Price * item.Count;
                    total += sum;
                    var card = MakeCartCard(product, item, cartRepo);
                    flowCart.Controls.Add(card);
                    _selectedProductIds.Add(product.Id);
                }
                UpdateTotals(items.Count, total, lblSubTotal, lblCount);

                BeginInvoke(new Action(() =>
                {
                    int cw = flowCart.ClientSize.Width;
                    if (cw > 0)
                        foreach (Control c in flowCart.Controls)
                            if (c is Panel p) p.Width = cw;
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки корзины: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string GetItemWord(int count)
        {
            if (count >= 2 && count <= 4) return "товара";
            if (count >= 5 || count == 0) return "товаров";
            return "товар";
        }

        private void UpdateSelectedTotal()
        {
            var user = _authService.CurrentUser;
            if (user == null) return;
            var cartRepo = new CartRepository(_context);
            var items = cartRepo.GetByUser(user.Id);
            int count = 0;
            decimal total = 0;
            foreach (var item in items)
            {
                if (!_selectedProductIds.Contains(item.ProductId)) continue;
                var product = _context.products.Find(item.ProductId);
                if (product == null) continue;
                count += item.Count;
                total += product.Price * item.Count;
            }
            UpdateTotals(count, total, lblSubTotal, null);
        }

        private static void UpdateTotals(int itemCount, decimal total, Label lblSubTotal, Label? lblCount)
        {
            if (lblCount != null)
                lblCount.Text = $"{itemCount} {GetItemWord(itemCount)}";
            lblSubTotal.Text = $"{total:N0} ₽";
        }

        private Panel MakeCartCard(Product product, Cart item, CartRepository cartRepo)
        {
            var card = StyleHelper.RoundedCard(300, 110);
            card.Tag = product.Id;
            card.ParentChanged += (s, e) =>
            {
                if (card.Parent is FlowLayoutPanel flp && flp.ClientSize.Width > 0)
                {
                    card.Width = flp.ClientSize.Width;
                    if (cbSelectAll != null && cbSelectAll.Checked)
                        _selectedProductIds.Add(product.Id);
                }
            };

            var cb = new CheckBox
            {
                Location = new Point(8, 46),
                Size = new Size(16, 16),
                Checked = _selectedProductIds.Contains(product.Id) || cbSelectAll?.Checked == true
            };
            cb.CheckedChanged += (s, e) =>
            {
                if (cb.Checked)
                    _selectedProductIds.Add(product.Id);
                else
                    _selectedProductIds.Remove(product.Id);
                UpdateSelectedTotal();
            };

            var pb = StyleHelper.ProductImageBox(product.ProductImage, 76, 76);
            pb.Location = new Point(30, 17);
            pb.Cursor = Cursors.Hand;
            StyleHelper.SetRoundedRegion(pb, 10);
            pb.Click += (s, e) => ProductClicked?.Invoke(product);

            var lblName = new Label
            {
                Text = product.Name,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = StyleHelper.TextPrimary,
                Location = new Point(116, 16),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            lblName.Click += (s, e) => ProductClicked?.Invoke(product);

            var lblUnitPrice = new Label
            {
                Text = $"{product.Price:N0} ₽ / шт.",
                Font = new Font("Segoe UI", 9),
                ForeColor = StyleHelper.TextMuted,
                Location = new Point(116, 44),
                AutoSize = true
            };

            var lineTotal = product.Price * item.Count;
            var lblLineTotal = new Label
            {
                Text = $"{lineTotal:N0} ₽",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = StyleHelper.Accent,
                Location = new Point(card.Width - 150, 18),
                AutoSize = true
            };

            var btnMinus = QtyBtn("-", new Point(116, 66), 28, 28);
            var lblQty = new Label
            {
                Text = item.Count.ToString(),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = StyleHelper.TextPrimary,
                Location = new Point(150, 66),
                Size = new Size(36, 28),
                TextAlign = ContentAlignment.MiddleCenter
            };
            var btnPlus = QtyBtn("+", new Point(192, 66), 28, 28);

            btnMinus.Click += (s, e) =>
            {
                if (item.Count <= 1) { cartRepo.Delete(item.Id); cartRepo.Save(); _selectedProductIds.Remove(product.Id); Refresh(); }
                else { item.Count--; cartRepo.Save(); Refresh(); }
            };
            btnPlus.Click += (s, e) =>
            {
                item.Count++;
                cartRepo.Save();
                Refresh();
            };

            var btnRemove = new Button
            {
                Text = "✕",
                Size = new Size(28, 28),
                Location = new Point(card.Width - 40, 70),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = StyleHelper.TextMuted,
                Font = new Font("Segoe UI", 11),
                Cursor = Cursors.Hand
            };
            btnRemove.FlatAppearance.BorderSize = 0;
            btnRemove.MouseEnter += (s, e) => btnRemove.ForeColor = StyleHelper.Danger;
            btnRemove.MouseLeave += (s, e) => btnRemove.ForeColor = StyleHelper.TextMuted;
            btnRemove.Click += (s, e) =>
            {
                cartRepo.Delete(item.Id);
                cartRepo.Save();
                _selectedProductIds.Remove(product.Id);
                Refresh();
            };

            card.Controls.AddRange(new Control[] { cb, pb, lblName, lblUnitPrice, lblLineTotal, btnRemove, btnMinus, lblQty, btnPlus });
            card.Resize += (s, e) =>
            {
                lblLineTotal.Location = new Point(card.Width - 150, 18);
                btnRemove.Location = new Point(card.Width - 40, 70);
                btnRemove.BringToFront();
            };
            return card;
        }

        private static Button QtyBtn(string text, Point loc, int w, int h)
        {
            var btn = new Button
            {
                Text = text,
                Location = loc,
                Size = new Size(w, h),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = StyleHelper.TextPrimary,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var path = StyleHelper.GetRoundedPath(btn.ClientRectangle, 6);
                using var brush = new SolidBrush(btn.BackColor);
                e.Graphics.FillPath(brush, path);
                TextRenderer.DrawText(e.Graphics, btn.Text, btn.Font, btn.ClientRectangle, btn.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            btn.MouseEnter += (s, e) => { btn.BackColor = Color.FromArgb(226, 232, 240); btn.Invalidate(); };
            btn.MouseLeave += (s, e) => { btn.BackColor = Color.FromArgb(241, 245, 249); btn.Invalidate(); };
            return btn;
        }

    private async void Checkout()
    {
        var user = _authService.CurrentUser;
        if (user == null) return;
        if (_selectedProductIds.Count == 0)
        {
            MessageBox.Show("Выберите товары для заказа", "Корзина");
            return;
        }

        try
        {
            await _orderService.BuyCartAsync();
            MessageBox.Show("Заказ оформлен! Товары перемещены в раздел «Заказы».", "Успех",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            Refresh();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка оформления заказа",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

        private void ClearCart()
        {
            var user = _authService.CurrentUser;
            if (user == null) return;
            var cartRepo = new CartRepository(_context);
            var items = cartRepo.GetByUser(user.Id);
            var toDelete = _selectedProductIds.Count > 0
                ? items.Where(i => _selectedProductIds.Contains(i.ProductId)).ToList()
                : items.ToList();
            if (toDelete.Count == 0) return;
            foreach (var item in toDelete)
            {
                _selectedProductIds.Remove(item.ProductId);
                cartRepo.Delete(item.Id);
            }
            cartRepo.Save();
            Refresh();
        }
    }

    public class FavoritesPanel : Panel, IRefreshable
    {
        private readonly AuthService _authService;
        private readonly AppDbContext _context;
        private Label lblTitle = null!;
        private Label lblEmpty = null!;
        private FlowLayoutPanel flowFavorites = null!;

        public Action<Product>? ProductClicked { get; set; }

        public FavoritesPanel(AuthService authService, AppDbContext context)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            BackColor = StyleHelper.BgPage;

            lblTitle = StyleHelper.CardTitle("❤ Избранное", 24, 0);

            flowFavorites = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                BackColor = StyleHelper.BgPage
            };

            lblEmpty = new Label
            {
                Text = "❤  Здесь пока ничего нет.\nДобавляйте товары в избранное!",
                Font = new Font("Segoe UI", 13),
                ForeColor = StyleHelper.TextMuted,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            Controls.AddRange(new Control[] { lblTitle, flowFavorites, lblEmpty });
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            const int margin = 24;
            flowFavorites.Location = new Point(margin, 55);
            flowFavorites.Size = new Size(Width - margin * 2, Height - 75);
            if (lblEmpty != null)
                lblEmpty.Location = new Point(margin, Math.Max(80, Height / 2 - 30));
        }

        public new void Refresh()
        {
            flowFavorites.Controls.Clear();
            var user = _authService.CurrentUser;
            if (user == null) { lblEmpty.Visible = true; return; }

            try
            {
                var favRepo = new FavoriteRepository(_context);
                var favIds = favRepo.GetByUser(user.Id).Select(f => f.ProductId).ToList();

                if (!favIds.Any()) { lblEmpty.Visible = true; return; }
                lblEmpty.Visible = false;

                foreach (var id in favIds)
                {
                    var product = _context.products.Find(id);
                    if (product == null) continue;
                    flowFavorites.Controls.Add(MakeFavCard(product, favRepo, user.Id));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private Panel MakeFavCard(Product product, FavoriteRepository favRepo, Guid userId)
        {
            var card = StyleHelper.RoundedCard(240, 370, 12);
            card.Margin = new Padding(10);

            var pb = StyleHelper.ProductImageBox(product.ProductImage, 212, 150);
            pb.Location = new Point(14, 10);
            pb.Cursor = Cursors.Hand;
            StyleHelper.SetRoundedRegion(pb, 8);
            pb.Click += (s, e) => ProductClicked?.Invoke(product);

            var lblName = new Label
            {
                Text = product.Name ?? "Без названия",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = StyleHelper.TextPrimary,
                Location = new Point(14, 172),
                Size = new Size(212, 44),
                AutoEllipsis = true,
                Cursor = Cursors.Hand
            };
            lblName.Click += (s, e) => ProductClicked?.Invoke(product);

            var lblPrice = new Label
            {
                Text = $"{product.Price:N0} ₽",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = StyleHelper.Accent,
                Location = new Point(14, 220),
                AutoSize = true
            };

            var lblCategory = new Label
            {
                Text = product.Category ?? "",
                Font = new Font("Segoe UI", 9),
                ForeColor = StyleHelper.TextMuted,
                Location = new Point(14, 248),
                AutoSize = true
            };

            var btnRemove = new Button
            {
                Text = "✕ Убрать",
                Location = new Point(14, 295),
                Size = new Size(106, 36),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.Transparent,
                ForeColor = StyleHelper.Danger,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRemove.FlatAppearance.BorderSize = 1;
            btnRemove.FlatAppearance.BorderColor = StyleHelper.Danger;
            btnRemove.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var path = StyleHelper.GetRoundedPath(btnRemove.ClientRectangle, 6);
                using var pen = new Pen(btnRemove.ForeColor, 1);
                e.Graphics.DrawPath(pen, path);
                TextRenderer.DrawText(e.Graphics, btnRemove.Text, btnRemove.Font,
                    btnRemove.ClientRectangle, btnRemove.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            btnRemove.MouseEnter += (s, e) => { btnRemove.BackColor = Color.FromArgb(254, 242, 242); btnRemove.Invalidate(); };
            btnRemove.MouseLeave += (s, e) => { btnRemove.BackColor = Color.Transparent; btnRemove.Invalidate(); };
            btnRemove.Click += (s, e) =>
            {
                var item = favRepo.GetFavoriteItem(userId, product.Id);
                if (item != null) { favRepo.Delete(item.Id); favRepo.Save(); }
                Refresh();
            };

            var btnCart = new Button
            {
                Text = "🛒 В корзину",
                Location = new Point(130, 295),
                Size = new Size(106, 36),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = StyleHelper.Accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCart.FlatAppearance.BorderSize = 0;
            btnCart.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var path = StyleHelper.GetRoundedPath(btnCart.ClientRectangle, 6);
                using var brush = new SolidBrush(btnCart.BackColor);
                e.Graphics.FillPath(brush, path);
                TextRenderer.DrawText(e.Graphics, btnCart.Text, btnCart.Font,
                    btnCart.ClientRectangle, btnCart.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            btnCart.MouseEnter += (s, e) => { btnCart.BackColor = ControlPaint.Dark(StyleHelper.Accent, 0.1f); btnCart.Invalidate(); };
            btnCart.MouseLeave += (s, e) => { btnCart.BackColor = StyleHelper.Accent; btnCart.Invalidate(); };
            btnCart.Click += (s, e) =>
            {
                var cartRepo = new CartRepository(_context);
                var existing = cartRepo.GetCartItem(userId, product.Id);
                if (existing != null) existing.Count++;
                else cartRepo.Add(new Cart { Id = Guid.NewGuid(), UserId = userId, ProductId = product.Id, Count = 1 });
                cartRepo.Save();
                MessageBox.Show($"«{product.Name}» добавлен в корзину!", "Корзина",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            card.Controls.AddRange(new Control[] { pb, lblName, lblPrice, btnRemove, btnCart });
            return card;
        }
    }

    public class StatisticPanel : Panel, IRefreshable
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;
        private Label lblTitle = null!;
        private ComboBox cmbPeriod = null!;
        private FlowLayoutPanel flowMetrics = null!;
        private Panel pnlTopProducts = null!;
        private Panel pnlCategories = null!;
        private Panel pnlRecentOrders = null!;

        public StatisticPanel(AppDbContext context, AuthService authService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            BackColor = StyleHelper.BgPage;
            AutoScroll = true;

            lblTitle = StyleHelper.CardTitle("📊 Статистика", 24, 0);

            cmbPeriod = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            cmbPeriod.Items.AddRange(new[] { "Все время", "Сегодня", "Эта неделя", "Этот месяц" });
            cmbPeriod.SelectedIndex = 0;
            cmbPeriod.SelectedIndexChanged += (s, e) => Refresh();

            flowMetrics = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoSize = true,
                BackColor = StyleHelper.BgPage,
                Location = new Point(24, 50)
            };

            pnlTopProducts = MakeSectionCard();
            pnlCategories = MakeSectionCard();
            pnlRecentOrders = MakeSectionCard();

            Controls.AddRange(new Control[] { lblTitle, cmbPeriod, flowMetrics, pnlTopProducts, pnlCategories, pnlRecentOrders });
        }

        private static Panel MakeSectionCard()
        {
            var pnl = new Panel
            {
                BackColor = StyleHelper.BgCard,
                AutoSize = true
            };
            pnl.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, pnl.Width - 1, pnl.Height - 1);
                using var path = StyleHelper.GetRoundedPath(rect, 10);
                e.Graphics.FillPath(new SolidBrush(StyleHelper.BgCard), path);
                using var pen = new Pen(StyleHelper.Border, 1);
                e.Graphics.DrawPath(pen, path);
            };
            return pnl;
        }

        private static Label SectionTitle(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = StyleHelper.TextPrimary,
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        private static Label SectionLine(string text, int x, int y, Color? color = null)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                ForeColor = color ?? StyleHelper.TextPrimary,
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (cmbPeriod == null) return;

            const int margin = 24;
            int w = Width - margin * 2;

            int titleW = TextRenderer.MeasureText(lblTitle.Text, lblTitle.Font).Width;
            lblTitle.Location = new Point(margin, 0);

            cmbPeriod.Location = new Point(Math.Max(margin + titleW + 16, Width - margin - 140), 4);
            cmbPeriod.Size = new Size(140, 24);

            flowMetrics.Location = new Point(margin, 50);
            flowMetrics.MaximumSize = new Size(w, 0);

            int sectionY = flowMetrics.Bottom + 20;
            foreach (var c in Controls)
            {
                if (c is Panel pnl && pnl != flowMetrics && pnl.AutoSize)
                {
                    pnl.Location = new Point(margin, sectionY);
                    pnl.Width = w;
                    sectionY = pnl.Bottom + 16;
                }
            }
        }

        private DateTime? GetPeriodStart()
        {
            return cmbPeriod.SelectedIndex switch
            {
                1 => DateTime.Today,
                2 => DateTime.Today.AddDays(-7),
                3 => DateTime.Today.AddDays(-30),
                _ => null
            };
        }

        public new void Refresh()
        {
            flowMetrics.Controls.Clear();
            pnlTopProducts.Controls.Clear();
            pnlCategories.Controls.Clear();
            pnlRecentOrders.Controls.Clear();

            try
            {
                var periodStart = GetPeriodStart();
                var filteredOrders = _context.orders.AsEnumerable();
                if (periodStart.HasValue)
                    filteredOrders = filteredOrders.Where(o => o.CreatedAt >= periodStart.Value);

                var ordersList = filteredOrders.ToList();

                int totalUsers = _context.users.Count();
                int totalProducts = _context.products.Count(p => p.IsApproved);
                int totalOrders = ordersList.Count;
                decimal totalRevenue = ordersList.Sum(o => o.Price * o.Count);

                AddMetricCard("👥 Пользователи", totalUsers.ToString("N0"), StyleHelper.Accent);
                AddMetricCard("📦 Товары", totalProducts.ToString("N0"), StyleHelper.Accent);
                AddMetricCard("📋 Заказы", $"{totalOrders}", StyleHelper.Success);
                AddMetricCard("💰 Выручка", $"{totalRevenue:N0} ₽", StyleHelper.Accent);

                BuildTopProducts(ordersList);
                BuildCategories(ordersList);
                BuildRecentOrders(ordersList);

                OnResize(EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BuildTopProducts(List<Order> orders)
        {
            var top = orders
                .GroupBy(o => o.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Count = g.Sum(o => o.Count),
                    Revenue = g.Sum(o => o.Price * o.Count)
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            int y = 16;
            pnlTopProducts.Controls.Add(SectionTitle("🏆 Топ товаров", 16, y));
            y += 34;

            if (top.Count == 0)
            {
                pnlTopProducts.Controls.Add(SectionLine("Нет данных за выбранный период", 16, y, StyleHelper.TextMuted));
            }
            else
            {
                var maxCount = top.Max(x => x.Count);
                foreach (var item in top)
                {
                    var product = _context.products.Find(item.ProductId);
                    var name = product?.Name ?? "Неизвестно";

                    var line = new Label
                    {
                        Text = $"{name} — {item.Count} шт. — {item.Revenue:N0} ₽",
                        Font = new Font("Segoe UI", 10),
                        ForeColor = StyleHelper.TextPrimary,
                        Location = new Point(16, y),
                        AutoSize = true
                    };
                    pnlTopProducts.Controls.Add(line);

                    var barWidth = (int)((float)item.Count / maxCount * 200);
                    var bar = new Panel
                    {
                        Size = new Size(barWidth, 6),
                        Location = new Point(16, y + 20),
                        BackColor = StyleHelper.Accent
                    };
                    bar.Paint += (s, e) =>
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        using var path = StyleHelper.GetRoundedPath(bar.ClientRectangle, 3);
                        e.Graphics.FillPath(new SolidBrush(StyleHelper.Accent), path);
                    };
                    pnlTopProducts.Controls.Add(bar);

                    y += 32;
                }
            }

            y += 8;
            pnlTopProducts.Height = y + 16;
        }

        private void BuildCategories(List<Order> orders)
        {
            var catData = orders
                .GroupBy(o => o.ProductId)
                .Select(g =>
                {
                    var product = _context.products.Find(g.Key);
                    return new { Category = product?.Category ?? "Без категории", Count = g.Sum(o => o.Count) };
                })
                .GroupBy(x => x.Category)
                .Select(g => new { Category = g.Key, Count = g.Sum(x => x.Count) })
                .OrderByDescending(x => x.Count)
                .ToList();

            int y = 16;
            pnlCategories.Controls.Add(SectionTitle("📂 По категориям", 16, y));
            y += 34;

            if (catData.Count == 0)
            {
                pnlCategories.Controls.Add(SectionLine("Нет данных", 16, y, StyleHelper.TextMuted));
            }
            else
            {
                var maxCat = catData.Max(x => x.Count);
                foreach (var cat in catData)
                {
                    var line = SectionLine($"{cat.Category}: {cat.Count} шт.", 16, y);
                    pnlCategories.Controls.Add(line);

                    var barWidth = (int)((float)cat.Count / maxCat * 200);
                    var bar = new Panel
                    {
                        Size = new Size(Math.Max(barWidth, 4), 6),
                        Location = new Point(16, y + 20),
                        BackColor = StyleHelper.Success
                    };
                    bar.Paint += (s, e) =>
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        using var path = StyleHelper.GetRoundedPath(bar.ClientRectangle, 3);
                        e.Graphics.FillPath(new SolidBrush(StyleHelper.Success), path);
                    };
                    pnlCategories.Controls.Add(bar);

                    y += 32;
                }
            }

            y += 8;
            pnlCategories.Height = y + 16;
        }

        private void BuildRecentOrders(List<Order> orders)
        {
            int y = 16;
            pnlRecentOrders.Controls.Add(SectionTitle("📄 Последние заказы", 16, y));
            y += 34;

            var recent = orders.OrderByDescending(o => o.CreatedAt).Take(10).ToList();

            if (recent.Count == 0)
            {
                pnlRecentOrders.Controls.Add(SectionLine("Нет заказов", 16, y, StyleHelper.TextMuted));
            }
            else
            {
                var header = new Label
                {
                    Text = "Дата и время           Покупатель          Товар                          Кол-во   Сумма",
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = StyleHelper.TextMuted,
                    Location = new Point(16, y),
                    AutoSize = true
                };
                pnlRecentOrders.Controls.Add(header);
                y += 24;

                foreach (var order in recent)
                {
                    var product = _context.products.Find(order.ProductId);
                    var pName = product?.Name ?? "Неизвестно";
                    var user = _context.users.Find(order.UserId);
                    var uName = user?.Name ?? "Неизвестно";
                    var amount = order.Price * order.Count;

                    var line = new Label
                    {
                        Text = $"{order.CreatedAt:dd.MM.yy HH:mm}  {uName,-18} {pName,-30} ×{order.Count,-4} {amount:N0} ₽",
                        Font = new Font("Segoe UI", 9),
                        ForeColor = StyleHelper.TextPrimary,
                        Location = new Point(16, y),
                        AutoSize = true
                    };
                    pnlRecentOrders.Controls.Add(line);

                    var statusDot = new Panel
                    {
                        Size = new Size(10, 10),
                        Location = new Point(pnlRecentOrders.Width - 30, y + 3),
                        BackColor = StyleHelper.Success
                    };
                    statusDot.Paint += (s, e) =>
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        e.Graphics.FillEllipse(new SolidBrush(StyleHelper.Success), 0, 0, 9, 9);
                    };
                    pnlRecentOrders.Controls.Add(statusDot);

                    y += 22;
                }
            }

            y += 8;
            pnlRecentOrders.Height = y + 16;
        }

        private void AddMetricCard(string label, string value, Color accent)
        {
            var card = new Panel
            {
                Size = new Size(200, 90),
                BackColor = StyleHelper.BgCard,
                Margin = new Padding(0, 0, 12, 12)
            };
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = StyleHelper.GetRoundedPath(card.ClientRectangle, 10);
                e.Graphics.FillPath(new SolidBrush(StyleHelper.BgCard), path);
                using var pen = new Pen(StyleHelper.Border, 1);
                e.Graphics.DrawPath(pen, path);
                using var accentPen = new Pen(accent, 3);
                var topPath = StyleHelper.GetRoundedPath(new Rectangle(0, -3, card.Width, 6), 3);
                e.Graphics.DrawPath(accentPen, topPath);
            };

            var lblVal = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = accent,
                Location = new Point(16, 14),
                AutoSize = true
            };
            var lblLbl = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 10),
                ForeColor = StyleHelper.TextMuted,
                Location = new Point(16, 52),
                AutoSize = true
            };

            card.Controls.AddRange(new Control[] { lblVal, lblLbl });
            flowMetrics.Controls.Add(card);
        }
    }

    public class ProfilePanel : Panel, IRefreshable
    {
        private readonly AuthService _authService;
        private readonly AppDbContext _context;
        private Label lblTitle = null!;
        private Label lblNameVal = null!;
        private Label lblEmailVal = null!;
        private Label lblRoleVal = null!;
        private Label lblBalanceVal = null!;
        private Label lblBalanceHint = null!;
        private Button btnEditName = null!;
        private Button btnChangePassword = null!;
        private Button btnUploadPhoto = null!;
        private PictureBox pbAvatar = null!;
        private const int CardX = 24;
        private const int CardY = 55;
        private const int CardW = 520;
        private const int CardH = 330;

        public ProfilePanel(AuthService authService, AppDbContext context)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            BackColor = StyleHelper.BgPage;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            lblTitle = StyleHelper.CardTitle("👤 Личный кабинет", 24, 0);

            int ax = CardX + 24;
            int ay = CardY + 24;

            pbAvatar = new PictureBox
            {
                Size = new Size(120, 120),
                Location = new Point(ax, ay),
                BackColor = Color.FromArgb(241, 245, 249),
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand
            };
            pbAvatar.Paint += Avatar_Paint;
            pbAvatar.Click += (s, e) => UploadPhoto();

            btnUploadPhoto = new Button
            {
                Text = "📷 Загрузить фото",
                Location = new Point(ax, ay + 130),
                Size = new Size(150, 28),
                Font = new Font("Segoe UI", 8),
                FlatStyle = FlatStyle.Flat,
                ForeColor = StyleHelper.Accent,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft
            };
            btnUploadPhoto.FlatAppearance.BorderSize = 0;
            btnUploadPhoto.MouseEnter += (s, e) => btnUploadPhoto.ForeColor = ControlPaint.Dark(StyleHelper.Accent, 0.2f);
            btnUploadPhoto.MouseLeave += (s, e) => btnUploadPhoto.ForeColor = StyleHelper.Accent;
            btnUploadPhoto.Click += (s, e) => UploadPhoto();

            int fx = CardX + 170;
            int fy = CardY + 24;

            lblNameVal = CreateFieldLabel(fx, fy + 20);
            lblEmailVal = CreateFieldLabel(fx, fy + 68);
            lblRoleVal = CreateFieldLabel(fx, fy + 116);

            var lblName = CreateFieldCaption("Имя", fx, fy);
            var lblEmail = CreateFieldCaption("Email", fx, fy + 48);
            var lblRole = CreateFieldCaption("Роль", fx, fy + 96);

            lblBalanceVal = CreateFieldLabel(fx + 170, fy + 20);
            lblBalanceHint = new Label
            {
                Text = "Средства для покупок",
                Font = new Font("Segoe UI", 8),
                ForeColor = StyleHelper.TextMuted,
                Location = new Point(fx + 170, fy + 44),
                AutoSize = true
            };

            int btnY = CardY + CardH - 80;
            btnEditName = StyleHelper.ModernBtn("✏ Изменить имя", fx, btnY, 150, 38, StyleHelper.Accent, Color.White);
            btnEditName.Click += BtnEditName_Click;

            btnChangePassword = StyleHelper.ModernBtn("🔒 Сменить пароль", fx + 165, btnY, 150, 38, Color.FromArgb(100, 116, 139), Color.White);
            btnChangePassword.Click += BtnChangePassword_Click;

            Controls.AddRange(new Control[] {
                lblTitle, pbAvatar, btnUploadPhoto,
                btnEditName, btnChangePassword, lblBalanceHint,
                lblName, lblEmail, lblRole,
                lblNameVal, lblEmailVal, lblRoleVal, lblBalanceVal
            });
        }

        private void Avatar_Paint(object? sender, PaintEventArgs e)
        {
            var pb = (PictureBox)sender!;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using var path = new GraphicsPath();
            path.AddEllipse(0, 0, pb.Width - 1, pb.Height - 1);

            var user = _authService.CurrentUser;
            if (user?.Avatar != null && user.Avatar.Length > 0)
            {
                try
                {
                    using var ms = new MemoryStream(user.Avatar);
                    using var img = Image.FromStream(ms);
                    e.Graphics.SetClip(path);
                    e.Graphics.DrawImage(img, 0, 0, pb.Width, pb.Height);
                    e.Graphics.ResetClip();
                }
                catch
                {
                    DrawAvatarPlaceholder(e, pb);
                }
            }
            else
            {
                DrawAvatarPlaceholder(e, pb);
            }

            using var pen = new Pen(StyleHelper.Border, 2);
            e.Graphics.DrawEllipse(pen, 0, 0, pb.Width - 1, pb.Height - 1);
        }

        private static void DrawAvatarPlaceholder(PaintEventArgs e, PictureBox pb)
        {
            e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(226, 232, 240)), 0, 0, pb.Width - 1, pb.Height - 1);
            TextRenderer.DrawText(e.Graphics, "📷", new Font("Segoe UI", 24),
                pb.ClientRectangle, Color.FromArgb(148, 163, 184),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private void UploadPhoto()
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                Title = "Выберите фото"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var user = _authService.CurrentUser;
                    if (user == null) return;

                    user.Avatar = File.ReadAllBytes(dlg.FileName);
                    var userRepo = new UserRepository(_context);
                    userRepo.Update(user);
                    _context.SaveChanges();

                    pbAvatar.Invalidate();
                    MessageBox.Show("Фото сохранено!", "Готово",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки фото: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private static Label CreateFieldCaption(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9),
                ForeColor = StyleHelper.TextMuted,
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        private static Label CreateFieldLabel(int x, int y)
        {
            return new Label
            {
                Text = "—",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = StyleHelper.TextPrimary,
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using var shadowPen = new Pen(Color.FromArgb(20, 0, 0, 0), 6);
            var shadowRect = new Rectangle(CardX + 3, CardY + 3, CardW, CardH);
            var shadowPath = StyleHelper.GetRoundedPath(shadowRect, 14);
            e.Graphics.DrawPath(shadowPen, shadowPath);

            var path = StyleHelper.GetRoundedPath(new Rectangle(CardX, CardY, CardW, CardH), 14);
            e.Graphics.FillPath(new SolidBrush(StyleHelper.BgCard), path);
            e.Graphics.DrawPath(new Pen(StyleHelper.Border, 1), path);
        }

        public new void Refresh()
        {
            var user = _authService.CurrentUser;
            if (user == null) return;

            lblNameVal.Text = user.Name;
            lblEmailVal.Text = user.Email;
            lblRoleVal.Text = user.Role.ToString();
            lblBalanceVal.Text = $"{user.Balance:N0} ₽";
            pbAvatar.Invalidate();
        }

        private void BtnEditName_Click(object? sender, EventArgs e)
        {
            var user = _authService.CurrentUser;
            if (user == null) return;

            var input = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите новое имя:", "Изменить имя", user.Name);

            if (!string.IsNullOrWhiteSpace(input) && input != user.Name)
            {
                user.Name = input;
                lblNameVal.Text = input;
                MessageBox.Show("Имя обновлено!", "Готово",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnChangePassword_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Функция смены пароля будет добавлена.\n(Подключить UserService.ChangePassword)",
                "В разработке", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    public class OrdersPanel : Panel, IRefreshable
    {
        private readonly AuthService _authService;
        private readonly AppDbContext _context;
        private readonly ProductService _productService;
        private readonly OrderService _orderService;
        private readonly IRefundService _refundService;
        private Label lblTitle = null!;
        private Label lblEmpty = null!;
        private FlowLayoutPanel flowOrders = null!;

        public Action<Product>? ProductClicked { get; set; }

        public OrdersPanel(AuthService authService, AppDbContext context,
            ProductService productService, OrderService orderService, IRefundService refundService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _refundService = refundService ?? throw new ArgumentNullException(nameof(refundService));
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            BackColor = StyleHelper.BgPage;

            lblTitle = StyleHelper.CardTitle("📦 Мои заказы", 24, 0);

            flowOrders = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = StyleHelper.BgPage
            };
            flowOrders.HandleCreated += (s, e) =>
            {
                flowOrders.HorizontalScroll.Enabled = false;
                flowOrders.HorizontalScroll.Visible = false;
            };
            flowOrders.Layout += (s, e) =>
            {
                int cw = Math.Max(flowOrders.ClientSize.Width, flowOrders.Width - SystemInformation.VerticalScrollBarWidth);
                if (cw <= 0) return;
                foreach (Control c in flowOrders.Controls)
                    if (c is Panel p) p.Width = cw;
            };

            lblEmpty = new Label
            {
                Text = "📦  У вас пока нет заказов",
                Font = new Font("Segoe UI", 14),
                ForeColor = StyleHelper.TextMuted,
                AutoSize = true,
                Visible = false
            };

            Controls.AddRange(new Control[] { lblTitle, flowOrders, lblEmpty });
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            const int margin = 24;
            flowOrders.Location = new Point(margin, 55);
            flowOrders.Size = new Size(Width - margin * 2, Height - 75);
            if (lblEmpty != null)
                lblEmpty.Location = new Point(margin, Math.Max(80, Height / 2 - 20));
        }

        public new void Refresh()
        {
            flowOrders.Controls.Clear();
            var user = _authService.CurrentUser;
            if (user == null) { lblEmpty.Visible = true; return; }

            try
            {
                var orders = _orderService.GetUserOrders(user.Id);
                var refunds = _refundService.GetUserRequests(user.Id);

                if (!orders.Any() && !refunds.Any()) { lblEmpty.Visible = true; return; }
                lblEmpty.Visible = false;

                foreach (var order in orders)
                {
                    var product = _context.products.Find(order.ProductId);
                    if (product == null) continue;
                    order.Product = product;
                    var existingRefund = refunds.FirstOrDefault(r => r.OrderId == order.Id);
                    flowOrders.Controls.Add(MakeOrderCard(order, product, user.Id, existingRefund));
                }

                if (refunds.Any())
                {
                    flowOrders.Controls.Add(new Label
                    {
                        Text = "↩ Запросы на возврат",
                        Font = new Font("Segoe UI", 13, FontStyle.Bold),
                        ForeColor = StyleHelper.TextPrimary,
                        AutoSize = true,
                        Margin = new Padding(0, 16, 0, 4)
                    });

                    foreach (var req in refunds)
                    {
                        var product = _context.products.Find(req.ProductId);
                        flowOrders.Controls.Add(MakeRefundCard(req, product));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

    private Panel MakeOrderCard(Order order, Product product, Guid userId, RefundRequest? existingRefund)
    {
        var card = StyleHelper.RoundedCard(300, 100);
        card.Margin = new Padding(0, 0, 0, 8);

        var pb = StyleHelper.ProductImageBox(product.ProductImage, 72, 72);
        pb.Location = new Point(14, 14);
        pb.Cursor = Cursors.Hand;
        StyleHelper.SetRoundedRegion(pb, 8);
        pb.Click += (s, e) => ProductClicked?.Invoke(product);

        var lblName = new Label
        {
            Text = product.Name ?? "Без названия",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = StyleHelper.TextPrimary,
            Location = new Point(100, 14),
            AutoSize = true,
            Cursor = Cursors.Hand
        };
        lblName.Click += (s, e) => ProductClicked?.Invoke(product);

        var lblMeta = new Label
        {
            Text = $"{order.Count} шт. × {order.Price / order.Count:N0} ₽ = {order.Price:N0} ₽",
            Font = new Font("Segoe UI", 9),
            ForeColor = StyleHelper.TextMuted,
            Location = new Point(100, 40),
            AutoSize = true
        };

        var lblDate = new Label
        {
            Text = order.CreatedAt.ToString("dd.MM.yyyy"),
            Font = new Font("Segoe UI", 9),
            ForeColor = StyleHelper.TextMuted,
            Location = new Point(100, 58),
            AutoSize = true
        };

        card.Controls.AddRange(new Control[] { pb, lblName, lblMeta, lblDate });

        if (existingRefund != null)
        {
            var lblRefundStatus = new Label
            {
                Text = existingRefund.Status switch
                {
                    RefundStatus.Pending => "⏳ Возврат ожидает",
                    RefundStatus.Approved => "✅ Возврат одобрен",
                    RefundStatus.Declined => "❌ Возврат отклонён",
                    _ => existingRefund.Status.ToString()
                },
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = existingRefund.Status switch
                {
                    RefundStatus.Pending => Color.FromArgb(194, 65, 12),
                    RefundStatus.Approved => StyleHelper.Success,
                    RefundStatus.Declined => StyleHelper.Danger,
                    _ => StyleHelper.TextMuted
                },
                AutoSize = true
            };
            card.Controls.Add(lblRefundStatus);

            card.Resize += (s, e) =>
            {
                lblRefundStatus.Location = new Point(card.Width - 170, 38);
            };
            lblRefundStatus.Location = new Point(card.Width - 170, 38);
        }
        else
        {
            var btnRefund = new Button
            {
                Text = "↩ Возврат",
                Size = new Size(100, 34),
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(255, 237, 213),
                ForeColor = Color.FromArgb(194, 65, 12),
                Cursor = Cursors.Hand
            };
            btnRefund.FlatAppearance.BorderSize = 0;
            btnRefund.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = StyleHelper.GetRoundedPath(btnRefund.ClientRectangle, 8);
                using var brush = new SolidBrush(btnRefund.BackColor);
                e.Graphics.FillPath(brush, path);
                TextRenderer.DrawText(e.Graphics, btnRefund.Text, btnRefund.Font,
                    btnRefund.ClientRectangle, btnRefund.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            btnRefund.MouseEnter += (s, e) =>
            {
                btnRefund.BackColor = Color.FromArgb(254, 215, 170);
                btnRefund.Invalidate();
            };
            btnRefund.MouseLeave += (s, e) =>
            {
                btnRefund.BackColor = Color.FromArgb(255, 237, 213);
                btnRefund.Invalidate();
            };
            btnRefund.Click += (s, e) => RequestRefund(order, product);

            card.Controls.Add(btnRefund);

            card.Resize += (s, e) =>
            {
                btnRefund.Location = new Point(card.Width - 124, 33);
            };
            btnRefund.Location = new Point(card.Width - 124, 33);
        }

        return card;
    }

        private void RequestRefund(Order order, Product product)
        {
            var countStr = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите количество товара для возврата:",
                "Оформление возврата",
                order.Count.ToString());

            if (string.IsNullOrWhiteSpace(countStr)) return;
            if (!int.TryParse(countStr, out int count) || count <= 0 || count > order.Count)
            {
                MessageBox.Show($"Некорректное количество. Можно вернуть от 1 до {order.Count} шт.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var reason = Microsoft.VisualBasic.Interaction.InputBox(
                "Укажите причину возврата:",
                "Причина возврата",
                "");

            if (string.IsNullOrWhiteSpace(reason))
            {
                MessageBox.Show("Укажите причину возврата", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _refundService.CreateRequest(order.Id, count, reason);
                MessageBox.Show($"Запрос на возврат «{product.Name}» ({count} шт.) отправлен!",
                    "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private Panel MakeRefundCard(RefundRequest req, Product? product)
        {
            var card = StyleHelper.RoundedCard(300, 90);
            card.Margin = new Padding(0, 0, 0, 8);
            int x = 20;
            int y = 14;
            var lblProduct = new Label
            {
                Text = product?.Name ?? "Товар удалён",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = StyleHelper.TextPrimary,
                Location = new Point(x, y),
                AutoSize = true
            };
            var lblMeta = new Label
            {
                Text = $"{req.Count} шт. — {req.CreatedAt:dd.MM.yyyy}",
                Font = new Font("Segoe UI", 9),
                ForeColor = StyleHelper.TextMuted,
                Location = new Point(x, y + 24),
                AutoSize = true
            };
            var lblStatus = new Label
            {
                Text = req.Status switch
                {
                    RefundStatus.Pending => "⏳ На рассмотрении",
                    RefundStatus.Approved => "✅ Одобрен",
                    RefundStatus.Declined => "❌ Отклонён",
                    _ => req.Status.ToString()
                },
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = req.Status switch
                {
                    RefundStatus.Pending => Color.FromArgb(194, 65, 12),
                    RefundStatus.Approved => StyleHelper.Success,
                    RefundStatus.Declined => StyleHelper.Danger,
                    _ => StyleHelper.TextMuted
                },
                Location = new Point(x, y + 48),
                AutoSize = true
            };
            card.Controls.AddRange(new Control[] { lblProduct, lblMeta, lblStatus });
            if (!string.IsNullOrEmpty(req.ReviewComment))
            {
                var lblComment = new Label
                {
                    Text = $"Комментарий: {req.ReviewComment}",
                    Font = new Font("Segoe UI", 9, FontStyle.Italic),
                    ForeColor = StyleHelper.TextMuted,
                    Location = new Point(x, y + 68),
                    AutoSize = true
                };
                card.Controls.Add(lblComment);
                card.Height = 110;
            }
            return card;
        }
    }

    public class RefundsPanel : Panel, IRefreshable
    {
        private readonly AuthService _authService;
        private readonly AppDbContext _context;
        private readonly IRefundService _refundService;
        private Label lblTitle = null!;
        private Label lblEmpty = null!;
        private FlowLayoutPanel flowRefunds = null!;

        public RefundsPanel(AuthService authService, AppDbContext context, IRefundService refundService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _refundService = refundService ?? throw new ArgumentNullException(nameof(refundService));
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            BackColor = StyleHelper.BgPage;

            lblTitle = StyleHelper.CardTitle("↩ Мои возвраты", 24, 0);

            flowRefunds = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = StyleHelper.BgPage
            };
            flowRefunds.HandleCreated += (s, e) =>
            {
                flowRefunds.HorizontalScroll.Enabled = false;
                flowRefunds.HorizontalScroll.Visible = false;
            };
            flowRefunds.Layout += (s, e) =>
            {
                int cw = Math.Max(flowRefunds.ClientSize.Width, flowRefunds.Width - SystemInformation.VerticalScrollBarWidth);
                if (cw <= 0) return;
                foreach (Control c in flowRefunds.Controls)
                    if (c is Panel p) p.Width = cw;
            };

            lblEmpty = new Label
            {
                Text = "↩  У вас нет запросов на возврат",
                Font = new Font("Segoe UI", 14),
                ForeColor = StyleHelper.TextMuted,
                AutoSize = true,
                Visible = false
            };

            Controls.AddRange(new Control[] { lblTitle, flowRefunds, lblEmpty });
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            const int margin = 24;
            flowRefunds.Location = new Point(margin, 55);
            flowRefunds.Size = new Size(Width - margin * 2, Height - 75);
            if (lblEmpty != null)
                lblEmpty.Location = new Point(margin, Math.Max(80, Height / 2 - 20));
        }

        public new void Refresh()
        {
            flowRefunds.Controls.Clear();
            var user = _authService.CurrentUser;
            if (user == null) { lblEmpty.Visible = true; return; }

            try
            {
                var requests = _refundService.GetUserRequests(user.Id);
                if (!requests.Any()) { lblEmpty.Visible = true; return; }
                lblEmpty.Visible = false;

                foreach (var req in requests)
                {
                    var product = _context.products.Find(req.ProductId);
                    flowRefunds.Controls.Add(MakeRefundCard(req, product));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки возвратов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private Panel MakeRefundCard(RefundRequest req, Product? product)
        {
            var card = StyleHelper.RoundedCard(300, 90);
            card.Margin = new Padding(0, 0, 0, 8);

            int x = 20;
            int y = 14;

            var lblProduct = new Label
            {
                Text = product?.Name ?? "Товар удалён",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = StyleHelper.TextPrimary,
                Location = new Point(x, y),
                AutoSize = true
            };

            var lblMeta = new Label
            {
                Text = $"{req.Count} шт. — {req.CreatedAt:dd.MM.yyyy}",
                Font = new Font("Segoe UI", 9),
                ForeColor = StyleHelper.TextMuted,
                Location = new Point(x, y + 24),
                AutoSize = true
            };

            var lblStatus = new Label
            {
                Text = req.Status switch
                {
                    RefundStatus.Pending => "⏳ На рассмотрении",
                    RefundStatus.Approved => "✅ Одобрен",
                    RefundStatus.Declined => "❌ Отклонён",
                    _ => req.Status.ToString()
                },
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = req.Status switch
                {
                    RefundStatus.Pending => Color.FromArgb(194, 65, 12),
                    RefundStatus.Approved => StyleHelper.Success,
                    RefundStatus.Declined => StyleHelper.Danger,
                    _ => StyleHelper.TextMuted
                },
                Location = new Point(x, y + 48),
                AutoSize = true
            };

            card.Controls.AddRange(new Control[] { lblProduct, lblMeta, lblStatus });

            if (!string.IsNullOrEmpty(req.ReviewComment))
            {
                var lblComment = new Label
                {
                    Text = $"Комментарий: {req.ReviewComment}",
                    Font = new Font("Segoe UI", 9, FontStyle.Italic),
                    ForeColor = StyleHelper.TextMuted,
                    Location = new Point(x, y + 68),
                    AutoSize = true
                };
                card.Controls.Add(lblComment);
                card.Height = 110;
            }

            return card;
        }
    }

    public class CreateProductPanel : Panel, IRefreshable
    {
        private readonly AuthService _authService;
        private readonly ProductService _productService;
        private readonly AppDbContext _context;

        private TextBox txtName = null!;
        private TextBox txtDescription = null!;
        private TextBox txtPrice = null!;
        private ComboBox cmbCategory = null!;
        private TextBox txtAmount = null!;
        private Button btnSelectImage = null!;
        private Button btnCreate = null!;
        private string? selectedImagePath = null;

        private Panel previewCard = null!;
        private Label previewName = null!;
        private Label previewPrice = null!;
        private Label previewCategory = null!;
        private PictureBox previewImage = null!;
        private Label lblSuccess = null!;

        public CreateProductPanel(AuthService authService, ProductService productService, AppDbContext context)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            BackColor = StyleHelper.BgPage;

            var lblTitle = StyleHelper.CardTitle("➕ Создать товар", 24, 0);

            var card = new Panel
            {
                BackColor = Color.White,
                Location = new Point(24, 55),
                Size = new Size(680, 400)
            };
            StyleHelper.SetRoundedRegion(card, 14);
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                using var path = StyleHelper.GetRoundedPath(rect, 14);
                e.Graphics.FillPath(new SolidBrush(Color.White), path);
                using var pen = new Pen(StyleHelper.Border, 1);
                e.Graphics.DrawPath(pen, path);
            };

            int x = 24;
            int y = 24;

            var fontLabel = new Font("Segoe UI", 9, FontStyle.Bold);
            var fontField = new Font("Segoe UI", 10);
            var colorLabel = StyleHelper.TextMuted;
            int fw = 280;
            int fh = 34;

            Label MakeLabel(string text, int lx, int ly)
            {
                var lbl = new Label { Text = text, Font = fontLabel, ForeColor = colorLabel, Location = new Point(lx, ly), AutoSize = true };
                card.Controls.Add(lbl);
                return lbl;
            }

            TextBox MakeField(int fx, int fy, string placeholder)
            {
                var tb = new TextBox
                {
                    Location = new Point(fx, fy),
                    Size = new Size(fw, fh),
                    Font = fontField,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.FromArgb(250, 251, 252),
                    ForeColor = StyleHelper.TextPrimary,
                    PlaceholderText = placeholder
                };
                card.Controls.Add(tb);
                return tb;
            }

            MakeLabel("Название", x, y + 4);
            txtName = MakeField(x + 90, y, "Введите название товара");
            txtName.TextChanged += UpdatePreview;

            y += 48;
            MakeLabel("Описание", x, y + 4);
            txtDescription = new TextBox
            {
                Location = new Point(x + 90, y),
                Size = new Size(fw, 68),
                Font = fontField,
                Multiline = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 251, 252),
                ForeColor = StyleHelper.TextPrimary,
                PlaceholderText = "Описание (необязательно)",
                ScrollBars = ScrollBars.Vertical,
                AcceptsReturn = true
            };
            card.Controls.Add(txtDescription);

            y += 82;
            MakeLabel("Цена", x, y + 4);
            txtPrice = MakeField(x + 90, y, "0.00");
            txtPrice.TextChanged += UpdatePreview;

            y += 48;
            MakeLabel("Категория", x, y + 4);
            cmbCategory = new ComboBox
            {
                Location = new Point(x + 90, y),
                Size = new Size(fw, fh),
                Font = fontField,
                DropDownStyle = ComboBoxStyle.DropDown,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(250, 251, 252),
                ForeColor = StyleHelper.TextPrimary
            };
            cmbCategory.Items.AddRange(new[] { "Электроника", "Одежда", "Продукты", "Книги", "Спорт", "Мебель", "Игрушки", "Другое" });
            cmbCategory.SelectedIndex = -1;
            cmbCategory.TextChanged += UpdatePreview;
            card.Controls.Add(cmbCategory);

            y += 48;
            MakeLabel("Количество", x, y + 4);
            txtAmount = MakeField(x + 90, y, "0");

            int previewX = x + 90 + fw + 32;
            previewCard = new Panel
            {
                Location = new Point(previewX, 20),
                Size = new Size(200, 290),
                BackColor = Color.White
            };
            StyleHelper.SetRoundedRegion(previewCard, 10);
            previewCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, previewCard.Width - 1, previewCard.Height - 1);
                using var path = StyleHelper.GetRoundedPath(r, 10);
                e.Graphics.FillPath(new SolidBrush(Color.White), path);
                using var pen = new Pen(StyleHelper.Border, 1);
                e.Graphics.DrawPath(pen, path);
                using var shadow = new SolidBrush(Color.FromArgb(6, 0, 0, 0));
                e.Graphics.FillRectangle(shadow, 6, 2, previewCard.Width - 12, 4);
            };

            previewImage = new PictureBox
            {
                Size = new Size(176, 120),
                Location = new Point(12, 10),
                BackColor = Color.FromArgb(241, 245, 249),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            StyleHelper.SetRoundedRegion(previewImage, 8);
            previewImage.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, previewImage.Width - 1, previewImage.Height - 1);
                using var path = StyleHelper.GetRoundedPath(r, 8);
                using var brush = new SolidBrush(previewImage.BackColor);
                e.Graphics.FillPath(brush, path);
                if (previewImage.Image != null)
                {
                    e.Graphics.SetClip(path);
                    e.Graphics.DrawImage(previewImage.Image, 0, 0, previewImage.Width, previewImage.Height);
                    e.Graphics.ResetClip();
                }
                else
                {
                    TextRenderer.DrawText(e.Graphics, "📷", new Font("Segoe UI", 28),
                        previewImage.ClientRectangle, Color.FromArgb(200, 200, 210),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                using var pen = new Pen(StyleHelper.Border, 1);
                e.Graphics.DrawPath(pen, path);
            };
            previewCard.Controls.Add(previewImage);

            previewName = new Label
            {
                Text = "Название товара",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = StyleHelper.TextPrimary,
                Location = new Point(12, 140),
                Size = new Size(176, 36),
                AutoEllipsis = true
            };
            previewCard.Controls.Add(previewName);

            previewCategory = new Label
            {
                Text = "Категория",
                Font = new Font("Segoe UI", 8),
                ForeColor = StyleHelper.TextMuted,
                Location = new Point(12, 178),
                AutoSize = true
            };
            previewCard.Controls.Add(previewCategory);

            previewPrice = new Label
            {
                Text = "0 ₽",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = StyleHelper.TextPrimary,
                Location = new Point(12, 200),
                AutoSize = true
            };
            previewCard.Controls.Add(previewPrice);

            previewCard.Controls.Add(new Label
            {
                Text = "⬅ Превью товара",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = StyleHelper.TextMuted,
                Location = new Point(12, 280),
                AutoSize = true
            });

            card.Controls.Add(previewCard);

            int btnY = 330;
            btnSelectImage = new Button
            {
                Text = "📁 Выбрать фото",
                Location = new Point(x, btnY),
                Size = new Size(140, 38),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.Transparent,
                ForeColor = StyleHelper.Accent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft
            };
            btnSelectImage.FlatAppearance.BorderSize = 0;
            btnSelectImage.MouseEnter += (s, e) => btnSelectImage.ForeColor = ControlPaint.Dark(StyleHelper.Accent, 0.2f);
            btnSelectImage.MouseLeave += (s, e) => btnSelectImage.ForeColor = StyleHelper.Accent;
            btnSelectImage.Click += (s, e) => SelectImage();
            card.Controls.Add(btnSelectImage);

            var btnClearImage = new Button
            {
                Text = "✕ Очистить",
                Location = new Point(x + 148, btnY),
                Size = new Size(80, 38),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.Transparent,
                ForeColor = StyleHelper.TextMuted,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft
            };
            btnClearImage.FlatAppearance.BorderSize = 0;
            btnClearImage.MouseEnter += (s, e) => btnClearImage.ForeColor = StyleHelper.Danger;
            btnClearImage.MouseLeave += (s, e) => btnClearImage.ForeColor = StyleHelper.TextMuted;
            btnClearImage.Click += (s, e) => { selectedImagePath = null; previewImage.Image = null; previewImage.Invalidate(); };
            card.Controls.Add(btnClearImage);

            btnCreate = new Button
            {
                Text = "✓ Создать товар",
                Location = new Point(previewX, btnY),
                Size = new Size(170, 38),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = StyleHelper.Accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCreate.FlatAppearance.BorderSize = 0;
            StyleHelper.SetRoundedRegion(btnCreate, 8);
            btnCreate.Paint += (s, e) =>
            {
                if (s is not Button b) return;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = StyleHelper.GetRoundedPath(new Rectangle(0, 0, b.Width, b.Height), 8);
                using var brush = new SolidBrush(b.BackColor);
                e.Graphics.FillPath(brush, path);
                TextRenderer.DrawText(e.Graphics, b.Text, b.Font, b.ClientRectangle, b.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            btnCreate.MouseEnter += (s, e) => { btnCreate.BackColor = ControlPaint.Dark(StyleHelper.Accent, 0.1f); btnCreate.Invalidate(); };
            btnCreate.MouseLeave += (s, e) => { btnCreate.BackColor = StyleHelper.Accent; btnCreate.Invalidate(); };
            btnCreate.Click += BtnCreate_Click;
            card.Controls.Add(btnCreate);

            var btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(previewX + 178, btnY),
                Size = new Size(100, 38),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                ForeColor = StyleHelper.TextMuted,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.FlatAppearance.BorderColor = StyleHelper.Border;
            StyleHelper.SetRoundedRegion(btnCancel, 8);
            btnCancel.Paint += (s, e) =>
            {
                if (s is not Button b) return;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = StyleHelper.GetRoundedPath(new Rectangle(0, 0, b.Width, b.Height), 8);
                using var brush = new SolidBrush(b.BackColor);
                e.Graphics.FillPath(brush, path);
                using var pen = new Pen(b.FlatAppearance.BorderColor, 1);
                e.Graphics.DrawPath(pen, path);
                TextRenderer.DrawText(e.Graphics, b.Text, b.Font, b.ClientRectangle, b.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            btnCancel.MouseEnter += (s, e) => { btnCancel.BackColor = Color.FromArgb(248, 250, 252); btnCancel.Invalidate(); };
            btnCancel.MouseLeave += (s, e) => { btnCancel.BackColor = Color.White; btnCancel.Invalidate(); };
            btnCancel.Click += (s, e) => { var main = FindForm() as MainForm; main?.CloseEmbeddedForm(); };

            lblSuccess = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = StyleHelper.Success,
                AutoSize = true,
                Visible = false
            };
            Controls.Add(lblTitle);
            Controls.Add(card);
            Controls.Add(lblSuccess);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Controls.Count >= 2 && Controls[0] is Label && Controls[1] is Panel card)
            {
                card.Location = new Point(Math.Max(24, (Width - card.Width) / 2), 55);
            }
        }

        private void UpdatePreview(object? sender = null, EventArgs? eArgs = null)
        {
            previewName.Text = string.IsNullOrWhiteSpace(txtName.Text) ? "Название товара" : txtName.Text;
            previewCategory.Text = string.IsNullOrWhiteSpace(cmbCategory.Text) ? "Категория" : cmbCategory.Text;
            if (decimal.TryParse(txtPrice.Text, out decimal price) && price > 0)
                previewPrice.Text = $"{price:N0} ₽";
            else
                previewPrice.Text = "0 ₽";
        }

        private void SelectImage()
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Выберите фото товара"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                selectedImagePath = ofd.FileName;
                try
                {
                    previewImage.Image = Image.FromFile(selectedImagePath);
                    previewImage.Invalidate();
                    previewCard.Invalidate();
                }
                catch
                {
                    MessageBox.Show("Не удалось загрузить изображение", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    selectedImagePath = null;
                }
            }
        }

        private byte[]? ConvertImageToBytes()
        {
            if (string.IsNullOrEmpty(selectedImagePath)) return null;
            try { using var img = Image.FromFile(selectedImagePath); using var ms = new MemoryStream(); img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg); return ms.ToArray(); }
            catch { return File.ReadAllBytes(selectedImagePath); }
        }

        private void BtnCreate_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            { MessageBox.Show("Введите название товара", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtName.Focus(); return; }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            { MessageBox.Show("Введите корректную цену", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtPrice.Focus(); return; }

            if (string.IsNullOrWhiteSpace(cmbCategory.Text))
            { MessageBox.Show("Выберите или введите категорию", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); cmbCategory.Focus(); return; }

            int amount = 0;
            if (!string.IsNullOrWhiteSpace(txtAmount.Text) && !int.TryParse(txtAmount.Text, out amount))
            { MessageBox.Show("Количество должно быть целым числом", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtAmount.Focus(); return; }

            try
            {
                var product = _productService.CreateProduct(
                    txtName.Text.Trim(), txtDescription.Text.Trim(), price,
                    cmbCategory.Text.Trim(), ConvertImageToBytes(), amount);

                lblSuccess.Text = $"✅ Товар «{product.Name}» создан и отправлен на модерацию!";
                lblSuccess.Location = new Point(24, 463);
                lblSuccess.Visible = true;

                txtName.Clear(); txtDescription.Clear(); txtPrice.Clear(); cmbCategory.Text = ""; txtAmount.Clear();
                selectedImagePath = null; previewImage.Image = null; previewImage.Invalidate();
                UpdatePreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public new void Refresh()
        {
            lblSuccess.Visible = false;
        }
    }
}
