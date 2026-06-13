using System;
using System.Windows.Forms;
using ShopProject.WinForms;

namespace ShopProject
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ServiceLocator.Initialize();

            Application.ThreadException += (s, e) =>
            {
                MessageBox.Show($"Ошибка: {e.Exception.Message}", "Критическая ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                MessageBox.Show($"Необработанная ошибка: {ex?.Message}", "Критическая ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            if (args.Length > 0 && args[0] == "--console")
            {
                string autoLoginEmail = null;
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--auto-login" && i + 1 < args.Length)
                    {
                        autoLoginEmail = args[i + 1];
                        break;
                    }
                }

                ConsoleMode.Run(autoLoginEmail);
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                try
                {
                    Application.Run(new MainForm());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Критическая ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}