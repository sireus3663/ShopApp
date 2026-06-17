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
            SuspendLayout();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new Point(0, 0);
            mainPanel.Size = new Size(800, 500);
            mainPanel.TabIndex = 0;
            pbImage.Location = new Point(0, 0);
            pbImage.Size = new Size(200, 200);
            pbImage.SizeMode = PictureBoxSizeMode.Zoom;
            pbImage.TabIndex = 0;
            pbImage.TabStop = false;
            lblName.Text = "";
            lblPrice.Text = "";
            lblOldPrice.Text = "";
            lblCategory.Text = "";
            lblDescription.Text = "";
            btnAddToCart.Text = "";
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 500);
            ControlBox = false;
            Controls.Add(mainPanel);
            FormBorderStyle = FormBorderStyle.None;
            Name = "ProductDetailForm";
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
