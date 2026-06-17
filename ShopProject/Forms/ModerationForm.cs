using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using ShopProject.Models;
using ShopProject.Services;

namespace ShopProject.Forms
{
    public class ModerationForm : Form
    {
        private readonly AuthService _authService;
        private readonly ProductService _productService;
        private readonly FlowLayoutPanel flowModeration;
        private readonly Label lblTitle;

        public Action<Product, ProductService>? OnProductClick { get; set; }

        public ModerationForm(AuthService authService, ProductService productService)
        {
            _authService = authService;
            _productService = productService;

            BackColor = Color.FromArgb(245, 247, 250);
            Text = "Модерация товаров";
            Size = new Size(900, 600);
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(600, 400);

            lblTitle = new Label
            {
                Text = "\U0001f4dd Модерация товаров",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                AutoSize = true,
                Location = new Point(20, 16)
            };

            flowModeration = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.FromArgb(245, 247, 250),
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 60, 20, 20),
                BorderStyle = BorderStyle.None
            };

            Controls.Add(lblTitle);
            Controls.Add(flowModeration);

            flowModeration.Resize += (s, e) =>
            {
                foreach (Control c in flowModeration.Controls)
                    if (c is Panel p)
                        p.Width = flowModeration.ClientSize.Width - flowModeration.Padding.Horizontal - p.Margin.Horizontal;
            };

            var currentUser = _authService.CurrentUser;
            if (!PermissionService.CanModerate(currentUser?.Role ?? Role.Buyer))
            {
                MessageBox.Show("У вас нет прав для модерации");
                Close();
                return;
            }

            LoadProducts();
        }

        public void LoadProducts()
        {
            flowModeration.Controls.Clear();

            var products = _productService.GetForModerate();

            foreach (var product in products)
            {
                var panel = CreateProductPanel(product);
                flowModeration.Controls.Add(panel);
            }

            if (products.Count == 0)
            {
                flowModeration.Controls.Add(new Label
                {
                    Text = "Нет товаров на модерации",
                    Font = new Font("Segoe UI", 14),
                    ForeColor = Color.FromArgb(100, 116, 139),
                    AutoSize = true,
                    Margin = new Padding(10, 40, 0, 0)
                });
            }
        }

        private Panel CreateProductPanel(Product product)
        {
            var panel = new Panel
            {
                Height = 110,
                Width = Math.Max(300, flowModeration.ClientSize.Width - flowModeration.Padding.Horizontal),
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 10)
            };

            panel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundedRect(panel.ClientRectangle, 10);
                e.Graphics.FillPath(new SolidBrush(Color.White), path);
                using var pen = new Pen(Color.FromArgb(226, 232, 240), 1);
                e.Graphics.DrawPath(pen, path);
            };

            panel.Resize += (s, e) => panel.Invalidate();

            var pb = new PictureBox
            {
                Size = new Size(80, 80),
                BackColor = Color.FromArgb(241, 245, 249),
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(14, 15),
                Cursor = Cursors.Hand
            };
            pb.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundedRect(new Rectangle(0, 0, pb.Width - 1, pb.Height - 1), 10);
                e.Graphics.SetClip(path);
                if (pb.Image != null)
                    e.Graphics.DrawImage(pb.Image, 0, 0, pb.Width, pb.Height);
                else
                {
                    e.Graphics.ResetClip();
                    using var brush = new SolidBrush(Color.FromArgb(241, 245, 249));
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(new Pen(Color.FromArgb(226, 232, 240), 1), path);
                    TextRenderer.DrawText(e.Graphics, "\U0001f4f7", new Font("Segoe UI", 10),
                        pb.ClientRectangle, Color.FromArgb(148, 163, 184),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                e.Graphics.ResetClip();
                using var borderPen = new Pen(Color.FromArgb(226, 232, 240), 1);
                e.Graphics.DrawPath(borderPen, path);
            };
            pb.Click += (s, e) => OnProductClick?.Invoke(product, _productService);
            if (product.ProductImage != null && product.ProductImage.Length > 0)
            {
                try
                {
                    using var ms = new MemoryStream(product.ProductImage);
                    pb.Image = Image.FromStream(ms);
                }
                catch { }
            }

            var lblName = new Label
            {
                Text = product.Name ?? "Без названия",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(106, 14),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            lblName.Click += (s, e) => OnProductClick?.Invoke(product, _productService);

            var lblCategory = new Label
            {
                Text = "\U0001f4c1 " + (product.Category ?? "Не указана"),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(106, 40),
                AutoSize = true
            };

            var lblPrice = new Label
            {
                Text = product.Price.ToString("N0") + " \u20bd",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                Location = new Point(106, 60),
                AutoSize = true
            };

            var desc = product.Description ?? "Описание отсутствует";
            desc = desc.Length > 100 ? desc.Substring(0, 100) + "..." : desc;
            var lblDescription = new Label
            {
                Text = desc,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(106, 80),
                AutoSize = true,
                MaximumSize = new Size(panel.Width - 480, 20)
            };

            var btnApprove = new Button
            {
                Text = "\u2713 Одобрить",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(110, 34),
                BackColor = Color.FromArgb(22, 163, 74),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnApprove.FlatAppearance.BorderSize = 0;
            btnApprove.Paint += (s, e) =>
            {
                var b = (Button)s!;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundedRect(b.ClientRectangle, 8);
                using var brush = new SolidBrush(b.BackColor);
                e.Graphics.FillPath(brush, path);
                TextRenderer.DrawText(e.Graphics, b.Text, b.Font, b.ClientRectangle, b.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            btnApprove.MouseEnter += (s, e) =>
            {
                btnApprove.BackColor = Color.FromArgb(21, 128, 61);
                btnApprove.Invalidate();
            };
            btnApprove.MouseLeave += (s, e) =>
            {
                btnApprove.BackColor = Color.FromArgb(22, 163, 74);
                btnApprove.Invalidate();
            };
            btnApprove.Click += (s, e) => ApproveProduct(product);

            var btnDecline = new Button
            {
                Text = "\u2715 Отклонить",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(110, 34),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDecline.FlatAppearance.BorderSize = 0;
            btnDecline.Paint += (s, e) =>
            {
                var b = (Button)s!;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundedRect(b.ClientRectangle, 8);
                using var brush = new SolidBrush(b.BackColor);
                e.Graphics.FillPath(brush, path);
                TextRenderer.DrawText(e.Graphics, b.Text, b.Font, b.ClientRectangle, b.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            btnDecline.MouseEnter += (s, e) =>
            {
                btnDecline.BackColor = Color.FromArgb(220, 38, 38);
                btnDecline.Invalidate();
            };
            btnDecline.MouseLeave += (s, e) =>
            {
                btnDecline.BackColor = Color.FromArgb(239, 68, 68);
                btnDecline.Invalidate();
            };
            btnDecline.Click += (s, e) => DeclineProduct(product);

            panel.Controls.Add(pb);
            panel.Controls.Add(lblName);
            panel.Controls.Add(lblCategory);
            panel.Controls.Add(lblPrice);
            panel.Controls.Add(lblDescription);
            panel.Controls.Add(btnApprove);
            panel.Controls.Add(btnDecline);

            panel.Resize += (s, e) =>
            {
                btnApprove.Location = new Point(panel.Width - 250, 18);
                btnDecline.Location = new Point(panel.Width - 130, 18);
            };

            btnApprove.Location = new Point(panel.Width - 250, 18);
            btnDecline.Location = new Point(panel.Width - 130, 18);

            return panel;
        }

        private void ApproveProduct(Product product)
        {
            try
            {
                _productService.Approve(product.Id);
                MessageBox.Show("Товар одобрен");
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void DeclineProduct(Product product)
        {
            try
            {
                _productService.Decline(product.Id);
                MessageBox.Show("Товар отклонён");
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private static GraphicsPath RoundedRect(Rectangle rect, int r)
        {
            var p = new GraphicsPath();
            p.AddArc(rect.X, rect.Y, r * 2, r * 2, 180, 90);
            p.AddArc(rect.Right - r * 2, rect.Y, r * 2, r * 2, 270, 90);
            p.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
            p.AddArc(rect.X, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
