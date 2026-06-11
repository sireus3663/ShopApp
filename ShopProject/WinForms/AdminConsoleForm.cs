using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ShopProject.WinForms
{
    public class AdminConsoleForm : Form
    {
        private RichTextBox consoleOutput;
        private TextBox inputBox;
        private Process _consoleProcess;
        private bool _isRunning = false;

        public AdminConsoleForm()
        {
            InitializeComponent();
            StartConsole();
        }

        private void InitializeComponent()
        {
            this.Text = "Консоль администратора";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.FormClosing += AdminConsoleForm_FormClosing;

            var toolStrip = new ToolStrip
            {
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White
            };

            var restartBtn = new ToolStripButton
            {
                Text = "Перезапустить",
                ForeColor = Color.White,
                BackColor = Color.FromArgb(80, 80, 85)
            };
            restartBtn.Click += (s, e) => RestartConsole();

            var clearBtn = new ToolStripButton
            {
                Text = "Очистить",
                ForeColor = Color.White,
                BackColor = Color.FromArgb(80, 80, 85)
            };
            clearBtn.Click += (s, e) => consoleOutput.Clear();

            var statusLabel = new ToolStripLabel
            {
                Text = "Статус: Запуск...",
                ForeColor = Color.FromArgb(100, 200, 100)
            };

            toolStrip.Items.Add(restartBtn);
            toolStrip.Items.Add(clearBtn);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(statusLabel);

            consoleOutput = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                ForeColor = Color.FromArgb(0, 255, 0),
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                WordWrap = false
            };

            inputBox = new TextBox
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.White,
                Font = new Font("Consolas", 10)
            };
            inputBox.KeyDown += InputBox_KeyDown;

            this.Controls.Add(consoleOutput);
            this.Controls.Add(inputBox);
            this.Controls.Add(toolStrip);
        }

        private void StartConsole()
        {
            try
            {
                var projectPath = Path.GetFullPath(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    @"..\..\..\"
                ));

                _consoleProcess = new Process();
                _consoleProcess.StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "run -- --console",
                    WorkingDirectory = projectPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                };

                _consoleProcess.OutputDataReceived += (s, e) => AppendOutput(e.Data);
                _consoleProcess.ErrorDataReceived += (s, e) => AppendOutput($"[ERROR] {e.Data}");

                _consoleProcess.Start();
                _consoleProcess.BeginOutputReadLine();
                _consoleProcess.BeginErrorReadLine();

                _isRunning = true;
                UpdateStatus("Активен", Color.FromArgb(100, 200, 100));
            }
            catch (Exception ex)
            {
                AppendOutput($"[ОШИБКА] Не удалось запустить консоль: {ex.Message}");
                UpdateStatus("Ошибка", Color.FromArgb(200, 100, 100));
            }
        }

        private void AppendOutput(string text)
        {
            if (consoleOutput.InvokeRequired)
            {
                consoleOutput.Invoke(new Action(() => AppendOutput(text)));
                return;
            }

            if (!string.IsNullOrEmpty(text))
            {
                consoleOutput.AppendText(text + Environment.NewLine);
                consoleOutput.ScrollToCaret();
            }
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && _isRunning)
            {
                var command = inputBox.Text.Trim();
                if (!string.IsNullOrEmpty(command))
                {
                    AppendOutput($"> {command}");
                    _consoleProcess.StandardInput.WriteLine(command);
                    inputBox.Clear();
                }
            }
        }

        private void RestartConsole()
        {
            if (_consoleProcess != null && !_consoleProcess.HasExited)
            {
                _consoleProcess.Kill();
                _consoleProcess.Dispose();
            }

            AppendOutput("\n[СИСТЕМА] Перезапуск консоли...\n");
            StartConsole();
        }

        private void UpdateStatus(string status, Color color)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateStatus(status, color)));
                return;
            }

            var toolStrip = this.Controls[2] as ToolStrip;
            if (toolStrip != null && toolStrip.Items[3] is ToolStripLabel label)
            {
                label.Text = $"Статус: {status}";
                label.ForeColor = color;
            }
        }

        private void AdminConsoleForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_consoleProcess != null && !_consoleProcess.HasExited)
            {
                _consoleProcess.Kill();
                _consoleProcess.Dispose();
            }
        }
    }
}