using System;
using System.Drawing;
using System.Windows.Forms;
using ShopProject.Services;

namespace ShopProject.Forms
{
    public class LoginForm : Form
    {
        private readonly AuthService _authService;
        private readonly UserService _userService;

        private Label lblTitle, lblEmail, lblPassword, lblError;
        private TextBox txtEmail, txtPassword;
        private Button btnLogin, btnGoRegister;
        private CheckBox chkShowPassword;

        public LoginForm(AuthService authService, UserService userService)
        {
            _authService = authService;
            _userService = userService;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "ShopProject \u2014 \u0412\u0445\u043E\u0434";
            Size = new Size(420, 520);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.White;

            lblTitle = new Label
            {
                Text = "\u0412\u0445\u043E\u0434 \u0432 \u0430\u043A\u043A\u0430\u0443\u043D\u0442",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize = true,
                Location = new Point(30, 40)
            };

            lblEmail = new Label
            {
                Text = "Email",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(30, 110)
            };
            txtEmail = new TextBox
            {
                Location = new Point(30, 132),
                Size = new Size(340, 36),
                Font = new Font("Segoe UI", 11),
                PlaceholderText = "example@mail.com",
                BorderStyle = BorderStyle.FixedSingle
            };

            lblPassword = new Label
            {
                Text = "\u041F\u0430\u0440\u043E\u043B\u044C",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(30, 185)
            };
            txtPassword = new TextBox
            {
                Location = new Point(30, 207),
                Size = new Size(340, 36),
                Font = new Font("Segoe UI", 11),
                PasswordChar = '\u25CF',
                BorderStyle = BorderStyle.FixedSingle
            };

            chkShowPassword = new CheckBox
            {
                Text = "\u041F\u043E\u043A\u0430\u0437\u0430\u0442\u044C \u043F\u0430\u0440\u043E\u043B\u044C",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(30, 250),
                AutoSize = true
            };
            chkShowPassword.CheckedChanged += (s, e) =>
                txtPassword.PasswordChar = chkShowPassword.Checked ? '\0' : '\u25CF';

            lblError = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Red,
                Location = new Point(30, 280),
                Size = new Size(340, 40),
                Visible = false
            };

            btnLogin = new Button
            {
                Text = "\u0412\u043E\u0439\u0442\u0438",
                Location = new Point(30, 330),
                Size = new Size(340, 45),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            btnGoRegister = new Button
            {
                Text = "\u041D\u0435\u0442 \u0430\u043A\u043A\u0430\u0443\u043D\u0442\u0430? \u0417\u0430\u0440\u0435\u0433\u0438\u0441\u0442\u0440\u0438\u0440\u043E\u0432\u0430\u0442\u044C\u0441\u044F",
                Location = new Point(30, 390),
                Size = new Size(340, 35),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(37, 99, 235),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGoRegister.FlatAppearance.BorderSize = 0;
            btnGoRegister.Click += (s, e) =>
            {
                var reg = new RegisterForm(_authService, _userService);
                reg.ShowDialog();
            };

            Controls.AddRange(new Control[]
            {
                lblTitle, lblEmail, txtEmail,
                lblPassword, txtPassword, chkShowPassword,
                lblError, btnLogin, btnGoRegister
            });

            AcceptButton = btnLogin;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;
            try
            {
                _authService.Login(txtEmail.Text.Trim(), txtPassword.Text);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
                lblError.Visible = true;
            }
        }
    }
}
