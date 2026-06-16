using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopProject.Models;

namespace ShopProject.Services.Interfaces
{
    public interface IAuthService
    {
        User? CurrentUser { get; }

        void Login(string email, string password);
        void LoginById(User user);
        void Logout();
        User RequireUser();

        Task LoginAsync(string email, string password);
        Task<User> RequireUserAsync();
    }
}