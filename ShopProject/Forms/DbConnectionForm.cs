using System;
using System.Drawing;
using System.Windows.Forms;
using ShopProject.Services;

namespace ShopProject.Forms
{
    public class DbConnectionForm : Form
    {
        private readonly AppConfigService _configService;

        private Label lblTitle, lblError, lblHost, lblPort, lblDb, lblUser, lblPass;
        private TextBox txtHost, txtPort, txtDb, txtUser, txtPass;
        private Button btnConnect, btnExit;
        private Label lblStatus;

        public DbConnectionForm(AppConfigService configService, string errorMessage = "")
        {
            _configService = configService;
            InitializeComponents(errorMessage);
        }

        private void InitializeComponents(string errorMessage)
        {
            this.Text = "ShopProject — Подключение к базе данных";
            this.Size = new Size(420, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            lblTitle = new Label
            {
                Text = "Подключение к базе данных",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(25, 20),
                Size = new Size(360, 30)
            };

            lblError = new Label
            {
                Text = string.IsNullOrEmpty(errorMessage)
                    ? "Не удалось подключиться. Проверьте данные."
                    : errorMessage,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(239, 68, 68),
                Location = new Point(25, 55),
                Size = new Size(360, 35)
            };
            ParseCurrentConnection(out string host, out string port, out string db, out string user);

            int y = 100;
            lblHost = MakeLabel("Host", y);
            txtHost = MakeTextBox(host, y + 22);

            y = 160;
            lblPort = MakeLabel("Port", y);
            txtPort = MakeTextBox(port, y + 22);

            y = 220;
            lblDb = MakeLabel("Database", y);
            txtDb = MakeTextBox(db, y + 22);

            y = 280;
            lblUser = MakeLabel("Username", y);
            txtUser = MakeTextBox(user, y + 22);

            y = 340;
            lblPass = MakeLabel("Password", y);
            txtPass = MakeTextBox("", y + 22);
            txtPass.PasswordChar = '●';
            txtPass.PlaceholderText = "Введите пароль...";

            lblStatus = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(239, 68, 68),
                Location = new Point(25, 385),
                Size = new Size(360, 20)
            };

            btnConnect = new Button
            {
                Text = "Подключиться",
                Location = new Point(25, 410),
                Size = new Size(220, 42),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnConnect.FlatAppearance.BorderSize = 0;
            btnConnect.Click += BtnConnect_Click;

            btnExit = new Button
            {
                Text = "Выйти",
                Location = new Point(260, 410),
                Size = new Size(130, 42),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            this.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblTitle, lblError,
                lblHost, txtHost,
                lblPort, txtPort,
                lblDb, txtDb,
                lblUser, txtUser,
                lblPass, txtPass,
                lblStatus, btnConnect, btnExit
            });

            this.AcceptButton = btnConnect;
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Подключение...";
            lblStatus.ForeColor = Color.Gray;
            btnConnect.Enabled = false;
            this.Refresh();

            try
            {
                _configService.UpdateConnectionString(
                    txtHost.Text.Trim(),
                    txtPort.Text.Trim(),
                    txtDb.Text.Trim(),
                    txtUser.Text.Trim(),
                    txtPass.Text
                );
                var ctx = new ShopProject.Db.AppDbContext();
                if (!ctx.Database.CanConnect())
                    throw new Exception("Не удалось подключиться к базе данных.");
                ctx.Dispose();

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Ошибка: " + ex.Message.Split('\n')[0];
                lblStatus.ForeColor = Color.FromArgb(239, 68, 68);
                btnConnect.Enabled = true;
            }
        }

        private void ParseCurrentConnection(out string host, out string port, out string db, out string user)
        {
            host = "localhost"; port = "5432"; db = "marketplace"; user = "postgres";
            try
            {
                var conn = _configService.GetConnectionString();
                foreach (var part in conn.Split(';'))
                {
                    var kv = part.Split('=');
                    if (kv.Length != 2) continue;
                    switch (kv[0].Trim().ToLower())
                    {
                        case "host":     host = kv[1].Trim(); break;
                        case "port":     port = kv[1].Trim(); break;
                        case "database": db   = kv[1].Trim(); break;
                        case "username": user = kv[1].Trim(); break;
                    }
                }
            }
            catch { }
        }

        private Label MakeLabel(string text, int y) => new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.Gray,
            Location = new Point(25, y),
            AutoSize = true
        };

        private TextBox MakeTextBox(string value, int y) => new TextBox
        {
            Text = value,
            Location = new Point(25, y),
            Size = new Size(360, 30),
            Font = new Font("Segoe UI", 10),
            BorderStyle = BorderStyle.FixedSingle
        };
    }
}
