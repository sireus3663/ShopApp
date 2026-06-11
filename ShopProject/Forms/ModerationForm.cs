using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ShopProject.Models;
using ShopProject.Services;

namespace ShopProject.Forms
{
    public partial class ModerationForm : Form
    {
        private readonly AuthService _authService;
        private readonly ProductService _productService;

        public ModerationForm(AuthService authService, ProductService productService)
        {
            _authService = authService;
            _productService = productService;
            InitializeComponent();

            var currentUser = _authService.currentUser;
            if (!PermissionService.CanModerate(currentUser?.Role ?? Role.Buyer))
            {
                MessageBox.Show("У вас нет прав для модерации");
                this.Close();
                return;
            }

            LoadProducts();
        }

        private void LoadProducts()
        {
            flowModeration.Controls.Clear();
            var products = _productService.GetForModerate();

            foreach (var product in products)
            {
                var panel = CreateProductPanel(product);
                flowModeration.Controls.Add(panel);
            }
        }

       private Panel CreateProductPanel(Product product)
{
    var panel = new Panel
    {
        Width = 800,
        Height = 100,
        BorderStyle = BorderStyle.FixedSingle,
        Margin = new Padding(5),
        BackColor = Color.White
    };

    var pbImage = new PictureBox
    {
        Location = new Point(10, 10),
        Size = new Size(80, 80),
        SizeMode = PictureBoxSizeMode.Zoom
    };

    if (product.ProductImage != null && product.ProductImage.Length > 0)
    {
        using (var ms = new MemoryStream(product.ProductImage))
        {
            pbImage.Image = Image.FromStream(ms);
        }
    }
    else
    {
        pbImage.BackColor = Color.LightGray;
    }

    var lblName = new Label
    {
        Text = product.Name,
        Location = new Point(100, 10),
        Size = new Size(200, 25),
        Font = new Font("Segoe UI", 10, FontStyle.Bold)
    };

    var lblCategory = new Label
    {
        Text = "Категория: " + product.Category,
        Location = new Point(100, 40),
        Size = new Size(200, 25)
    };

    var lblPrice = new Label
    {
        Text = product.Price.ToString("N0") + " ₽",
        Location = new Point(100, 65),
        Size = new Size(150, 25)
    };

    var desc = product.Description.Length > 80 
        ? product.Description.Substring(0, 80) + "..." 
        : product.Description;

    var lblDescription = new Label
    {
        Text = desc,
        Location = new Point(300, 10),
        Size = new Size(350, 80)
    };

    var btnApprove = new Button
    {
        Text = "Одобрить",
        Location = new Point(680, 15),
        Size = new Size(100, 30),
        BackColor = Color.Green,
        ForeColor = Color.White
    };
    btnApprove.Click += (s, e) => ApproveProduct(product);

    var btnDecline = new Button
    {
        Text = "Отклонить",
        Location = new Point(680, 55),
        Size = new Size(100, 30),
        BackColor = Color.Red,
        ForeColor = Color.White
    };
    btnDecline.Click += (s, e) => DeclineProduct(product);

    panel.Controls.Add(pbImage);
    panel.Controls.Add(lblName);
    panel.Controls.Add(lblCategory);
    panel.Controls.Add(lblPrice);
    panel.Controls.Add(lblDescription);
    panel.Controls.Add(btnApprove);
    panel.Controls.Add(btnDecline);

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
                MessageBox.Show(ex.Message);
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
                MessageBox.Show(ex.Message);
            }
        }
    }
}