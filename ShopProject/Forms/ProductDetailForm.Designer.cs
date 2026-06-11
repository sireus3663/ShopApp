namespace ShopProject.Forms
{
    partial class ProductDetailForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            mainPanel = new TableLayoutPanel();
            pbImage = new PictureBox();
            lblName = new Label();
            lblPrice = new Label();
            lblOldPrice = new Label();
            lblCategory = new Label();
            lblAmount = new Label();
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
            mainPanel.Controls.Add(lblAmount, 1, 4);
            mainPanel.Controls.Add(btnAddToCart, 1, 5);
            mainPanel.Controls.Add(lblDescription, 1, 6);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new Point(0, 0);
            mainPanel.Name = "mainPanel";
            mainPanel.RowCount = 6;
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
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
            // lblAmount
            // 
            lblAmount.AutoSize = true;
            lblAmount.Location = new Point(253, 200);
            lblAmount.Name = "lblAmount";
            lblAmount.Size = new Size(68, 30);
            lblAmount.TabIndex = 5;
            lblAmount.Text = "label5";
            // 
            // btnAddToCart
            // 
            btnAddToCart.Location = new Point(253, 253);
            btnAddToCart.Name = "btnAddToCart";
            btnAddToCart.Size = new Size(370, 40);
            btnAddToCart.TabIndex = 6;
            btnAddToCart.Text = "Добавить в коризну";
            btnAddToCart.UseVisualStyleBackColor = true;
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.Location = new Point(253, 300);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(68, 30);
            lblDescription.TabIndex = 7;
            lblDescription.Text = "label6";
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

        #endregion

        private TableLayoutPanel mainPanel;
        private PictureBox pbImage;
        private Label lblName;
        private Label lblPrice;
        private Label lblOldPrice;
        private Label lblCategory;
        private Label lblAmount;
        private Button btnAddToCart;
        private Label lblDescription;
    }
}