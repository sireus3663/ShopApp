using ShopProject.Db;
using ShopProject.Models;
using System;
using System.Threading.Tasks;

namespace ShopProject.Services
{
    public class AuthService
    {
        private User? _currentUser;
        public User? currentUser => _currentUser;

        private readonly UserRepository _userRepository;
        private readonly AppConfigService? _configService;

        public AuthService(AppDbContext context, AppConfigService? configService = null)
        {
            _userRepository = new UserRepository(context);
            _configService = configService;
        }

        public User? Get_currentUser() => _currentUser;

        public void Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new Exception("поля не заполнены");

            if (!_userRepository.Exists(email))
                throw new Exception("пользователя с таким email не существует");

            User user = _userRepository.GetByEmail(email);

            if (!user.VerifyPassword(password))
                throw new Exception("пароль неверный");

            if (user.IsBlocked)
                throw new Exception("Ваш аккаунт заблокирован. Обратитесь к администратору.");

            _currentUser = user;
            _configService?.SetCurrentUserId(user.Id);
        }

        public void LoginById(User user)
        {
            _currentUser = user;
        }

        public void Logout()
        {
            _currentUser = null;
            _configService?.SetCurrentUserId(null);
        }

        public User RequireUser()
        {
            if (_currentUser == null)
                throw new InvalidOperationException("Пользователь не авторизован");
            return _currentUser;
        }

        public async Task LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new Exception("поля не заполнены");

            if (!await _userRepository.ExistsAsync(email))
                throw new Exception("пользователя с таким email не существует");

            User user = await _userRepository.GetByEmailAsync(email);

            if (!user.VerifyPassword(password))
                throw new Exception("пароль неверный");

            if (user.IsBlocked)
                throw new Exception("Ваш аккаунт заблокирован. Обратитесь к администратору.");

            _currentUser = user;
            _configService?.SetCurrentUserId(user.Id);
        }

        public async Task<User> RequireUserAsync()
        {
            if (_currentUser == null)
                throw new InvalidOperationException("Пользователь не авторизован");
            return await Task.FromResult(_currentUser);
        }
    }
}