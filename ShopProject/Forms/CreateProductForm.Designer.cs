namespace ShopProject.Forms
{
    partial class CreateProductForm
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
            lblName = new Label();
            txtName = new TextBox();
            lblDesc = new Label();
            txtDescription = new TextBox();
            lblPrice = new Label();
            txtPrice = new TextBox();
            txtCategory = new TextBox();
            lblCategory = new Label();
            txtAmount = new TextBox();
            lblAmount = new Label();
            lblImage = new Label();
            pbPreview = new PictureBox();
            btnSelectImage = new Button();
            btnCreate = new Button();
            btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)pbPreview).BeginInit();
            SuspendLayout();
            // 
            // lblName
            // 
            lblName.Location = new Point(20, 50);
            lblName.Name = "lblName";
            lblName.Size = new Size(116, 25);
            lblName.TabIndex = 0;
            lblName.Text = "Название:";
            // 
            // txtName
            // 
            txtName.Location = new Point(20, 80);
            txtName.Name = "txtName";
            txtName.Size = new Size(400, 35);
            txtName.TabIndex = 1;
            // 
            // lblDesc
            // 
            lblDesc.Location = new Point(20, 122);
            lblDesc.Name = "lblDesc";
            lblDesc.Size = new Size(116, 25);
            lblDesc.TabIndex = 2;
            lblDesc.Text = "Описание:";
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(20, 158);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(400, 100);
            txtDescription.TabIndex = 3;
            // 
            // lblPrice
            // 
            lblPrice.AutoSize = true;
            lblPrice.Location = new Point(20, 264);
            lblPrice.Name = "lblPrice";
            lblPrice.Size = new Size(68, 30);
            lblPrice.TabIndex = 4;
            lblPrice.Text = "Цена:";
            // 
            // txtPrice
            // 
            txtPrice.Location = new Point(20, 297);
            txtPrice.Multiline = true;
            txtPrice.Name = "txtPrice";
            txtPrice.Size = new Size(180, 30);
            txtPrice.TabIndex = 5;
            // 
            // txtCategory
            // 
            txtCategory.Location = new Point(20, 363);
            txtCategory.Multiline = true;
            txtCategory.Name = "txtCategory";
            txtCategory.Size = new Size(180, 30);
            txtCategory.TabIndex = 7;
            // 
            // lblCategory
            // 
            lblCategory.AutoSize = true;
            lblCategory.Location = new Point(20, 330);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new Size(116, 30);
            lblCategory.TabIndex = 6;
            lblCategory.Text = "Категория:";
            // 
            // txtAmount
            // 
            txtAmount.Location = new Point(20, 429);
            txtAmount.Multiline = true;
            txtAmount.Name = "txtAmount";
            txtAmount.Size = new Size(180, 30);
            txtAmount.TabIndex = 9;
            // 
            // lblAmount
            // 
            lblAmount.AutoSize = true;
            lblAmount.Location = new Point(20, 396);
            lblAmount.Name = "lblAmount";
            lblAmount.Size = new Size(130, 30);
            lblAmount.TabIndex = 8;
            lblAmount.Text = "Количество:";
            // 
            // lblImage
            // 
            lblImage.AutoSize = true;
            lblImage.Location = new Point(20, 466);
            lblImage.Name = "lblImage";
            lblImage.Size = new Size(66, 30);
            lblImage.TabIndex = 10;
            lblImage.Text = "Фото:";
            // 
            // pbPreview
            // 
            pbPreview.Location = new Point(25, 499);
            pbPreview.Name = "pbPreview";
            pbPreview.Size = new Size(200, 200);
            pbPreview.SizeMode = PictureBoxSizeMode.Zoom;
            pbPreview.TabIndex = 11;
            pbPreview.TabStop = false;
            // 
            // btnSelectImage
            // 
            btnSelectImage.Location = new Point(247, 499);
            btnSelectImage.Name = "btnSelectImage";
            btnSelectImage.Size = new Size(131, 75);
            btnSelectImage.TabIndex = 12;
            btnSelectImage.Text = "Выбрать фото";
            btnSelectImage.UseVisualStyleBackColor = true;
            btnSelectImage.Click += btnSelectImage_Click_1;
            // 
            // btnCreate
            // 
            btnCreate.Location = new Point(56, 720);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(131, 40);
            btnCreate.TabIndex = 13;
            btnCreate.Text = "Создать";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click_1;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(289, 720);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(131, 40);
            btnCancel.TabIndex = 14;
            btnCancel.Text = "Отмена";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // CreateProductForm
            // 
            AutoScaleDimensions = new SizeF(12F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(476, 786);
            Controls.Add(btnCancel);
            Controls.Add(btnCreate);
            Controls.Add(btnSelectImage);
            Controls.Add(pbPreview);
            Controls.Add(lblImage);
            Controls.Add(txtAmount);
            Controls.Add(lblAmount);
            Controls.Add(txtCategory);
            Controls.Add(lblCategory);
            Controls.Add(txtPrice);
            Controls.Add(lblPrice);
            Controls.Add(txtDescription);
            Controls.Add(lblDesc);
            Controls.Add(txtName);
            Controls.Add(lblName);
            Name = "CreateProductForm";
            Text = "CreateProductForm";
            ((System.ComponentModel.ISupportInitialize)pbPreview).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblName;
        private TextBox txtName;
        private Label lblDesc;
        private TextBox txtDescription;
        private Label lblPrice;
        private TextBox txtPrice;
        private TextBox txtCategory;
        private Label lblCategory;
        private TextBox txtAmount;
        private Label lblAmount;
        private Label lblImage;
        private PictureBox pbPreview;
        private Button btnSelectImage;
        private Button btnCreate;
        private Button btnCancel;
    }
}