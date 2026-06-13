using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ShopProject.Models;

namespace ShopProject.Forms
{
    public class AdminConsoleForm : Form
    {
        private readonly User _currentUser;
        private RichTextBox consoleOutput;
        private TextBox inputBox;
        private Process _consoleProcess;
        private bool _isRunning = false;

        public AdminConsoleForm(User currentUser)  
        {
            _currentUser = currentUser;
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
                string workingDirectory;
                string fileName;
                string arguments;

                if (Debugger.IsAttached)
                {
                    workingDirectory = Path.GetFullPath(Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        @"..\..\..\"
                    ));
                    fileName = "dotnet";
                    arguments = $"run -- --console --auto-login \"{_currentUser.Email}\"";
                }
                else
                {
                    workingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    fileName = Path.Combine(workingDirectory, "ShopProject.exe");
                    arguments = $"--console --auto-login \"{_currentUser.Email}\"";

                    if (!File.Exists(fileName))
                    {
                        fileName = "dotnet";
                        arguments = $"run -- --console --auto-login \"{_currentUser.Email}\"";
                    }
                }

                _consoleProcess = new Process();
                _consoleProcess.StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                };

                _consoleProcess.OutputDataReceived += (s, e) => AppendColoredOutput(e.Data);
                _consoleProcess.ErrorDataReceived += (s, e) => AppendColoredOutput($"[ERROR] {e.Data}", Color.Red);

                _consoleProcess.Start();
                _consoleProcess.BeginOutputReadLine();
                _consoleProcess.BeginErrorReadLine();

                _isRunning = true;
                UpdateStatus("Активен", Color.FromArgb(100, 200, 100));
                AppendColoredOutput($"[СИСТЕМА] Консоль запущена. Пользователь: {_currentUser.Name} ({_currentUser.Role})", Color.Cyan);
                AppendColoredOutput("[СИСТЕМА] Введите 'help' для списка команд\n", Color.Cyan);
            }
            catch (Exception ex)
            {
                AppendColoredOutput($"[ОШИБКА] Не удалось запустить консоль: {ex.Message}", Color.Red);
                UpdateStatus("Ошибка", Color.FromArgb(200, 100, 100));
            }
        }

        private void AppendColoredOutput(string text, Color? forcedColor = null)
        {
            if (consoleOutput.InvokeRequired)
            {
                consoleOutput.Invoke(new Action(() => AppendColoredOutput(text, forcedColor)));
                return;
            }

            if (string.IsNullOrEmpty(text)) return;

            Color textColor = forcedColor ?? GetColorByText(text);

            var originalColor = consoleOutput.SelectionColor;

            consoleOutput.SelectionColor = textColor;
            consoleOutput.AppendText(text + Environment.NewLine);

            consoleOutput.SelectionColor = originalColor;

            consoleOutput.ScrollToCaret();
        }

        private Color GetColorByText(string text)
        {
            if (string.IsNullOrEmpty(text)) return Color.Gray;

            if (text.Contains("[ERROR]") ||
                text.Contains("[ОШИБКА]") ||
                text.Contains("Exception:") ||
                text.Contains("не найден") ||
                text.Contains("не удалось") ||
                text.Contains("недостаточно") ||
                text.Contains("неверный") ||
                text.StartsWith("ERROR:"))
            {
                return Color.Red;
            }

            if (text.Contains("[OK]") ||
                text.Contains("[SUCCESS]") ||
                text.Contains("успешно") ||
                text.Contains("одобрен") ||
                text.Contains("создан") ||
                text.Contains("добавлен") ||
                text.Contains("удалён") ||
                text.Contains("Добро пожаловать") ||
                text.Contains("изменён"))
            {
                return Color.LightGreen;
            }

            if (text.Contains("[WARNING]") ||
                text.Contains("[WARN]") ||
                text.Contains("ВНИМАНИЕ"))
            {
                return Color.Orange;
            }

            if (text.Contains("[i]") ||
                text.Contains("[INFO]") ||
                text.Contains("[СИСТЕМА]") ||
                text.Contains("Всего") ||
                text.Contains("Товары") ||
                text.Contains("Корзина") ||
                text.Contains("Избранное"))
            {
                return Color.Cyan;
            }

            if (text.StartsWith("> "))
            {
                return Color.White;
            }

            if (text.Contains("===") || text.Contains("Доступные команды"))
            {
                return Color.Yellow;
            }

            return Color.LightGray;
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && _isRunning)
            {
                var command = inputBox.Text.Trim();
                if (!string.IsNullOrEmpty(command))
                {
                    AppendColoredOutput($"> {command}", Color.White);
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

            AppendColoredOutput("\n[СИСТЕМА] Перезапуск консоли...\n", Color.Yellow);
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
