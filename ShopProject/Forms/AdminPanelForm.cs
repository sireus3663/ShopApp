using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Forms.ViewModels;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace ShopProject.Forms
{
    public class AdminPanelForm : Form
    {
        private readonly AppDbContext _context;
        private readonly User _currentUser;
        private Panel headerPanel = null!;
        private DataGridView usersGrid = null!;
        private TextBox searchBox = null!;
        private ComboBox cmbRoleFilter = null!;
        private ComboBox cmbStatusFilter = null!;
        private Label lblTitle = null!;
        private Label lblSubtitle = null!;
        private Label lblUserCount = null!;
        private Label lblSelectedInfo = null!;
        private Panel gridBorder = null!;
        private Button btnChangeRole = null!;
        private Button btnBlock = null!;
        private Button btnHistory = null!;
        private Button btnBalance = null!;
        private readonly AdminViewModel _viewModel = null!;

        private static readonly Color BgPage = Color.FromArgb(245, 247, 250);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);
        private static readonly Color Danger = Color.FromArgb(239, 68, 68);
        private static readonly Color Green = Color.FromArgb(22, 163, 74);
        private static readonly Color Gray = Color.FromArgb(100, 116, 139);
        private static readonly Color Border = Color.FromArgb(226, 232, 240);

        public AdminPanelForm(AppDbContext context, User currentUser)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

            var userRepo = new UserRepository(context);
            var orderRepo = new OrderRepository(context);
            var productRepo = new ProductRepository(context);
            _viewModel = new AdminViewModel(userRepo, orderRepo, productRepo, currentUser);

            InitializeComponent();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            Text = "Управление пользователями";
            BackColor = BgPage;
            Padding = new Padding(24);
            MinimumSize = new Size(850, 580);

            headerPanel = new Panel
            {
                BackColor = Color.FromArgb(241, 245, 249),
                Height = 44
            };
            headerPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var path = RoundedRect(new Rectangle(0, 0, headerPanel.Width - 1, headerPanel.Height - 1), 10);
                e.Graphics.FillPath(new SolidBrush(Color.FromArgb(241, 245, 249)), path);
            };

            lblTitle = new Label
            {
                Text = "👥 Управление пользователями",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                AutoSize = true
            };

            lblSubtitle = new Label
            {
                Text = $"Администратор: {_currentUser.Name}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Gray,
                AutoSize = true
            };

            searchBox = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(15, 23, 42),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = " 🔍 Поиск по имени или email..."
            };
            searchBox.TextChanged += (s, e) => LoadUsers();

            cmbRoleFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            cmbRoleFilter.Items.AddRange(new[] { "Все роли", "Admin", "User" });
            cmbRoleFilter.SelectedIndex = 0;
            cmbRoleFilter.SelectedIndexChanged += (s, e) => LoadUsers();

            cmbStatusFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            cmbStatusFilter.Items.AddRange(new[] { "Все статусы", "Активен", "Заблокирован" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => LoadUsers();

            lblUserCount = new Label
            {
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Gray,
                AutoSize = true
            };

            gridBorder = new Panel
            {
                BackColor = Color.White
            };
            gridBorder.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, gridBorder.Width - 1, gridBorder.Height - 1);
                using var path = RoundedRect(rect, 8);
                e.Graphics.FillPath(new SolidBrush(Color.White), path);
                using var pen = new Pen(Border, 1);
                e.Graphics.DrawPath(pen, path);
            };

            usersGrid = new DataGridView
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                BackgroundColor = Color.White,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(15, 23, 42),
                GridColor = Border,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            };
            usersGrid.RowTemplate.Height = 42;
            usersGrid.ColumnHeadersHeight = 42;
            usersGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            usersGrid.EnableHeadersVisualStyles = false;
            usersGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            usersGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            usersGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(71, 85, 105);
            usersGrid.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 4, 0, 4);
            usersGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            usersGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            usersGrid.RowsDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            usersGrid.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(37, 99, 235);
            usersGrid.RowsDefaultCellStyle.SelectionForeColor = Color.White;
            usersGrid.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex < 0 || e.RowIndex < 0) return;
                if (usersGrid.Columns[e.ColumnIndex].HeaderText == "Статус")
                {
                    if (e.Value?.ToString() == "Заблокирован")
                        e.CellStyle.ForeColor = Danger;
                    else if (e.Value?.ToString() == "Активен")
                        e.CellStyle.ForeColor = Green;
                    e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }
            };
            usersGrid.SelectionChanged += (s, e) => UpdateSelectedInfo();
            usersGrid.CellDoubleClick += (s, e) => ChangeRole_Click(s, e);

            lblSelectedInfo = new Label
            {
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(71, 85, 105),
                AutoSize = true
            };

            headerPanel.Controls.AddRange(new Control[] { lblTitle, lblSubtitle });
            gridBorder.Controls.Add(usersGrid);
            Controls.AddRange(new Control[]
            {
                headerPanel, searchBox, cmbRoleFilter, cmbStatusFilter,
                lblUserCount, gridBorder, lblSelectedInfo
            });

            btnChangeRole = CreateActionBtn("Изменить роль", Accent);
            btnChangeRole.Click += ChangeRole_Click;
            btnBlock = CreateActionBtn("Блокировка", Danger);
            btnBlock.Click += BlockUser_Click;
            btnHistory = CreateActionBtn("История операций", Gray);
            btnHistory.Click += HistoryBtn_Click;
            btnBalance = CreateActionBtn("Изменить баланс", Accent);
            btnBalance.Click += ChangeBalanceBtn_Click;

            Controls.AddRange(new Control[] { btnChangeRole, btnBlock, btnHistory, btnBalance });
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (searchBox == null) return;

            int w = ClientSize.Width - Padding.Horizontal;

            headerPanel.Location = new Point(Padding.Left, 0);
            headerPanel.Width = w;

            lblTitle.Location = new Point(20, (headerPanel.Height - lblTitle.Height) / 2);

            lblSubtitle.Text = $"Администратор: {_currentUser.Name}";
            int subW = TextRenderer.MeasureText(lblSubtitle.Text, lblSubtitle.Font).Width;
            lblSubtitle.Location = new Point(headerPanel.Width - subW - 20, (headerPanel.Height - lblSubtitle.Height) / 2);

            int filterY = 54;
            int filterH = 32;
            int gap = 8;

            int sw = Math.Max(200, Math.Min(300, w - 480));
            searchBox.Location = new Point(Padding.Left, filterY);
            searchBox.Size = new Size(sw, filterH);

            int comboH = 32;
            int comboW = 130;
            int cx = Padding.Left + sw + gap;
            cmbRoleFilter.Location = new Point(cx, filterY);
            cmbRoleFilter.Size = new Size(comboW, comboH);

            cx += comboW + gap;
            cmbStatusFilter.Location = new Point(cx, filterY);
            cmbStatusFilter.Size = new Size(comboW, comboH);

            int countY = filterY + filterH + 8;
            lblUserCount.Location = new Point(Padding.Left, countY);

            int selectedY = ClientSize.Height - Padding.Bottom - 72;
            lblSelectedInfo.Location = new Point(Padding.Left, selectedY);

            int btnY = ClientSize.Height - Padding.Bottom - 36;
            int[] widths = { 150, 120, 150, 150 };
            int totalW = widths.Sum() + gap * (widths.Length - 1);
            int startX = Math.Max(0, Padding.Left + (w - totalW) / 2);

            var btns = new[] { btnChangeRole, btnBlock, btnHistory, btnBalance };
            for (int i = 0; i < btns.Length; i++)
            {
                if (btns[i] != null)
                {
                    btns[i].Size = new Size(widths[i], 36);
                    btns[i].Location = new Point(startX, btnY);
                    startX += widths[i] + gap;
                }
            }

            int gridTop = countY + 22;
            int gridBottom = selectedY - 6;
            int gh = Math.Max(100, gridBottom - gridTop);
            gridBorder.Location = new Point(Padding.Left, gridTop);
            gridBorder.Size = new Size(w, gh);
            usersGrid.Location = new Point(0, 0);
            usersGrid.Size = new Size(gridBorder.Width, gridBorder.Height);
        }

        private void UpdateSelectedInfo()
        {
            if (usersGrid.SelectedRows.Count == 0)
            {
                lblSelectedInfo.Text = "";
                return;
            }

            var row = usersGrid.SelectedRows[0];
            var name = row.Cells["Имя"].Value?.ToString() ?? "";
            var email = row.Cells["Email"].Value?.ToString() ?? "";
            var role = row.Cells["Роль"].Value?.ToString() ?? "";
            var balance = row.Cells["Баланс"].Value?.ToString() ?? "";
            lblSelectedInfo.Text = $"👤 {name}  ·  {email}  ·  {role}  ·  {balance}";
        }

        private void LoadUsers()
        {
            try
            {
                var searchText = searchBox.Text?.Trim() ?? "";
                var roleFilter = cmbRoleFilter.SelectedItem?.ToString();
                var statusFilter = cmbStatusFilter.SelectedItem?.ToString();

                var users = _viewModel.SearchUsers(searchText);

                if (roleFilter != null && roleFilter != "Все роли" && Enum.TryParse<Role>(roleFilter, out var role))
                    users = users.Where(u => u.Role == role).ToList();

                if (statusFilter == "Активен")
                    users = users.Where(u => !u.IsBlocked).ToList();
                else if (statusFilter == "Заблокирован")
                    users = users.Where(u => u.IsBlocked).ToList();

                var allCount = _viewModel.SearchUsers("").Count;
                var filteredCount = users.Count;
                lblUserCount.Text = filteredCount == allCount
                    ? $"📊 Всего: {allCount} пользователей"
                    : $"📊 Показано: {filteredCount} из {allCount}";

                usersGrid.DataSource = users.Select(u => new
                {
                    u.Id,
                    Имя = u.Name,
                    u.Email,
                    Роль = u.Role.ToString(),
                    Баланс = $"{u.Balance:N0} ₽",
                    Статус = u.IsBlocked ? "Заблокирован" : "Активен"
                }).ToList();

                if (usersGrid.Columns["Id"] != null)
                    usersGrid.Columns["Id"].Visible = false;

                usersGrid.Columns["Имя"].Width = 180;
                usersGrid.Columns["Email"].FillWeight = 1;
                usersGrid.Columns["Email"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                usersGrid.Columns["Роль"].Width = 100;
                usersGrid.Columns["Баланс"].Width = 130;
                usersGrid.Columns["Статус"].Width = 120;

                foreach (DataGridViewColumn col in usersGrid.Columns)
                    col.SortMode = DataGridViewColumnSortMode.NotSortable;

                usersGrid.ClearSelection();
                UpdateSelectedInfo();
            }
            catch (Exception ex)
            {
                ErrorForm.Show(ex.Message, ex);
            }
        }

        private User? GetSelectedUser()
        {
            if (usersGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите пользователя", "Ошибка");
                return null;
            }
            var userId = (Guid)usersGrid.SelectedRows[0].Cells["Id"].Value;
            return _viewModel.GetUser(userId);
        }

        private void ChangeRole_Click(object? sender, EventArgs e)
        {
            var user = GetSelectedUser();
            if (user == null) return;

            using var roleForm = new Form
            {
                Text = "Смена роли",
                Size = new Size(320, 200),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(245, 247, 250),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lbl = new Label
            {
                Text = $"Выберите новую роль для {user.Name}:",
                Location = new Point(24, 20),
                Size = new Size(260, 24),
                Font = new Font("Segoe UI", 10)
            };

            var combo = new ComboBox
            {
                Location = new Point(24, 54),
                Size = new Size(260, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            combo.Items.AddRange(Enum.GetNames(typeof(Role)));
            combo.SelectedItem = user.Role.ToString();

            var saveBtn = new Button
            {
                Text = "Сохранить",
                Location = new Point(24, 100),
                Size = new Size(120, 34),
                BackColor = Accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            saveBtn.FlatAppearance.BorderSize = 0;
            saveBtn.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundedRect(saveBtn.ClientRectangle, 8);
                using var brush = new SolidBrush(saveBtn.BackColor);
                pe.Graphics.FillPath(brush, path);
                TextRenderer.DrawText(pe.Graphics, saveBtn.Text, saveBtn.Font,
                    saveBtn.ClientRectangle, saveBtn.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            saveBtn.MouseEnter += (s, ev) => { saveBtn.BackColor = ControlPaint.Dark(Accent, 0.1f); saveBtn.Invalidate(); };
            saveBtn.MouseLeave += (s, ev) => { saveBtn.BackColor = Accent; saveBtn.Invalidate(); };
            saveBtn.Click += (s, ev) =>
            {
                var selected = combo.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(selected)) return;
                var newRole = (Role)Enum.Parse(typeof(Role), selected);
                _viewModel.ChangeUserRole(user.Id, newRole);
                LoadUsers();
                roleForm.Close();
                MessageBox.Show($"Роль пользователя {user.Name} изменена на {selected}", "Успешно");
            };

            roleForm.Controls.AddRange(new Control[] { lbl, combo, saveBtn });
            roleForm.ShowDialog(this);
        }

        private void BlockUser_Click(object? sender, EventArgs e)
        {
            var user = GetSelectedUser();
            if (user == null) return;

            try
            {
                _viewModel.ToggleUserBlock(user.Id);
                LoadUsers();
                var status = user.IsBlocked ? "заблокирован" : "разблокирован";
                MessageBox.Show($"Пользователь {user.Name} {status}", "Успешно");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void HistoryBtn_Click(object? sender, EventArgs e)
        {
            var user = GetSelectedUser();
            if (user == null) return;

            var orders = _viewModel.GetUserOrders(user.Id);
            var totalSpent = _viewModel.GetUserTotalSpent(user.Id);

            using var historyForm = new Form
            {
                Text = $"История операций - {user.Name}",
                Size = new Size(800, 470),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(245, 247, 250)
            };

            var grid = new DataGridView
            {
                Location = new Point(16, 16),
                Size = new Size(752, 300),
                BackgroundColor = Color.White,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(226, 232, 240),
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            };
            grid.RowTemplate.Height = 30;

            var data = orders.Select(o => new
            {
                Дата = o.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                Товар = _viewModel.GetProductName(o.ProductId),
                Количество = o.Count,
                Сумма = $"{o.Price:N0} ₽",
                Статус = "Выполнен"
            }).ToList();
            grid.DataSource = data;

            var info = new Label
            {
                Text = $"💰 Текущий баланс: {user.Balance:N0} ₽    |    💳 Всего потрачено: {totalSpent:N0} ₽",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Accent,
                Location = new Point(16, 330),
                AutoSize = true
            };

            var closeBtn = new Button
            {
                Text = "Закрыть",
                Location = new Point(668, 385),
                Size = new Size(100, 34),
                BackColor = Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundedRect(closeBtn.ClientRectangle, 8);
                using var brush = new SolidBrush(closeBtn.BackColor);
                pe.Graphics.FillPath(brush, path);
                TextRenderer.DrawText(pe.Graphics, closeBtn.Text, closeBtn.Font,
                    closeBtn.ClientRectangle, closeBtn.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            closeBtn.Click += (s, ev) => historyForm.Close();

            historyForm.Controls.AddRange(new Control[] { grid, info, closeBtn });
            historyForm.ShowDialog(this);
        }

        private void ChangeBalanceBtn_Click(object? sender, EventArgs e)
        {
            var user = GetSelectedUser();
            if (user == null) return;

            using var balanceForm = new Form
            {
                Text = "Изменение баланса",
                Size = new Size(340, 210),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(245, 247, 250),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lblCurrent = new Label
            {
                Text = $"💰 Текущий баланс: {user.Balance:N0} ₽",
                Location = new Point(24, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Accent,
                AutoSize = true
            };

            var lblNew = new Label
            {
                Text = "Новый баланс:",
                Location = new Point(24, 54),
                Font = new Font("Segoe UI", 10),
                AutoSize = true
            };

            var txtNew = new TextBox
            {
                Location = new Point(140, 50),
                Size = new Size(160, 28),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            var saveBtn = new Button
            {
                Text = "Сохранить",
                Location = new Point(24, 100),
                Size = new Size(120, 34),
                BackColor = Accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            saveBtn.FlatAppearance.BorderSize = 0;
            saveBtn.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundedRect(saveBtn.ClientRectangle, 8);
                using var brush = new SolidBrush(saveBtn.BackColor);
                pe.Graphics.FillPath(brush, path);
                TextRenderer.DrawText(pe.Graphics, saveBtn.Text, saveBtn.Font,
                    saveBtn.ClientRectangle, saveBtn.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            saveBtn.MouseEnter += (s, ev) => { saveBtn.BackColor = ControlPaint.Dark(Accent, 0.1f); saveBtn.Invalidate(); };
            saveBtn.MouseLeave += (s, ev) => { saveBtn.BackColor = Accent; saveBtn.Invalidate(); };
            saveBtn.Click += (s, ev) =>
            {
                if (decimal.TryParse(txtNew.Text, out var newBalance))
                {
                    try
                    {
                        _viewModel.ChangeUserBalance(user.Id, newBalance);
                        MessageBox.Show($"Баланс {user.Name}: {user.Balance:N0} ₽ → {newBalance:N0} ₽", "Успешно");
                        LoadUsers();
                        balanceForm.Close();
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
                }
                else
                    MessageBox.Show("Введите корректную сумму", "Ошибка");
            };

            balanceForm.Controls.AddRange(new Control[] { lblCurrent, lblNew, txtNew, saveBtn });
            balanceForm.ShowDialog(this);
        }

        private Button CreateActionBtn(string text, Color bg)
        {
            var b = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9),
                BackColor = bg,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundedRect(b.ClientRectangle, 8);
                using var brush = new SolidBrush(b.BackColor);
                e.Graphics.FillPath(brush, path);
                TextRenderer.DrawText(e.Graphics, b.Text, b.Font, b.ClientRectangle, b.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            b.MouseEnter += (s, e) => { b.BackColor = ControlPaint.Dark(bg, 0.1f); b.Invalidate(); };
            b.MouseLeave += (s, e) => { b.BackColor = bg; b.Invalidate(); };
            return b;
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
