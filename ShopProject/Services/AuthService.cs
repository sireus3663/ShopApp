using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopProject.Db;

namespace ShopProject.Service
{
    internal class AuthService
    {
        private User? _currentUser;
        //=> тоже самое что get{return _currentUser}
        public User? currentUser => _currentUser;
        private readonly UserRepository _userRepository;
        public AuthService(AppDbContext context)
        {
            _userRepository = new UserRepository(context);
        }

        public User? Get_currentUser()
        {
            return _currentUser;
        }

        public User? Login(string email, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    return null;
                }

                if (!_userRepository.Exists(email))
                {
                    return null;
                }

                User temp = _userRepository.GetByEmail(email);

                if (temp.Password != password)
                {
                    return null;
                }
                _currentUser = temp;
                return temp;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public void Logout()
        {
            _currentUser = null;
        }
        public User RequireUser()
        {
            if (_currentUser == null)
                throw new InvalidOperationException("Пользователь не авторизован");

            return _currentUser;
        }
    }
}
