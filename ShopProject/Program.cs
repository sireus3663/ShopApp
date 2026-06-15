using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShopProject.Db;
using ShopProject.Forms;
using ShopProject.Services;
using Microsoft.EntityFrameworkCore;

namespace ShopProject
{
    internal class Program
    {
        [STAThread]
        static async Task Main(string[] args)
        {
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
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                var context = new AppDbContext();
                await context.Database.MigrateAsync();

                var authService = new AuthService(context);
                var logger = new LoggerService();
                var userService = new UserService(context, authService, logger);

                var sessionRestored = await authService.RestoreSession();

                if (sessionRestored)
                {
                    Application.Run(new MainForm(authService, context));
                }
                else
                {
                    var loginForm = new LoginForm(authService, userService);
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        Application.Run(new MainForm(authService, context));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Критическая ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}