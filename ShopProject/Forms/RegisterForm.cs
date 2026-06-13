using System;
using System.Drawing;
using System.Windows.Forms;
using ShopProject.Services;

namespace ShopProject.Forms
{
    public class RegisterForm : Form
    {
        private readonly AuthService _authService;
        private readonly UserService _userService;

        private Label lblTitle, lblName, lblEmail, lblPassword, lblConfirm, lblError, lblSuccess;
        private TextBox txtName, txtEmail, txtPassword, txtConfirm;
        private Button btnRegister, btnBack;
        private CheckBox chkShowPassword;

        public RegisterForm(AuthService authService, UserService userService)
        {
            _authService = authService;
            _userService = userService;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "ShopProject \u2014 \u0420\u0435\u0433\u0438\u0441\u0442\u0440\u0430\u0446\u0438\u044F";
            Size = new Size(420, 600);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.White;

            lblTitle = new Label
            {
                Text = "\u0420\u0435\u0433\u0438\u0441\u0442\u0440\u0430\u0446\u0438\u044F",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize = true,
                Location = new Point(30, 35)
            };

            lblName = MakeLabel("\u0418\u043C\u044F", 100);
            txtName = MakeTextBox("\u0418\u0432\u0430\u043D \u0418\u0432\u0430\u043D\u043E\u0432", 122);

            lblEmail = MakeLabel("Email", 175);
            txtEmail = MakeTextBox("example@mail.com", 197);

            lblPassword = MakeLabel("\u041F\u0430\u0440\u043E\u043B\u044C", 250);
            txtPassword = MakeTextBox("", 272);
            txtPassword.PasswordChar = '\u25CF';

            lblConfirm = MakeLabel("\u041F\u043E\u0432\u0442\u043E\u0440\u0438\u0442\u0435 \u043F\u0430\u0440\u043E\u043B\u044C", 325);
            txtConfirm = MakeTextBox("", 347);
            txtConfirm.PasswordChar = '\u25CF';

            chkShowPassword = new CheckBox
            {
                Text = "\u041F\u043E\u043A\u0430\u0437\u0430\u0442\u044C \u043F\u0430\u0440\u043E\u043B\u0438",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(30, 390),
                AutoSize = true
            };
            chkShowPassword.CheckedChanged += (s, e) =>
            {
                char c = chkShowPassword.Checked ? '\0' : '\u25CF';
                txtPassword.PasswordChar = c;
                txtConfirm.PasswordChar = c;
            };

            lblError = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Red,
                Location = new Point(30, 415),
                Size = new Size(340, 35),
                Visible = false
            };

            lblSuccess = new Label
            {
                Text = "\u2713 \u0410\u043A\u043A\u0430\u0443\u043D\u0442 \u0441\u043E\u0437\u0434\u0430\u043D! \u041C\u043E\u0436\u0435\u0442\u0435 \u0432\u043E\u0439\u0442\u0438.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Green,
                Location = new Point(30, 415),
                Size = new Size(340, 35),
                Visible = false
            };

            btnRegister = new Button
            {
                Text = "\u0421\u043E\u0437\u0434\u0430\u0442\u044C \u0430\u043A\u043A\u0430\u0443\u043D\u0442",
                Location = new Point(30, 460),
                Size = new Size(340, 45),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(22, 163, 74),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += BtnRegister_Click;

            btnBack = new Button
            {
                Text = "\u2190 \u041D\u0430\u0437\u0430\u0434 \u043A\u043E \u0432\u0445\u043E\u0434\u0443",
                Location = new Point(30, 515),
                Size = new Size(340, 35),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                ForeColor = Color.Gray,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) => Close();

            Controls.AddRange(new Control[]
            {
                lblTitle,
                lblName, txtName,
                lblEmail, txtEmail,
                lblPassword, txtPassword,
                lblConfirm, txtConfirm,
                chkShowPassword,
                lblError, lblSuccess,
                btnRegister, btnBack
            });

            AcceptButton = btnRegister;
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;
            lblSuccess.Visible = false;

            if (string.IsNullOrWhiteSpace(txtName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                lblError.Text = "Заполните все поля";
                lblError.Visible = true;
                return;
            }

            if (!EmailValidator.IsValid(txtEmail.Text.Trim()))
            {
                lblError.Text = "Введите корректный email";
                lblError.Visible = true;
                return;
            }

            if (txtPassword.Text != txtConfirm.Text)
            {
                lblError.Text = "\u041F\u0430\u0440\u043E\u043B\u0438 \u043D\u0435 \u0441\u043E\u0432\u043F\u0430\u0434\u0430\u044E\u0442";
                lblError.Visible = true;
                return;
            }

            try
            {
                _userService.Register(txtName.Text.Trim(), txtEmail.Text.Trim(), txtPassword.Text);
                lblSuccess.Visible = true;
                btnRegister.Enabled = false;
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
                lblError.Visible = true;
            }
        }

        private Label MakeLabel(string text, int y) => new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.Gray,
            AutoSize = true,
            Location = new Point(30, y)
        };

        private TextBox MakeTextBox(string placeholder, int y) => new TextBox
        {
            Location = new Point(30, y),
            Size = new Size(340, 36),
            Font = new Font("Segoe UI", 11),
            PlaceholderText = placeholder,
            BorderStyle = BorderStyle.FixedSingle
        };
    }
}
