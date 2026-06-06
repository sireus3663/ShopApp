using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using ShopProject.Db;
using ShopProject.Models;

namespace ShopProject.WinForms
{
    public class AdminPanelForm : Form
    {
        private AppDbContext _context;
        private User _currentUser;

        public AdminPanelForm(AppDbContext context, User currentUser)
        {
            _context = context;
            _currentUser = currentUser;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Панель Администратора";
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(32, 32, 32);

            var title = new Label
            {
                Text = $"Администрирование - {_currentUser.Name}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            Controls.Add(title);

            var usersBtn = new Button
            {
                Text = "Управление пользователями",
                Location = new Point(20, 70),
                Size = new Size(220, 40),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            usersBtn.Click += (s, e) => MessageBox.Show("Экран управления пользователями", "Пользователи");
            Controls.Add(usersBtn);

            var productsBtn = new Button
            {
                Text = "Управление товарами",
                Location = new Point(20, 130),
                Size = new Size(220, 40),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            productsBtn.Click += (s, e) => MessageBox.Show("Экран управления товарами", "Товары");
            Controls.Add(productsBtn);

            var consoleBtn = new Button
            {
                Text = "Открыть консоль",
                Location = new Point(20, 190),
                Size = new Size(220, 40),
                BackColor = Color.FromArgb(80, 80, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            consoleBtn.Click += (s, e) =>
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        UseShellExecute = true,
                    };
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось открыть консоль: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            Controls.Add(consoleBtn);

            var errorBtn = new Button
            {
                Text = "Показать ошибку (Тест)",
                Location = new Point(20, 250),
                Size = new Size(220, 40),
                BackColor = Color.FromArgb(200, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            errorBtn.Click += (s, e) => ErrorDialogForm.ShowError("Тестовая ошибка - описание...");
            Controls.Add(errorBtn);
        }
    }
}