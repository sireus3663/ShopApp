using ShopProject.Db;
using ShopProject.Forms;
using ShopProject.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShopProject
{
    internal class Program
    {
        [STAThread]
        static async Task Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--console")
            {
                ConsoleMode.Run(args);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var logger = new LoggerService();
            var configService = new AppConfigService();
            var startup = new StartupService(logger, configService);

            var context = startup.TryConnect();
            if (context == null)
            {
                using var form = new DbConnectionForm();
                if (form.ShowDialog() != DialogResult.OK) return;
                context = form.ConnectedContext;
            }

            startup.InitializeOnFirstLaunch(context);

            var authService = new AuthService(context);
            await authService.RestoreSession();

            Application.Run(new MainForm(authService, context));
        }
    }
}
