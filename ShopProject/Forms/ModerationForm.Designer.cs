namespace ShopProject.Forms
{
    partial class ModerationForm
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

        private FlowLayoutPanel flowModeration;
    }
}
