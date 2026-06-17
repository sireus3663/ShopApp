using System;
using System.Windows.Forms;
using ShopProject.Forms;
using ShopProject.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection; 


namespace ShopProject
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {

            Application.ApplicationExit += (sender, e) =>
            {
                // Очистка ресурсов при выходе
                Environment.Exit(0);
            };

            var serviceProvider = DependencyInjection.ConfigureServices();

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

                var consoleMode = serviceProvider.GetRequiredService<ConsoleMode>();
                consoleMode.Run(autoLoginEmail);
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                try
                {
                    var mainForm = serviceProvider.GetRequiredService<MainForm>();
                    Application.Run(mainForm);
                }
                catch (Exception ex)
                {
                    var logger = serviceProvider.GetRequiredService<ILoggerService>();
                    logger.Error("Критическая ошибка при запуске", ex);

                    MessageBox.Show($"Ошибка: {ex.Message}", "Критическая ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}