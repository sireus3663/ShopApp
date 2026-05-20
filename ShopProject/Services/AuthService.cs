using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopProject.Db;

namespace ShopProject.Service
{
    internal class AuthService : IAuthService
    {
        private User? _currentUser;
        //=> тоже самое что get{return _currentUser}
        public User? currentUser => _currentUser;
        private readonly UserRepository _userRepository;
        public AuthService(AppDbContext context)
        {
            _userRepository = new UserRepository(context);
        }

        public User Get_currentUser()
        {
            return _currentUser;
        }

        public User Login(string email, string password)
        {
            if (_userRepository.Exists(email))
            {
                User temp = _userRepository.GetByEmail(email);
                if(temp.Password == password)
                {
                    _currentUser = temp;
                    return temp;
                }
                else { Console.WriteLine("Пароль не подходит"); _currentUser = null; return null; }
            }
            else { Console.WriteLine("Нет пользователя с такой почтой"); _currentUser = null; return null; }
        }
        public void Logout() {
            _currentUser = null;
        }
        public User RequireUser()
        {
            if( _currentUser == null)
            {
                return null;
            }
            else
            {
                return _currentUser;
            }
        }
    }
}
