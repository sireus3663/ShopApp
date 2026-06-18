using Microsoft.Win32;

namespace ShopProject.Services
{
    public static class TokenStorage
    {
        private const string REGISTRY_PATH = @"Software\ShopProject";
        private const string TOKEN_KEY = "SessionToken";

        public static void Save(string token)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(REGISTRY_PATH))
            {
                key.SetValue(TOKEN_KEY, token, RegistryValueKind.String);
            }
        }

        public static string Get()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(REGISTRY_PATH))
            {
                return key?.GetValue(TOKEN_KEY) as string;
            }
        }

        public static void Clear()
        {
            using (var key = Registry.CurrentUser.CreateSubKey(REGISTRY_PATH))
            {
                key.DeleteValue(TOKEN_KEY, false);
            }
        }
    }
}