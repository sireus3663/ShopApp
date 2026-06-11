namespace ShopProject.Forms
{
    partial class ProductsForm
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
            txtSearch = new TextBox();
            btnSearch = new Button();
            cmbSort = new ComboBox();
            btnAddProduct = new Button();
            flowProducts = new FlowLayoutPanel();
            panel1 = new Panel();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(12, 12);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(400, 35);
            txtSearch.TabIndex = 0;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(420, 12);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(94, 45);
            btnSearch.TabIndex = 1;
            btnSearch.Text = "Найти";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // cmbSort
            // 
            cmbSort.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSort.FormattingEnabled = true;
            cmbSort.Items.AddRange(new object[] { "Цена: по убыванию", "Цена: по возрастанию", "Название: А-Я", "Название: Я-А" });
            cmbSort.Location = new Point(520, 12);
            cmbSort.Name = "cmbSort";
            cmbSort.Size = new Size(150, 38);
            cmbSort.TabIndex = 3;
            // 
            // btnAddProduct
            // 
            btnAddProduct.Location = new Point(732, 12);
            btnAddProduct.Name = "btnAddProduct";
            btnAddProduct.Size = new Size(202, 48);
            btnAddProduct.TabIndex = 4;
            btnAddProduct.Text = "Добавить товар";
            btnAddProduct.UseVisualStyleBackColor = true;
            btnAddProduct.Click += btnAddProduct_Click;
            // 
            // flowProducts
            // 
            flowProducts.AutoScroll = true;
            flowProducts.Dock = DockStyle.Fill;
            flowProducts.Location = new Point(0, 0);
            flowProducts.Name = "flowProducts";
            flowProducts.Padding = new Padding(10);
            flowProducts.Size = new Size(976, 636);
            flowProducts.TabIndex = 5;
            // 
            // panel1
            // 
            panel1.Controls.Add(txtSearch);
            panel1.Controls.Add(btnSearch);
            panel1.Controls.Add(btnAddProduct);
            panel1.Controls.Add(cmbSort);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(976, 60);
            panel1.TabIndex = 6;
            // 
            // ProductsForm
            // 
            AutoScaleDimensions = new SizeF(12F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(976, 636);
            Controls.Add(panel1);
            Controls.Add(flowProducts);
            Name = "ProductsForm";
            Text = "ProductsForm";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TextBox txtSearch;
        private Button btnSearch;
        private ComboBox cmbSort;
        private Button btnAddProduct;
        private FlowLayoutPanel flowProducts;
        private Panel panel1;
    }
}