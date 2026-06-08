using System;
using System.Windows.Forms;
using ShopProject.Db;
using ShopProject.Services;

namespace ShopProject.Forms
{
    public static class FormsApp
    {
        public static void Run(AuthService authService, UserService userService, AppDbContext context)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var loginForm = new LoginForm(authService, userService);
            if (loginForm.ShowDialog() != DialogResult.OK)
                return;
            Application.Run(new MainForm(authService, userService, context));
        }
    }
}
