using System;

namespace ShopProject.Services.Interfaces
{
    public interface IAppConfigService
    {
        string GetConnectionString();
        void UpdateDbPassword(string newPassword);
        void UpdateConnectionString(string host, string port, string database, string username, string password);
        Guid? GetCurrentUserId();
        void SetCurrentUserId(Guid? userId);
    }
}