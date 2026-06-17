using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;
using ShopProject.Services.Interfaces;

namespace ShopProject.Forms
{
    public class RefundModerationForm : Form
    {
        private readonly IRefundService _refundService;
        private readonly AuthService _authService;
        private readonly AppDbContext _context;
        private readonly FlowLayoutPanel flowRefunds;
        private readonly Label lblTitle;

        public RefundModerationForm(IRefundService refundService, AuthService authService, AppDbContext context)
        {
            _refundService = refundService;
            _authService = authService;
            _context = context;

            BackColor = Color.FromArgb(245, 247, 250);
            Text = "Модерация возвратов";
            Size = new Size(900, 600);
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(600, 400);

            lblTitle = new Label
            {
                Text = "↩ Запросы на возврат",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                AutoSize = true,
                Location = new Point(20, 16)
            };

            flowRefunds = new FlowLayoutPanel
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
            Controls.Add(flowRefunds);

            flowRefunds.Resize += (s, e) =>
            {
                foreach (Control c in flowRefunds.Controls)
                    if (c is Panel p)
                        p.Width = flowRefunds.ClientSize.Width - flowRefunds.Padding.Horizontal - p.Margin.Horizontal;
            };

            LoadRequests();
        }

        private void LoadRequests()
        {
            flowRefunds.Controls.Clear();

            var requests = _refundService.GetPendingRequests();

            foreach (var req in requests)
            {
                var product = _context.products.Find(req.ProductId);
                var user = _context.users.Find(req.UserId);
                var order = _context.orders.Find(req.OrderId);
                flowRefunds.Controls.Add(MakeRequestCard(req, product, user, order));
            }

            if (requests.Count == 0)
            {
                flowRefunds.Controls.Add(new Label
                {
                    Text = "Нет запросов на возврат",
                    Font = new Font("Segoe UI", 14),
                    ForeColor = Color.FromArgb(100, 116, 139),
                    AutoSize = true,
                    Margin = new Padding(10, 40, 0, 0)
                });
            }
        }

        private Panel MakeRequestCard(RefundRequest req, Product? product, User? user, Order? order)
        {
            var panel = new Panel
            {
                Height = 130,
                Width = Math.Max(300, flowRefunds.ClientSize.Width - flowRefunds.Padding.Horizontal),
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

            int x = 20;
            int y = 14;

            var lblProduct = new Label
            {
                Text = $"🛒 {product?.Name ?? "Товар удалён"}",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(x, y),
                AutoSize = true
            };

            var lblUser = new Label
            {
                Text = $"👤 {user?.Name ?? "Неизвестно"}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(x, y + 24),
                AutoSize = true
            };

            var lblMeta = new Label
            {
                Text = $"{req.Count} шт. | {req.CreatedAt:dd.MM.yyyy HH:mm} | Заказ: {req.OrderId.ToString().Substring(0, 8)}...",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(x, y + 44),
                AutoSize = true
            };

            var lblReason = new Label
            {
                Text = $"Причина: {req.Reason}",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(x, y + 64),
                AutoSize = true,
                MaximumSize = new Size(panel.Width - 280, 40)
            };

            var btnApprove = new Button
            {
                Text = "✓ Одобрить",
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
            btnApprove.Click += (s, e) =>
            {
                try
                {
                    var moderator = _authService.RequireUser();
                    _refundService.Approve(req.Id, moderator.Id);
                    MessageBox.Show("Возврат одобрен. Средства возвращены пользователю.", "Успех");
                    LoadRequests();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка");
                }
            };

            var btnDecline = new Button
            {
                Text = "✕ Отклонить",
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
            btnDecline.Click += (s, e) =>
            {
                var comment = Microsoft.VisualBasic.Interaction.InputBox(
                    "Укажите причину отклонения (необязательно):",
                    "Отклонение возврата", "");
                try
                {
                    var moderator = _authService.RequireUser();
                    _refundService.Decline(req.Id, moderator.Id,
                        string.IsNullOrWhiteSpace(comment) ? null : comment);
                    MessageBox.Show("Возврат отклонён.", "Готово");
                    LoadRequests();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка");
                }
            };

            panel.Controls.AddRange(new Control[] { lblProduct, lblUser, lblMeta, lblReason, btnApprove, btnDecline });

            panel.Resize += (s, e) =>
            {
                btnApprove.Location = new Point(panel.Width - 250, 22);
                btnDecline.Location = new Point(panel.Width - 130, 22);
                lblReason.MaximumSize = new Size(panel.Width - 280, 40);
            };

            btnApprove.Location = new Point(panel.Width - 250, 22);
            btnDecline.Location = new Point(panel.Width - 130, 22);

            return panel;
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
