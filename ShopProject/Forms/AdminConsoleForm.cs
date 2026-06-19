using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace ShopProject.Forms
{
    public class AdminConsoleForm : Form
    {
        private User _currentUser;
        private RichTextBox outputBox;
        private TextBox inputBox;
        private Button startBtn;
        private Button restartBtn;
        private Button clearBtn;
        private Process process;
        private List<string> commandHistory;
        private int historyIndex;

        public AdminConsoleForm(User currentUser)
        {
            _currentUser = currentUser;
            commandHistory = new List<string>();
            historyIndex = 0;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Admin Console — ShopApp";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.Black;
            this.ForeColor = Color.LimeGreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(600, 400);

            var title = new Label
            {
                Text = $"Admin Console — {_currentUser.Name}",
                ForeColor = Color.LimeGreen,
                Font = new Font("Consolas", 14, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            Controls.Add(title);

            var statusLabel = new Label
            {
                Text = "Статус: Остановлен",
                ForeColor = Color.Gray,
                Font = new Font("Consolas", 9),
                Location = new Point(10, 35),
                AutoSize = true,
                Tag = "statusLabel"
            };
            Controls.Add(statusLabel);

            outputBox = new RichTextBox
            {
                Location = new Point(10, 60),
                Size = new Size(860, 460),
                BackColor = Color.Black,
                ForeColor = Color.LimeGreen,
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                WordWrap = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                BorderStyle = BorderStyle.None
            };
            Controls.Add(outputBox);

            var inputPanel = new Panel
            {
                Location = new Point(10, 530),
                Size = new Size(860, 35),
                BackColor = Color.FromArgb(20, 20, 20)
            };

            var promptLabel = new Label
            {
                Text = ">",
                ForeColor = Color.LimeGreen,
                Font = new Font("Consolas", 12, FontStyle.Bold),
                Location = new Point(5, 7),
                AutoSize = true
            };
            inputPanel.Controls.Add(promptLabel);

            inputBox = new TextBox
            {
                Location = new Point(20, 5),
                Size = new Size(830, 25),
                BackColor = Color.Black,
                ForeColor = Color.LimeGreen,
                Font = new Font("Consolas", 12),
                BorderStyle = BorderStyle.None
            };
            inputBox.KeyDown += InputBox_KeyDown;
            inputPanel.Controls.Add(inputBox);
            Controls.Add(inputPanel);

            var btnY = 580;

            startBtn = ConsoleButton("Запустить", new Point(10, btnY), Color.FromArgb(0, 80, 0));
            startBtn.Click += StartBtn_Click;
            Controls.Add(startBtn);

            restartBtn = ConsoleButton("Перезапустить", new Point(120, btnY), Color.FromArgb(80, 80, 0));
            restartBtn.Click += RestartBtn_Click;
            Controls.Add(restartBtn);

            clearBtn = ConsoleButton("Очистить", new Point(250, btnY), Color.FromArgb(80, 0, 0));
            clearBtn.Click += (s, e) => outputBox.Clear();
            Controls.Add(clearBtn);

            inputBox.Enabled = false;
        }

        private Button ConsoleButton(string text, Point location, Color backColor)
        {
            var btn = new Button
            {
                Text = text,
                Location = location,
                Size = new Size(110, 35),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                BackColor = backColor,
                ForeColor = Color.LimeGreen,
                Font = new Font("Consolas", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(backColor);
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;
            return btn;
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            if (process != null && !process.HasExited)
            {
                AppendOutput("Процесс уже запущен.");
                return;
            }

            try
            {
                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = System.Text.Encoding.GetEncoding(866),
                        StandardErrorEncoding = System.Text.Encoding.GetEncoding(866)
                    },
                    EnableRaisingEvents = true
                };

                process.OutputDataReceived += (s, args) =>
                {
                    if (args.Data != null)
                        AppendOutput(args.Data);
                };
                process.ErrorDataReceived += (s, args) =>
                {
                    if (args.Data != null)
                        AppendOutput($"[ERR] {args.Data}");
                };
                process.Exited += (s, args) =>
                {
                    AppendOutput("--- Процесс завершён ---");
                    this.Invoke((MethodInvoker)(() =>
                    {
                        inputBox.Enabled = false;
                        SetStatus("Остановлен");
                    }));
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                inputBox.Enabled = true;
                inputBox.Clear();
                inputBox.Focus();
                SetStatus("Запущен");
                AppendOutput("--- Консоль запущена ---");
            }
            catch (Exception ex)
            {
                AppendOutput($"[ОШИБКА] {ex.Message}");
            }
        }

        private void RestartBtn_Click(object sender, EventArgs e)
        {
            if (process != null && !process.HasExited)
            {
                try
                {
                    process.StandardInput.WriteLine("exit");
                    process.WaitForExit(3000);
                    if (!process.HasExited)
                        process.Kill();
                }
                catch { }
                process.Dispose();
                process = null;
            }

            StartBtn_Click(sender, e);
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ExecuteCommand(inputBox.Text);
            }
            else if (e.KeyCode == Keys.Up)
            {
                e.SuppressKeyPress = true;
                if (commandHistory.Count > 0 && historyIndex > 0)
                {
                    historyIndex--;
                    inputBox.Text = commandHistory[historyIndex];
                    inputBox.SelectionStart = inputBox.Text.Length;
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                e.SuppressKeyPress = true;
                if (commandHistory.Count > 0 && historyIndex < commandHistory.Count - 1)
                {
                    historyIndex++;
                    inputBox.Text = commandHistory[historyIndex];
                    inputBox.SelectionStart = inputBox.Text.Length;
                }
                else
                {
                    historyIndex = commandHistory.Count;
                    inputBox.Clear();
                }
            }
        }

        private void ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return;

            AppendOutput($"> {command}");

            commandHistory.Add(command);
            historyIndex = commandHistory.Count;
            inputBox.Clear();

            if (process == null || process.HasExited)
            {
                AppendOutput("[ОШИБКА] Консоль не запущена. Нажмите 'Запустить'.");
                return;
            }

            try
            {
                process.StandardInput.WriteLine(command);
            }
            catch (Exception ex)
            {
                AppendOutput($"[ОШИБКА] {ex.Message}");
            }
        }

        private void AppendOutput(string text)
        {
            if (outputBox.IsDisposed) return;

            if (outputBox.InvokeRequired)
            {
                outputBox.Invoke((MethodInvoker)(() => AppendOutput(text)));
                return;
            }

            outputBox.AppendText(text + Environment.NewLine);
            outputBox.SelectionStart = outputBox.Text.Length;
            outputBox.ScrollToCaret();
        }

        private void SetStatus(string status)
        {
            foreach (Control c in Controls)
            {
                if (c is Label lbl && lbl.Tag is string tag && tag == "statusLabel")
                {
                    lbl.Text = $"Статус: {status}";
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (process != null && !process.HasExited)
            {
                try
                {
                    process.StandardInput.WriteLine("exit");
                    process.WaitForExit(2000);
                    if (!process.HasExited)
                        process.Kill();
                }
                catch { }
                process.Dispose();
            }
            base.OnFormClosing(e);
        }
    }
}
