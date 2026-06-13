using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Forms.ViewModels;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ShopProject.Forms;

namespace ShopProject.Forms
{
    public class AdminPanelForm : Form
    {
        private AppDbContext _context;
        private User _currentUser;
        private DataGridView usersGrid;
        private TextBox searchBox;
        private Button refreshBtn;
        private Button changeRoleBtn;
        private Button blockBtn;
        private AdminViewModel _viewModel;

        public AdminPanelForm(AppDbContext context, User currentUser)
        {
            _context = context;
            _currentUser = currentUser;

            var userRepo = new UserRepository(context);
            var orderRepo = new OrderRepository(context);
            var productRepo = new ProductRepository(context);
            _viewModel = new AdminViewModel(userRepo, orderRepo, productRepo, currentUser);

            InitializeComponent();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            this.Text = "Панель администратора";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);

            var title = new Label
            {
                Text = $"Управление пользователями - {_currentUser.Name}",
                ForeColor = Color.FromArgb(60, 60, 60),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            Controls.Add(title);

            searchBox = new TextBox
            {
                Location = new Point(20, 60),
                Size = new Size(200, 30),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(60, 60, 60)
            };
            searchBox.TextChanged += (s, e) => LoadUsers();
            Controls.Add(searchBox);

            refreshBtn = new Button
            {
                Text = "Обновить",
                Location = new Point(230, 58),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            refreshBtn.Click += (s, e) => LoadUsers();
            Controls.Add(refreshBtn);

            usersGrid = new DataGridView
            {
                Location = new Point(20, 100),
                Size = new Size(850, 400),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            Controls.Add(usersGrid);

            changeRoleBtn = new Button
            {
                Text = "Изменить роль",
                Location = new Point(20, 520),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            changeRoleBtn.Click += ChangeRole_Click;
            Controls.Add(changeRoleBtn);

            blockBtn = new Button
            {
                Text = "Блокировка",
                Location = new Point(180, 520),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(180, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            blockBtn.Click += BlockUser_Click;
            Controls.Add(blockBtn);

            var historyBtn = new Button
            {
                Text = "История операций",
                Location = new Point(340, 520),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            historyBtn.Click += HistoryBtn_Click;
            Controls.Add(historyBtn);

            var changeBalanceBtn = new Button
            {
                Text = "Изменить баланс",
                Location = new Point(500, 520),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            changeBalanceBtn.Click += ChangeBalanceBtn_Click;
            Controls.Add(changeBalanceBtn);

            var consoleBtn = new Button
            {
                Text = "Запустить консоль",
                Location = new Point(660, 520),
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(60, 80, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            consoleBtn.Click += ConsoleBtn_Click;
            Controls.Add(consoleBtn);

            var closeBtn = new Button
            {
                Text = "Закрыть",
                Location = new Point(770, 520),  
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(180, 180, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            closeBtn.Click += (s, e) => this.Close();
            Controls.Add(closeBtn);
        }

        private void LoadUsers()
        {
            try
            {
                var users = _viewModel.SearchUsers(searchBox.Text);
                usersGrid.DataSource = users.Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    Role = u.Role.ToString(),
                    Balance = $"{u.Balance:N0} руб.",
                    Status = u.IsBlocked ? "Заблокирован" : "Активен"
                }).ToList();
                usersGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                ErrorForm.Show(ex.Message, ex);
            }
        }

        private void ChangeRole_Click(object sender, EventArgs e)
        {
            if (usersGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите пользователя", "Ошибка");
                return;
            }

            var userId = (Guid)usersGrid.SelectedRows[0].Cells["Id"].Value;
            var user = _viewModel.GetUser(userId);
            if (user == null) return;

            var roleForm = new Form
            {
                Text = "Смена роли",
                Size = new Size(300, 180),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            var comboBox = new ComboBox
            {
                Location = new Point(50, 30),
                Size = new Size(180, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            comboBox.Items.AddRange(Enum.GetNames(typeof(Role)));
            comboBox.SelectedItem = user.Role.ToString();

            var btnSave = new Button
            {
                Text = "Сохранить",
                Location = new Point(50, 80),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += (s, ev) =>
            {
                var newRole = (Role)Enum.Parse(typeof(Role), comboBox.SelectedItem.ToString());
                _viewModel.ChangeUserRole(user.Id, newRole);
                LoadUsers();
                roleForm.Close();
                MessageBox.Show("Роль изменена", "Успешно");
            };

            roleForm.Controls.Add(comboBox);
            roleForm.Controls.Add(btnSave);
            roleForm.ShowDialog();
        }

        private void BlockUser_Click(object sender, EventArgs e)
        {
            if (usersGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите пользователя", "Ошибка");
                return;
            }

            var userId = (Guid)usersGrid.SelectedRows[0].Cells["Id"].Value;

            try
            {
                _viewModel.ToggleUserBlock(userId);
                LoadUsers();
                var user = _viewModel.GetUser(userId);
                var status = user.IsBlocked ? "заблокирован" : "разблокирован";
                MessageBox.Show($"Пользователь {user.Name} {status}", "Успешно");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void HistoryBtn_Click(object sender, EventArgs e)
        {
            if (usersGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите пользователя", "Ошибка");
                return;
            }

            var userId = (Guid)usersGrid.SelectedRows[0].Cells["Id"].Value;
            var user = _viewModel.GetUser(userId);
            if (user == null) return;

            var orders = _viewModel.GetUserOrders(userId);
            var totalSpent = _viewModel.GetUserTotalSpent(userId);

            var historyForm = new Form
            {
                Text = $"История операций - {user.Name}",
                Size = new Size(800, 500),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            var grid = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(740, 350),
                BackgroundColor = Color.White,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            var data = orders.Select(o => new
            {
                Дата = o.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                Название_товара = _viewModel.GetProductName(o.ProductId),
                Количество = o.Count,
                Сумма = $"{o.Price:N0} руб.",
                Статус = "Выполнен"
            }).ToList();

            grid.DataSource = data;

            var balanceLabel = new Label
            {
                Text = $"Текущий баланс: {user.Balance:N0} руб.",
                Location = new Point(20, 390),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60)
            };

            var spentLabel = new Label
            {
                Text = $"Всего потрачено: {totalSpent:N0} руб.",
                Location = new Point(20, 420),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(100, 100, 100)
            };

            var closeBtn = new Button
            {
                Text = "Закрыть",
                Location = new Point(660, 420),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            closeBtn.Click += (s, ev) => historyForm.Close();

            historyForm.Controls.Add(grid);
            historyForm.Controls.Add(balanceLabel);
            historyForm.Controls.Add(spentLabel);
            historyForm.Controls.Add(closeBtn);
            historyForm.ShowDialog();
        }

        private void ChangeBalanceBtn_Click(object sender, EventArgs e)
        {
            if (usersGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите пользователя", "Ошибка");
                return;
            }

            var userId = (Guid)usersGrid.SelectedRows[0].Cells["Id"].Value;
            var user = _viewModel.GetUser(userId);
            if (user == null) return;

            var balanceForm = new Form
            {
                Text = "Изменение баланса",
                Size = new Size(350, 200),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(240, 240, 240),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };

            var lblCurrent = new Label
            {
                Text = $"Текущий баланс: {user.Balance:N0} руб.",
                Location = new Point(30, 20),
                Size = new Size(280, 25),
                ForeColor = Color.FromArgb(60, 60, 60),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            var lblNew = new Label
            {
                Text = "Новый баланс:",
                Location = new Point(30, 60),
                Size = new Size(100, 25),
                ForeColor = Color.FromArgb(60, 60, 60)
            };

            var txtNewBalance = new TextBox
            {
                Location = new Point(130, 58),
                Size = new Size(150, 25),
                BackColor = Color.White
            };

            var btnSave = new Button
            {
                Text = "Сохранить",
                Location = new Point(30, 100),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnSave.Click += (s, ev) =>
            {
                if (decimal.TryParse(txtNewBalance.Text, out decimal newBalance))
                {
                    try
                    {
                        _viewModel.ChangeUserBalance(user.Id, newBalance);
                        MessageBox.Show($"Баланс изменён: {user.Balance:N0} руб. → {newBalance:N0} руб.", "Успешно");
                        LoadUsers();
                        balanceForm.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка");
                    }
                }
                else
                {
                    MessageBox.Show("Введите корректную сумму", "Ошибка");
                }
            };

            balanceForm.Controls.Add(lblCurrent);
            balanceForm.Controls.Add(lblNew);
            balanceForm.Controls.Add(txtNewBalance);
            balanceForm.Controls.Add(btnSave);
            balanceForm.ShowDialog();
        }
        private void ConsoleBtn_Click(object sender, EventArgs e)
        {
            var consoleForm = new AdminConsoleForm(_currentUser);
            consoleForm.ShowDialog();
        }
    }
}