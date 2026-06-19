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

        private Label lblTitle = null!;
        private Label lblName = null!;
        private Label lblEmail = null!;
        private Label lblPassword = null!;
        private Label lblConfirm = null!;
        private Label lblError = null!;
        private Label lblSuccess = null!;
        private TextBox txtName = null!;
        private TextBox txtEmail = null!;
        private TextBox txtPassword = null!;
        private TextBox txtConfirm = null!;
        private Button btnRegister = null!;
        private Button btnBack = null!;
        private CheckBox chkShowPassword = null!;

        public RegisterForm(AuthService authService, UserService userService)
        {
            _authService = authService;
            _userService = userService;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "ShopProject — Регистрация";
            Size = new Size(420, 600);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.White;

            lblTitle = new Label
            {
                Text = "Регистрация",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize = true,
                Location = new Point(30, 35)
            };

            lblName = MakeLabel("Имя", 100);
            txtName = MakeTextBox("Иван Иванов", 122);

            lblEmail = MakeLabel("Email", 175);
            txtEmail = MakeTextBox("example@mail.com", 197);

            lblPassword = MakeLabel("Пароль", 250);
            txtPassword = MakeTextBox("", 272);
            txtPassword.PasswordChar = '\u25CF';

            lblConfirm = MakeLabel("Повторите пароль", 325);
            txtConfirm = MakeTextBox("", 347);
            txtConfirm.PasswordChar = '\u25CF';

            chkShowPassword = new CheckBox
            {
                Text = "Показать пароли",
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
                Text = "✓ Аккаунт создан! Можете войти.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Green,
                Location = new Point(30, 415),
                Size = new Size(340, 35),
                Visible = false
            };

            btnRegister = new Button
            {
                Text = "Создать аккаунт",
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
                Text = "← Назад ко входу",
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

            if (txtName.Text.Length < 2)
            {
                lblError.Text = "Имя должно содержать минимум 2 символа";
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
                lblError.Text = "Пароли не совпадают";
                lblError.Visible = true;
                return;
            }

            if (txtPassword.Text.Length < 8)
            {
                lblError.Text = "Пароль должен содержать минимум 8 символов";
                lblError.Visible = true;
                return;
            }

            try
            {
                _userService.Register(txtName.Text.Trim(), txtEmail.Text.Trim(), txtPassword.Text);
                lblSuccess.Visible = true;
                btnRegister.Enabled = false;
                txtName.Enabled = false;
                txtEmail.Enabled = false;
                txtPassword.Enabled = false;
                txtConfirm.Enabled = false;
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
