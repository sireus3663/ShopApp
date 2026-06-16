using System;
using System.Diagnostics;
using System.Windows.Forms;
using ShopProject.Models;

namespace ShopProject.Services
{
    public static class ConsoleHelper
    {
        public static void OpenAdminConsole(User currentUser)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    UseShellExecute = true,
                    Arguments = $"--console --auto-login {currentUser.Email}"
                };

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска консоли: {ex.Message}");
            }
        }
    }
}