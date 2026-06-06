using System;
using System.Drawing;
using System.Windows.Forms;
using ShopProject.Db;
using ShopProject.Models;

namespace ShopProject.WinForms
{
    public class ModeratorPanelForm : Form
    {
        private AppDbContext _context;
        private User _moderator;

        public ModeratorPanelForm(AppDbContext context, User moderator)
        {
            _context = context;
            _moderator = moderator;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Панель Модерации";
            Size = new Size(700, 500);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(32, 32, 32);

            var title = new Label 
            { 
               Text = $"Модератор - {_moderator.Name}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
            };
            Controls.Add(title);

            var moderateBtn = new Button
            {
                Text = "Открыть экран модерации товаров",
                Location = new Point(20, 70),
                Size = new Size(300, 40),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            moderateBtn.Click += (s, e) => MessageBox.Show("Экран модерации товаров", "Модерация");
            Controls.Add(moderateBtn);
        }

    }
}
