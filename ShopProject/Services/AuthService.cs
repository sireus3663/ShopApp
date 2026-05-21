using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopProject.Db;

namespace ShopProject.Services
{
    public class AuthService
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

        public void Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                throw new Exception("поля не заполнены");
            }

            if (!_userRepository.Exists(email))
            {
                throw new Exception("пользователя с таким email не существует");
            }

            User temp = _userRepository.GetByEmail(email);

            if (temp.Password != password)
            {
                throw new Exception("пароль неверный");
            }
            _currentUser = temp;
        }
        public void Logout()
        {
            _currentUser = null;
        }
        public User RequireUser()
        {
            if (_currentUser == null) throw new InvalidOperationException("Пользователь не авторизован");

            return _currentUser;
        }
    }
}
