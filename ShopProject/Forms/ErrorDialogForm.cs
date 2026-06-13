using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShopProject.Forms
{
    public class ErrorDialogForm : Form
    {
        private Label lblMessage;
        private Button btnOk;

        public ErrorDialogForm(string message)
        {
            InitializeComponent();
            lblMessage.Text = message;
        }

        private void InitializeComponent()
        {
            Text = "Ошибка";
            Size = new Size(500, 220);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(32, 32, 32);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            lblMessage = new Label()
            {
                ForeColor = Color.White,
                Location = new Point(20, 20),
                Size = new Size(440, 100),
                AutoSize = false,
            };

            btnOk = new Button()
            {
                Text = "OK",
                Location = new Point(200, 130),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(200, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOk.Click += (s, e) => this.Close();

            Controls.Add(lblMessage);
            Controls.Add(btnOk);
        }
        public static void ShowError(string message)
        {
            try
            {
                using (var dlg = new ErrorDialogForm(message))
                {
                    dlg.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
