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
        private Label statusLabel;
        private Button restartBtn;
        private Button clearBtn;
        private Process? _consoleProcess;
        private bool _isRunning = false;
        private bool _started = false;

        public AdminConsoleForm(User currentUser)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            InitializeComponent();
        }

        public void Start()
        {
            if (!_started) StartConsole();
        }

        public void Stop()
        {
            if (_consoleProcess != null && !_consoleProcess.HasExited)
            {
                _consoleProcess.Kill();
                _consoleProcess.Dispose();
                _consoleProcess = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) Stop();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Text = "Консоль администратора";
            this.Size = new Size(1000, 650);
            this.MinimumSize = new Size(700, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.FormClosing += AdminConsoleForm_FormClosing;

            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = Color.FromArgb(40, 40, 45),
                Padding = new Padding(12, 0, 12, 0)
            };

            var titleLabel = new Label
            {
                Text = "🖥 Консоль администратора",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(12, 12),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            statusLabel = new Label
            {
                Text = "Статус: Запуск...",
                ForeColor = Color.FromArgb(100, 200, 100),
                Font = new Font("Segoe UI", 9),
                Location = new Point(300, 15),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            restartBtn = new Button
            {
                Text = "⟳",
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 60, 65),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(32, 28),
                Location = new Point(headerPanel.Width - 82, 10),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleCenter
            };
            restartBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 80, 85);
            restartBtn.FlatAppearance.MouseDownBackColor = Color.FromArgb(100, 100, 105);
            restartBtn.Click += (s, e) => RestartConsole();

            clearBtn = new Button
            {
                Text = "✕",
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 60, 65),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(32, 28),
                Location = new Point(headerPanel.Width - 42, 10),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleCenter
            };
            clearBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 80, 85);
            clearBtn.FlatAppearance.MouseDownBackColor = Color.FromArgb(100, 100, 105);
            clearBtn.Click += (s, e) => consoleOutput?.Clear();

            var tooltipRestart = new ToolTip();
            tooltipRestart.SetToolTip(restartBtn, "Перезапустить консоль");
            var tooltipClear = new ToolTip();
            tooltipClear.SetToolTip(clearBtn, "Очистить вывод");

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(statusLabel);
            headerPanel.Controls.Add(restartBtn);
            headerPanel.Controls.Add(clearBtn);

            consoleOutput = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(18, 18, 20),
                ForeColor = Color.FromArgb(0, 255, 0),
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                WordWrap = false,
                BorderStyle = BorderStyle.None
            };

            inputBox = new TextBox
            {
                Dock = DockStyle.Bottom,
                Height = 32,
                BackColor = Color.FromArgb(25, 25, 28),
                ForeColor = Color.White,
                Font = new Font("Consolas", 10),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0)
            };
            inputBox.KeyDown += InputBox_KeyDown;

            var inputPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 34,
                BackColor = Color.FromArgb(30, 30, 33),
                Padding = new Padding(2, 1, 2, 1)
            };
            inputPanel.Controls.Add(inputBox);

            this.Controls.Add(consoleOutput);
            this.Controls.Add(inputPanel);
            this.Controls.Add(headerPanel);
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

        private void AppendColoredOutput(string? text, Color? forcedColor = null)
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

        private void InputBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && _isRunning && _consoleProcess != null)
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

            if (statusLabel != null)
            {
                statusLabel.Text = $"Статус: {status}";
                statusLabel.ForeColor = color;
            }
        }

        private void AdminConsoleForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_consoleProcess != null && !_consoleProcess.HasExited)
            {
                _consoleProcess.Kill();
                _consoleProcess.Dispose();
            }
        }
    }
}
