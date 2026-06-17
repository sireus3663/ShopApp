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
    public class CreateProductForm : Form
    {
        private readonly AuthService _authService;
        private readonly ProductService _productService;
        private readonly AppDbContext _context;

        private TextBox txtName = null!;
        private TextBox txtDescription = null!;
        private TextBox txtPrice = null!;
        private ComboBox cmbCategory = null!;
        private TextBox txtAmount = null!;
        private string? selectedImagePath = null;

        private Panel previewCard = null!;
        private Label previewName = null!;
        private Label previewPrice = null!;
        private Label previewCategory = null!;
        private PictureBox previewImage = null!;

        public CreateProductForm(AuthService authService, ProductService productService, AppDbContext context)
        {
            _authService = authService;
            _productService = productService;
            _context = context;
            InitializeForm();
        }

        private void InitializeForm()
        {
            Text = "Создать товар";
            Size = new Size(720, 500);
            MinimumSize = new Size(660, 460);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = StyleHelper.BgPage;

            var card = new Panel
            {
                BackColor = Color.White,
                Size = new Size(660, 380)
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

            card.Controls.Add(new Label
            {
                Text = "➕ Создать товар",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = StyleHelper.TextPrimary,
                Location = new Point(24, 16),
                AutoSize = true
            });

            int x = 24;
            int y = 56;
            int fw = 280;

            txtName = MakeField(x + 110, y, fw, "Название товара");
            card.Controls.Add(MakeLabel("Название", x, y + 6));
            txtName.TextChanged += UpdatePreview;
            y += 42;

            txtDescription = new TextBox
            {
                Location = new Point(x + 110, y),
                Size = new Size(fw, 64),
                Font = new Font("Segoe UI", 10),
                Multiline = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 251, 252),
                ForeColor = StyleHelper.TextPrimary,
                PlaceholderText = "Описание (необязательно)",
                ScrollBars = ScrollBars.Vertical,
                AcceptsReturn = true
            };
            card.Controls.Add(txtDescription);
            card.Controls.Add(MakeLabel("Описание", x, y + 6));
            y += 76;

            txtPrice = MakeField(x + 110, y, fw, "0.00");
            card.Controls.Add(MakeLabel("Цена", x, y + 6));
            txtPrice.TextChanged += UpdatePreview;
            y += 42;

            cmbCategory = new ComboBox
            {
                Location = new Point(x + 110, y),
                Size = new Size(fw, 34),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDown,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(250, 251, 252),
                ForeColor = StyleHelper.TextPrimary
            };
            cmbCategory.Items.AddRange(new[] { "Электроника", "Одежда", "Продукты", "Книги", "Спорт", "Мебель", "Игрушки", "Другое" });
            cmbCategory.SelectedIndex = -1;
            cmbCategory.TextChanged += UpdatePreview;
            card.Controls.Add(cmbCategory);
            card.Controls.Add(MakeLabel("Категория", x, y + 6));
            y += 42;

            txtAmount = MakeField(x + 110, y, fw, "0");
            card.Controls.Add(MakeLabel("Количество", x, y + 6));
            y += 48;

            int px = x + 110 + fw + 24;
            previewCard = new Panel
            {
                Location = new Point(px, 48),
                Size = new Size(200, 280),
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
            };

            previewImage = new PictureBox
            {
                Size = new Size(176, 110),
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
                    TextRenderer.DrawText(e.Graphics, "📷", new Font("Segoe UI", 24),
                        previewImage.ClientRectangle, Color.FromArgb(200, 200, 210),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                using var pen = new Pen(StyleHelper.Border, 1);
                e.Graphics.DrawPath(pen, path);
            };
            previewCard.Controls.Add(previewImage);

            previewName = new Label
            {
                Text = "Название товара",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = StyleHelper.TextPrimary,
                Location = new Point(12, 128),
                Size = new Size(176, 36),
                AutoEllipsis = true
            };
            previewCard.Controls.Add(previewName);

            previewCategory = new Label
            {
                Text = "Категория",
                Font = new Font("Segoe UI", 8),
                ForeColor = StyleHelper.TextMuted,
                Location = new Point(12, 166),
                AutoSize = true
            };
            previewCard.Controls.Add(previewCategory);

            previewPrice = new Label
            {
                Text = "0 ₽",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = StyleHelper.TextPrimary,
                Location = new Point(12, 188),
                AutoSize = true
            };
            previewCard.Controls.Add(previewPrice);

            previewCard.Controls.Add(new Label
            {
                Text = "⬅ Превью",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = StyleHelper.TextMuted,
                Location = new Point(12, 256),
                AutoSize = true
            });
            card.Controls.Add(previewCard);

            int btnY = 310;
            var btnSelect = new Button
            {
                Text = "📁 Выбрать фото",
                Location = new Point(x, btnY),
                Size = new Size(130, 36),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.Transparent,
                ForeColor = StyleHelper.Accent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft
            };
            btnSelect.FlatAppearance.BorderSize = 0;
            btnSelect.MouseEnter += (s, e) => btnSelect.ForeColor = ControlPaint.Dark(StyleHelper.Accent, 0.2f);
            btnSelect.MouseLeave += (s, e) => btnSelect.ForeColor = StyleHelper.Accent;
            btnSelect.Click += (s, e) => SelectImage();
            card.Controls.Add(btnSelect);

            var btnCreate = new Button
            {
                Text = "✓ Создать товар",
                Location = new Point(x + 300, btnY),
                Size = new Size(160, 36),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = StyleHelper.Accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCreate.FlatAppearance.BorderSize = 0;
            StyleHelper.SetRoundedRegion(btnCreate, 8);
            btnCreate.Paint += RoundedPaint;
            btnCreate.MouseEnter += (s, e) => { btnCreate.BackColor = ControlPaint.Dark(StyleHelper.Accent, 0.1f); btnCreate.Invalidate(); };
            btnCreate.MouseLeave += (s, e) => { btnCreate.BackColor = StyleHelper.Accent; btnCreate.Invalidate(); };
            btnCreate.Click += BtnCreate_Click;
            card.Controls.Add(btnCreate);

            var btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(x + 468, btnY),
                Size = new Size(100, 36),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                ForeColor = StyleHelper.TextMuted,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.FlatAppearance.BorderColor = StyleHelper.Border;
            StyleHelper.SetRoundedRegion(btnCancel, 8);
            btnCancel.Paint += RoundedPaint;
            btnCancel.MouseEnter += (s, e) => { btnCancel.BackColor = Color.FromArgb(248, 250, 252); btnCancel.Invalidate(); };
            btnCancel.MouseLeave += (s, e) => { btnCancel.BackColor = Color.White; btnCancel.Invalidate(); };
            btnCancel.Click += (s, e) => Close();

            AcceptButton = btnCreate;
            Controls.Add(card);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Controls.Count > 0 && Controls[0] is Panel card)
                card.Location = new Point(Math.Max(0, (ClientSize.Width - card.Width) / 2), 24);
        }

        private static void RoundedPaint(object? sender, PaintEventArgs e)
        {
            if (sender is not Button btn) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, btn.Width, btn.Height);
            using var path = StyleHelper.GetRoundedPath(r, 8);
            using var brush = new SolidBrush(btn.BackColor);
            e.Graphics.FillPath(brush, path);
            if (btn.FlatAppearance.BorderSize > 0)
                using (var pen = new Pen(btn.FlatAppearance.BorderColor, 1))
                    e.Graphics.DrawPath(pen, path);
            TextRenderer.DrawText(e.Graphics, btn.Text, btn.Font, r, btn.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private static Label MakeLabel(string text, int x, int y)
        {
            return new Label { Text = text, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = StyleHelper.TextMuted, Location = new Point(x, y), AutoSize = true };
        }

        private static TextBox MakeField(int x, int y, int w, string placeholder)
        {
            return new TextBox
            {
                Location = new Point(x, y), Size = new Size(w, 32), Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(250, 251, 252),
                ForeColor = StyleHelper.TextPrimary, PlaceholderText = placeholder
            };
        }

        private void UpdatePreview(object? sender = null, EventArgs? eArgs = null)
        {
            previewName.Text = string.IsNullOrWhiteSpace(txtName.Text) ? "Название товара" : txtName.Text;
            previewCategory.Text = string.IsNullOrWhiteSpace(cmbCategory.Text) ? "Категория" : cmbCategory.Text;
            previewPrice.Text = decimal.TryParse(txtPrice.Text, out var p) && p > 0 ? $"{p:N0} ₽" : "0 ₽";
        }

        private void SelectImage()
        {
            using var ofd = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp", Title = "Выберите фото товара" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                selectedImagePath = ofd.FileName;
                try { previewImage.Image = Image.FromFile(selectedImagePath); previewImage.Invalidate(); }
                catch { MessageBox.Show("Не удалось загрузить изображение", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); selectedImagePath = null; }
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
                _productService.CreateProduct(txtName.Text.Trim(), txtDescription.Text.Trim(), price,
                    cmbCategory.Text.Trim(), ConvertImageToBytes(), amount);
                MessageBox.Show("Товар создан и отправлен на модерацию!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
    }
}
