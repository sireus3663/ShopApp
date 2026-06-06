using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShopProject.WinForms
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
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Ошибка";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(32, 32, 32);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var iconLabel = new Label
            {
                Text = "❌",
                Font = new Font("Segoe UI", 24),
                ForeColor = Color.Red,
                Location = new Point(20, 20),
                AutoSize = true
            };

            messageLabel = new Label
            {
                Location = new Point(80, 25),
                Size = new Size(380, 40),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            detailsBox = new TextBox
            {
                Location = new Point(20, 80),
                Size = new Size(440, 150),
                ForeColor = Color.LightGray,
                BackColor = Color.FromArgb(45, 45, 45),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Visible = false
            };

            copyButton = new Button
            {
                Text = "Копировать",
                Location = new Point(20, 240),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(64, 64, 64),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            copyButton.Click += (s, e) => { Clipboard.SetText(detailsBox.Text); };

            okButton = new Button
            {
                Text = "OK",
                Location = new Point(380, 240),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            okButton.Click += (s, e) => this.Close();

            this.Controls.Add(iconLabel);
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