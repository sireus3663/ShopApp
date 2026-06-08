using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ShopProject.WinForms
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

        public AdminPanelForm(AppDbContext context, User currentUser)
        {
            _context = context;
            _currentUser = currentUser;
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
                var repo = new UserRepository(_context);
                var users = repo.GetAll().AsQueryable();

                string search = searchBox.Text.Trim();
                if (!string.IsNullOrEmpty(search))
                {
                    users = users.Where(u => u.Name.Contains(search) || u.Email.Contains(search));
                }

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
            var userRepo = new UserRepository(_context);
            var user = userRepo.GetById(userId);

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
                var userService = new UserService(_context, null, null);
                userService.ChangeRole(user.Id, newRole);
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
            var userRepo = new UserRepository(_context);
            var user = userRepo.GetById(userId);

            if (user == null || user.Id == _currentUser.Id)
            {
                MessageBox.Show("Нельзя заблокировать себя", "Ошибка");
                return;
            }

            user.IsBlocked = !user.IsBlocked;
            userRepo.Update(user);
            LoadUsers();

            var status = user.IsBlocked ? "заблокирован" : "разблокирован";
            MessageBox.Show($"Пользователь {user.Name} {status}", "Успешно");
        }
    }
}