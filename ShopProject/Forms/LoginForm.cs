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

        private Label lblTitle = null!;
        private Label lblEmail = null!;
        private Label lblPassword = null!;
        private Label lblError = null!;
        private TextBox txtEmail = null!;
        private TextBox txtPassword = null!;
        private Button btnLogin = null!;
        private Button btnGoRegister = null!;
        private CheckBox chkShowPassword = null!;

        public LoginForm(AuthService authService, UserService userService)
        {
            _authService = authService;
            _userService = userService;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "ShopProject — Вход";
            Size = new Size(420, 520);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.White;

            lblTitle = new Label
            {
                Text = "Вход в аккаунт",
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
                Text = "Пароль",
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
                Text = "Показать пароль",
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
                Text = "Войти",
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
                Text = "Нет аккаунта? Зарегистрироваться",
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

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;
            try
            {
                var email = txtEmail.Text.Trim();
                var password = txtPassword.Text;

                if (string.IsNullOrWhiteSpace(email))
                {
                    lblError.Text = "Введите email";
                    lblError.Visible = true;
                    return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    lblError.Text = "Введите пароль";
                    lblError.Visible = true;
                    return;
                }

                await _authService.Login(email, password);

                if (_authService.currentUser != null)
                {
                    MessageBox.Show($"Добро пожаловать, {_authService.currentUser.Name}!", "Успешный вход");
                }

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
