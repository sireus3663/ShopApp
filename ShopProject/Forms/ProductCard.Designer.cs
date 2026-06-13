namespace ShopProject.Forms
{
    partial class ProductCard
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pbImage = new PictureBox();
            lblName = new Label();
            lblPrice = new Label();
            lblDiscountPrice = new Label();
            btnAddToCart = new Button();
            ((System.ComponentModel.ISupportInitialize)pbImage).BeginInit();
            SuspendLayout();
            // 
            // pbImage
            // 
            pbImage.Location = new Point(10, 10);
            pbImage.Name = "pbImage";
            pbImage.Size = new Size(200, 180);
            pbImage.SizeMode = PictureBoxSizeMode.Zoom;
            pbImage.TabIndex = 0;
            pbImage.TabStop = false;
            // 
            // lblName
            // 
            lblName.AutoEllipsis = true;
            lblName.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);
            lblName.Location = new Point(10, 200);
            lblName.Name = "lblName";
            lblName.Size = new Size(200, 40);
            lblName.TabIndex = 1;
            // 
            // lblPrice
            // 
            lblPrice.ImageAlign = ContentAlignment.MiddleLeft;
            lblPrice.Location = new Point(10, 233);
            lblPrice.Name = "lblPrice";
            lblPrice.Size = new Size(100, 30);
            lblPrice.TabIndex = 2;
            // 
            // lblDiscountPrice
            // 
            lblDiscountPrice.Location = new Point(110, 233);
            lblDiscountPrice.Name = "lblDiscountPrice";
            lblDiscountPrice.Size = new Size(70, 30);
            lblDiscountPrice.TabIndex = 3;
            lblDiscountPrice.TextAlign = ContentAlignment.MiddleRight;
            lblDiscountPrice.Visible = false;
            // 
            // btnAddToCart
            // 
            btnAddToCart.FlatStyle = FlatStyle.Flat;
            btnAddToCart.ForeColor = SystemColors.ActiveCaptionText;
            btnAddToCart.Location = new Point(10, 273);
            btnAddToCart.Name = "btnAddToCart";
            btnAddToCart.Size = new Size(200, 40);
            btnAddToCart.TabIndex = 4;
            btnAddToCart.Text = "\U0001f6d2";
            btnAddToCart.UseVisualStyleBackColor = true;
            btnAddToCart.Click += Button1_Click;
            // 
            // ProductCard
            // 
            AutoScaleDimensions = new SizeF(12F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(btnAddToCart);
            Controls.Add(lblDiscountPrice);
            Controls.Add(lblPrice);
            Controls.Add(lblName);
            Controls.Add(pbImage);
            Name = "ProductCard";
            Size = new Size(220, 320);
            ((System.ComponentModel.ISupportInitialize)pbImage).EndInit();
            ResumeLayout(false);
        }

        private PictureBox pbImage;
        private Label lblName;
        private Label lblPrice;
        private Label lblDiscountPrice;
        private Button btnAddToCart;
    }
}
