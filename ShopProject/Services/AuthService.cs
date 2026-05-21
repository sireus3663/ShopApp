using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopProject.Db;
using ShopProject.Models;

namespace ShopProject.Service
{
    public class AuthService
    {
        public UserRepository _userRepository;
        private User _currentUser;

        public AuthService()
        {
            var context = new AppDbContext();
            _userRepository = new UserRepository(context);
        }

        public bool Login(string email, string password)
        {
            var user = _userRepository.GetByLogin(email, password);
            if (user != null)
            {
                _currentUser = user;
                return true;
            }
            return false;
        }
        public void Logout()
        {
            _currentUser = null;
        }

        public User RequireUser()
        {
            if (_currentUser == null)
            {
                throw new Exception("Пользователь не авторизован");
            }
            return _currentUser;
        }

        public User RequireRole(Role requiredRole)
        {
            var user = RequireUser();
            if (user.Role != requiredRole && user.Role != Role.Admin)
            {
                throw new Exception($"Требуется роль '{requiredRole}'");
            }
            return user;
        }
    }
}