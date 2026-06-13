namespace ShopProject.Forms
{
    partial class ProductDetailForm
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
            mainPanel = new TableLayoutPanel();
            pbImage = new PictureBox();
            lblName = new Label();
            lblPrice = new Label();
            lblOldPrice = new Label();
            lblCategory = new Label();
            btnAddToCart = new Button();
            lblDescription = new Label();
            mainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbImage).BeginInit();
            SuspendLayout();
            // 
            // mainPanel
            // 
            mainPanel.ColumnCount = 2;
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            mainPanel.Controls.Add(pbImage, 0, 0);
            mainPanel.Controls.Add(lblName, 1, 0);
            mainPanel.Controls.Add(lblPrice, 1, 1);
            mainPanel.Controls.Add(lblOldPrice, 1, 2);
            mainPanel.Controls.Add(lblCategory, 1, 3);
            mainPanel.Controls.Add(btnAddToCart, 1, 4);
            mainPanel.Controls.Add(lblDescription, 1, 5);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new Point(0, 0);
            mainPanel.Name = "mainPanel";
            mainPanel.RowCount = 7;
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            mainPanel.RowStyles.Add(new RowStyle());
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            mainPanel.Size = new Size(626, 586);
            mainPanel.TabIndex = 0;
            // 
            // pbImage
            // 
            pbImage.Dock = DockStyle.Fill;
            pbImage.Location = new Point(3, 3);
            pbImage.Name = "pbImage";
            mainPanel.SetRowSpan(pbImage, 7);
            pbImage.Size = new Size(244, 580);
            pbImage.SizeMode = PictureBoxSizeMode.Zoom;
            pbImage.TabIndex = 0;
            pbImage.TabStop = false;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Location = new Point(253, 0);
            lblName.Name = "lblName";
            lblName.Size = new Size(68, 30);
            lblName.TabIndex = 1;
            lblName.Text = "label1";
            // 
            // lblPrice
            // 
            lblPrice.AutoSize = true;
            lblPrice.Location = new Point(253, 50);
            lblPrice.Name = "lblPrice";
            lblPrice.Size = new Size(68, 30);
            lblPrice.TabIndex = 2;
            lblPrice.Text = "label2";
            // 
            // lblOldPrice
            // 
            lblOldPrice.AutoSize = true;
            lblOldPrice.Location = new Point(253, 100);
            lblOldPrice.Name = "lblOldPrice";
            lblOldPrice.Size = new Size(68, 30);
            lblOldPrice.TabIndex = 3;
            lblOldPrice.Text = "label3";
            // 
            // lblCategory
            // 
            lblCategory.AutoSize = true;
            lblCategory.Location = new Point(253, 150);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new Size(68, 30);
            lblCategory.TabIndex = 4;
            lblCategory.Text = "label4";
            // 
            // btnAddToCart
            // 
            btnAddToCart.Location = new Point(253, 200);
            btnAddToCart.Name = "btnAddToCart";
            btnAddToCart.Size = new Size(370, 40);
            btnAddToCart.TabIndex = 5;
            btnAddToCart.Text = "\u0414\u043E\u0431\u0430\u0432\u0438\u0442\u044C \u0432 \u043A\u043E\u0440\u0437\u0438\u043D\u0443";
            btnAddToCart.UseVisualStyleBackColor = true;
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.Location = new Point(253, 250);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(68, 30);
            lblDescription.TabIndex = 6;
            lblDescription.Text = "label5";
            // 
            // ProductDetailForm
            // 
            AutoScaleDimensions = new SizeF(12F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(626, 586);
            Controls.Add(mainPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            Name = "ProductDetailForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "ProductDetailForm";
            mainPanel.ResumeLayout(false);
            mainPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pbImage).EndInit();
            ResumeLayout(false);
        }

        private TableLayoutPanel mainPanel;
        private PictureBox pbImage;
        private Label lblName;
        private Label lblPrice;
        private Label lblOldPrice;
        private Label lblCategory;
        private Button btnAddToCart;
        private Label lblDescription;
    }
}
