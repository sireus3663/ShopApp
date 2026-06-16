using ShopProject.Models;
using System;

namespace ShopProject.Services.Interfaces
{
    public interface IUserService
    {
        User Register(string name, string email, string password);
        void ChangeRole(Guid userId, Role newRole);
        void ShowProfile();
    }
}