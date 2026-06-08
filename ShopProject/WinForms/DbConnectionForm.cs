using System;
using System.Drawing;
using System.Windows.Forms;
using ShopProject.Db;
using ShopProject.Services;

namespace ShopProject.WinForms
{
    public class DbConnectionForm : Form
    {
        private TextBox txtHost, txtPort, txtDatabase, txtUsername, txtPassword;
        private Button btnConnect, btnCancel;
        private Label lblStatus;
        private AppConfigService _configService;
        private StartupService _startup;
        public AppDbContext ConnectedContext { get; private set; }

        public DbConnectionForm()
        {
            _configService = new AppConfigService();
            _startup = new StartupService(new LoggerService(), _configService);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Подключение к базе данных";
            this.Size = new Size(450, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int y = 30;
            int left = 50;
            int labelWidth = 100;
            int controlWidth = 250;

            AddLabel("Host:", left, y + 5);
            txtHost = AddTextBox("localhost", left + labelWidth, y, controlWidth);
            y += 45;

            AddLabel("Port:", left, y + 5);
            txtPort = AddTextBox("5432", left + labelWidth, y, controlWidth);
            y += 45;

            AddLabel("Database:", left, y + 5);
            txtDatabase = AddTextBox("marketplace", left + labelWidth, y, controlWidth);
            y += 45;

            AddLabel("Username:", left, y + 5);
            txtUsername = AddTextBox("postgres", left + labelWidth, y, controlWidth);
            y += 45;

            AddLabel("Password:", left, y + 5);
            txtPassword = AddTextBox("", left + labelWidth, y, controlWidth);
            txtPassword.PasswordChar = '*';
            y += 60;

            lblStatus = new Label
            {
                Location = new Point(left, y),
                Size = new Size(350, 40),
                ForeColor = Color.FromArgb(180, 100, 100),
                Text = "Введите данные для подключения"
            };
            this.Controls.Add(lblStatus);
            y += 50;

            btnConnect = new Button
            {
                Text = "Подключиться",
                Location = new Point(left, y),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(80, 80, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnConnect.Click += BtnConnect_Click;

            btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(left + 140, y),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(180, 180, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(btnConnect);
            this.Controls.Add(btnCancel);
        }

        private void AddLabel(string text, int x, int y)
        {
            var label = new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(100, 25),
                ForeColor = Color.FromArgb(60, 60, 60)
            };
            this.Controls.Add(label);
        }

        private TextBox AddTextBox(string defaultValue, int x, int y, int width)
        {
            var box = new TextBox
            {
                Text = defaultValue,
                Location = new Point(x, y),
                Size = new Size(width, 30),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(60, 60, 60),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(box);
            return box;
        }

        private async void BtnConnect_Click(object sender, EventArgs e)
        {
            _configService.UpdateConnectionString(
                txtHost.Text, txtPort.Text, txtDatabase.Text,
                txtUsername.Text, txtPassword.Text);

            lblStatus.ForeColor = Color.FromArgb(180, 150, 50);
            lblStatus.Text = "Подключение...";
            btnConnect.Enabled = false;

            await Task.Run(() =>
            {
                ConnectedContext = _startup.TryConnect();
            });

            if (ConnectedContext != null)
            {
                lblStatus.ForeColor = Color.FromArgb(80, 120, 80);
                lblStatus.Text = "Подключено успешно!";
                await Task.Delay(500);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                lblStatus.ForeColor = Color.FromArgb(180, 100, 100);
                lblStatus.Text = "Ошибка подключения! Проверьте данные.";
                btnConnect.Enabled = true;
            }
        }
    }
}