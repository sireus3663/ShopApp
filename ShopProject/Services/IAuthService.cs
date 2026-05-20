using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.Service
{
    public interface IAuthService
    {
        public User Login(string email, string password);
        public void Logout();
        public User RequireUser();
    }
}
