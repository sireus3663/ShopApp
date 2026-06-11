using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ShopProject.Models;

namespace ShopProject.Forms
{
    public partial class ProductCard : UserControl
    {
        private Guid _productId;
        public event Action<Product> ProductClicked;
        private Product _currentProduct;

        public ProductCard()
        {
            InitializeComponent();

            this.Click += (s, e) => ProductClicked?.Invoke(_currentProduct);
            foreach (Control ctrl in this.Controls)
            {
                ctrl.Click += (s, e) => ProductClicked?.Invoke(_currentProduct);
            }
        }
        public void SetProduct(Product product, decimal finalPrice)
        {
            _currentProduct = product;
            _productId = product.Id;
            lblName.Text = product.Name;
            lblPrice.Text = $"{product.Price:N0} ₽";

            if (finalPrice < product.Price)
            {
                lblDiscountPrice.Text = $"{finalPrice:N0} ₽";
                lblDiscountPrice.Visible = true;
                lblPrice.ForeColor = Color.Gray;
                lblPrice.Font = new Font(lblPrice.Font, FontStyle.Strikeout);
            }
            else
            {
                lblDiscountPrice.Visible = false;
                lblPrice.ForeColor = Color.Black;
                lblPrice.Font = new Font(lblPrice.Font, FontStyle.Regular);
            }

            pbImage.Image = ConvertBytesToImage(product.ProductImage);
            if (pbImage.Image == null)
            {
                pbImage.BackColor = Color.LightGray;
            }
        }

        public event Action<Guid> AddToCartClicked;

        private Image? ConvertBytesToImage(byte[]? imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0) return null;
            using (var ms = new MemoryStream(imageBytes))
            {
                return Image.FromStream(ms);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            AddToCartClicked?.Invoke(_productId);
        }
    }
}
