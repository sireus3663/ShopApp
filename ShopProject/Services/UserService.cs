using ShopProject.Db;
using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ShopProject.Services;

namespace ShopProject.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;
        private readonly AuthService _authService;

        public UserService(AppDbContext context, AuthService authService)
        {
            _userRepository = new UserRepository(context);
            _authService = authService;
        }

        public User Register(string name, string email, string password)
        {
            if (_userRepository.Exists(email))
                throw new Exception("Пользователь с таким email уже существует");

            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                throw new Exception("Пароль должен содержать минимум 8 символов");

            if (!Regex.IsMatch(password, @"\d"))
                throw new Exception("Пароль должен содержать хотя бы одну цифру");

            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Имя не может быть пустым");

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = email,
                Password = password,
                Balance = 0,
                Role = Role.Buyer
            };

            _userRepository.Add(newUser);
            return newUser;
        }

        public void ChangeRole(Guid userId, Role newRole)
        {
            if (!PermissionService.CanAdministrate(_authService.RequireUser().Role))
            {
                throw new Exception("недостаточно прав");
            }
            var user = _userRepository.GetById(userId);
            if (user == null)
                throw new Exception("Пользователь не найден");

            user.Role = newRole;
            _userRepository.Update(user);
        }
    }
}
