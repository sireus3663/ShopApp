namespace ShopProject.Forms
{
    partial class ModerationForm
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
            flowModeration = new FlowLayoutPanel();
            SuspendLayout();
            // 
            // flowModeration
            // 
            flowModeration.AutoScroll = true;
            flowModeration.Dock = DockStyle.Fill;
            flowModeration.Location = new Point(0, 0);
            flowModeration.Name = "flowModeration";
            flowModeration.Padding = new Padding(10);
            flowModeration.Size = new Size(1426, 1436);
            flowModeration.TabIndex = 0;
            // 
            // ModerationForm
            // 
            AutoScaleDimensions = new SizeF(12F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1426, 1436);
            Controls.Add(flowModeration);
            Name = "ModerationForm";
            Text = "ModerationForm";
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel flowModeration;
    }
}