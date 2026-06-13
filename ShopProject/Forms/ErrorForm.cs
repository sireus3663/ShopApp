using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShopProject.Forms
{
    public class ErrorForm : Form
    {
        private Label messageLabel;
        private TextBox detailsBox;
        private Button okButton;
        private Button copyButton;

        public ErrorForm(string message, Exception ex = null)
        {
            InitializeComponent();
            messageLabel.Text = message;
            if (ex != null)
            {
                detailsBox.Text = ex.Message + "\n\n" + ex.StackTrace;
                detailsBox.Visible = true;
                this.Height = 400;
                copyButton.Visible = true;
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Ошибка";
            this.Size = new Size(500, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            messageLabel = new Label
            {
                Location = new Point(30, 30),
                Size = new Size(440, 60),
                ForeColor = Color.FromArgb(60, 60, 60),
                Font = new Font("Segoe UI", 10)
            };

            detailsBox = new TextBox
            {
                Location = new Point(20, 100),
                Size = new Size(460, 120),
                ForeColor = Color.FromArgb(80, 80, 80),
                BackColor = Color.FromArgb(250, 250, 250),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Visible = false
            };

            copyButton = new Button
            {
                Text = "Копировать",
                Location = new Point(20, 180),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = false
            };
            copyButton.Click += (s, e) => { Clipboard.SetText(detailsBox.Text); };

            okButton = new Button
            {
                Text = "OK",
                Location = new Point(380, 180),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            okButton.Click += (s, e) => this.Close();

            this.Controls.Add(messageLabel);
            this.Controls.Add(detailsBox);
            this.Controls.Add(copyButton);
            this.Controls.Add(okButton);
        }

        public static void Show(string message, Exception ex = null)
        {
            var form = new ErrorForm(message, ex);
            form.ShowDialog();
        }
    }
}