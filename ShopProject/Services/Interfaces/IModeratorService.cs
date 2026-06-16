using System;

namespace ShopProject.Services.Interfaces
{
    public interface IModeratorService
    {
        void ViewUserProfile(string email);
        void ChangeUserBalance(string email, decimal newBalance);
        void ToggleBlockUser(string email);
    }
}