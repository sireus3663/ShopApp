using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Forms.ViewModels;

namespace ShopProject.Forms
{
    public class ModeratorPanelForm : Form
    {
        private DataGridView productsGrid;
        private Button approveBtn;
        private Button declineBtn;
        private Button refreshBtn;
        private ModeratorViewModel _viewModel;

        public ModeratorPanelForm(AppDbContext context, User moderator)
        {
            var productRepo = new ProductRepository(context);
            _viewModel = new ModeratorViewModel(productRepo);
            InitializeComponent();
            LoadProductsForModeration();
        }

        private void InitializeComponent()
        {
            this.Text = "Панель модерации";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            var title = new Label
            {
                Text = "Модерация товаров",
                ForeColor = Color.FromArgb(60, 60, 60),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            topPanel.Controls.Add(title);

            refreshBtn = new Button
            {
                Text = "Обновить",
                Location = new Point(20, 55),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            refreshBtn.Click += (s, e) => LoadProductsForModeration();
            topPanel.Controls.Add(refreshBtn);

            productsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(10, 10, 10, 10)
            };

            approveBtn = new Button
            {
                Text = "Одобрить",
                Location = new Point(10, 10),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(80, 120, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            approveBtn.Click += ApproveProduct_Click;
            bottomPanel.Controls.Add(approveBtn);

            declineBtn = new Button
            {
                Text = "Отклонить",
                Location = new Point(140, 10),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(180, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            declineBtn.Click += DeclineProduct_Click;
            bottomPanel.Controls.Add(declineBtn);

            var closeBtn = new Button
            {
                Text = "Закрыть",
                Location = new Point(770, 10),
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(180, 180, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            closeBtn.Click += (s, e) => this.Close();
            bottomPanel.Controls.Add(closeBtn);

            this.Controls.Add(productsGrid);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(topPanel);
        }

        private void LoadProductsForModeration()
        {
            try
            {
                var products = _viewModel.GetProductsForModeration();
                productsGrid.DataSource = products.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Category,
                    Описание = p.Description != null && p.Description.Length > 50
                        ? p.Description.Substring(0, 47) + "..."
                        : (p.Description ?? "Нет описания"),
                    Продавец = p.SellerId?.ToString() ?? "Не указан"
                }).ToList();
            }
            catch (Exception ex)
            {
                ErrorForm.Show(ex.Message, ex);
            }
        }

        private void ApproveProduct_Click(object sender, EventArgs e)
        {
            if (productsGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите товар", "Ошибка");
                return;
            }

            var productId = (Guid)productsGrid.SelectedRows[0].Cells["Id"].Value;

            try
            {
                _viewModel.ApproveProduct(productId);
                LoadProductsForModeration();
                MessageBox.Show("Товар одобрен", "Успешно");
            }
            catch (Exception ex)
            {
                ErrorForm.Show(ex.Message, ex);
            }
        }

        private void DeclineProduct_Click(object sender, EventArgs e)
        {
            if (productsGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите товар", "Ошибка");
                return;
            }

            var productId = (Guid)productsGrid.SelectedRows[0].Cells["Id"].Value;

            try
            {
                _viewModel.DeclineProduct(productId);
                LoadProductsForModeration();
                MessageBox.Show("Товар отклонён", "Успешно");
            }
            catch (Exception ex)
            {
                ErrorForm.Show(ex.Message, ex);
            }
        }
    }
}