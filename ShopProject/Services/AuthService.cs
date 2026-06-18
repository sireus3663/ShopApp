using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopProject.Db;
using ShopProject.Models;

namespace ShopProject.Services
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
        Task<bool> RestoreSessionAsync();
    }
}

namespace ShopProject.Services
{
    public class AuthService : IAuthService
    {
        private User? _currentUser;
        public User? CurrentUser => _currentUser;

        private readonly IUserRepository _userRepository;
        private readonly IAppConfigService? _configService;
        private readonly AppDbContext _context;

        public AuthService(IUserRepository userRepository, AppDbContext context, IAppConfigService? configService = null)
        {
            _userRepository = userRepository;
            _context = context;
            _configService = configService;
        }

        public void Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new Exception("поля не заполнены");

            if (!_userRepository.Exists(email))
                throw new Exception("пользователя с таким email не существует");

            User? user = _userRepository.GetByEmail(email);

            if (user == null)
                throw new Exception("пользователь не найден");

            if (!user.VerifyPassword(password))
                throw new Exception("пароль неверный");

            if (user.IsBlocked)
                throw new Exception("Ваш аккаунт заблокирован. Обратитесь к администратору.");

            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsActive = true
            };

            _context.sessions.Add(session);
            _context.SaveChanges();

            TokenStorage.Save(token);
            _currentUser = user;
            _configService?.SetCurrentUserId(user.Id);
        }

        public void LoginById(User user)
        {
            _currentUser = user;
        }

        public void Logout()
        {
            var token = TokenStorage.Get();
            if (!string.IsNullOrEmpty(token))
            {
                var session = _context.sessions.FirstOrDefault(s => s.Token == token);
                if (session != null)
                {
                    session.IsActive = false;
                    _context.SaveChanges();
                }
            }

            TokenStorage.Clear();
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

            User? user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
                throw new Exception("пользователь не найден");

            if (!user.VerifyPassword(password))
                throw new Exception("пароль неверный");

            if (user.IsBlocked)
                throw new Exception("Ваш аккаунт заблокирован. Обратитесь к администратору.");

            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsActive = true
            };

            _context.sessions.Add(session);
            await _context.SaveChangesAsync();

            TokenStorage.Save(token);
            _currentUser = user;
            _configService?.SetCurrentUserId(user.Id);
        }

        public async Task<User> RequireUserAsync()
        {
            if (_currentUser == null)
                throw new InvalidOperationException("Пользователь не авторизован");
            return await Task.FromResult(_currentUser);
        }

        public async Task<bool> RestoreSessionAsync()
        {
            var token = TokenStorage.Get();
            if (string.IsNullOrEmpty(token)) return false;

            var session = await _context.sessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Token == token && s.IsActive && s.ExpiresAt > DateTime.UtcNow);

            if (session == null)
            {
                TokenStorage.Clear();
                return false;
            }

            _currentUser = session.User;
            _configService?.SetCurrentUserId(session.User.Id);
            return true;
        }
    }
}