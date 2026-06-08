using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ShopProject.Db;
using ShopProject.Models;

namespace ShopProject.WinForms
{
    public class ModeratorPanelForm : Form
    {
        private AppDbContext _context;
        private User _moderator;
        private DataGridView productsGrid;
        private Button approveBtn;
        private Button declineBtn;
        private Button refreshBtn;

        public ModeratorPanelForm(AppDbContext context, User moderator)
        {
            _context = context;
            _moderator = moderator;
            InitializeComponent();
            LoadProductsForModeration();
        }

        private void InitializeComponent()
        {
            this.Text = "Панель модерации";
            this.Size = new Size(900, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);

            var title = new Label
            {
                Text = $"Модерация товаров - {_moderator.Name}",
                ForeColor = Color.FromArgb(60, 60, 60),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            Controls.Add(title);

            refreshBtn = new Button
            {
                Text = "Обновить",
                Location = new Point(20, 60),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            refreshBtn.Click += (s, e) => LoadProductsForModeration();
            Controls.Add(refreshBtn);

            productsGrid = new DataGridView
            {
                Location = new Point(20, 100),
                Size = new Size(850, 350),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            Controls.Add(productsGrid);

            approveBtn = new Button
            {
                Text = "Одобрить",
                Location = new Point(20, 470),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(80, 120, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            approveBtn.Click += ApproveProduct_Click;
            Controls.Add(approveBtn);

            declineBtn = new Button
            {
                Text = "Отклонить",
                Location = new Point(150, 470),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(180, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            declineBtn.Click += DeclineProduct_Click;
            Controls.Add(declineBtn);

            var closeBtn = new Button
            {
                Text = "Закрыть",
                Location = new Point(770, 470),
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(180, 180, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            closeBtn.Click += (s, e) => this.Close();
            Controls.Add(closeBtn);
        }

        private void LoadProductsForModeration()
        {
            try
            {
                var repo = new ProductRepository(_context);
                var products = repo.GetAll()
                    .Where(p => !p.IsApproved)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Price,
                        p.Category,
                        Seller = p.SellerId.ToString()
                    })
                    .ToList();

                productsGrid.DataSource = products;
                productsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
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
            var productRepo = new ProductRepository(_context);
            var product = productRepo.GetById(productId);

            if (product != null)
            {
                product.IsApproved = true;
                productRepo.Update(product);
                LoadProductsForModeration();
                MessageBox.Show($"Товар '{product.Name}' одобрен", "Успешно");
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
            var productRepo = new ProductRepository(_context);
            var product = productRepo.GetById(productId);

            if (product != null)
            {
                productRepo.Delete(productId);
                LoadProductsForModeration();
                MessageBox.Show($"Товар '{product.Name}' отклонён", "Успешно");
            }
        }
    }
}