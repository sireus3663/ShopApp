using Microsoft.EntityFrameworkCore;
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
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _userRepository = new UserRepository(context);
            _context = context;
        }

        public User? Get_currentUser() => _currentUser;

        public async Task<User> Login(string email, string password)
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

            return user;
        }

        public void LoginById(User user)
        {
            _currentUser = user;
        }

        public async Task Logout()
        {
            var token = TokenStorage.Get();
            if (!string.IsNullOrEmpty(token))
            {
                var session = await _context.sessions.FirstOrDefaultAsync(s => s.Token == token);
                if (session != null)
                {
                    session.IsActive = false;
                    await _context.SaveChangesAsync();
                }
            }

            TokenStorage.Clear();
            _currentUser = null;
        }

        public async Task<bool> RestoreSession()
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
            return true;
        }

        public User RequireUser()
        {
            if (_currentUser == null)
                throw new InvalidOperationException("Пользователь не авторизован");
            return _currentUser;
        }

    }
}