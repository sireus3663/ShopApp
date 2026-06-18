using ShopProject.Models;
using System;
using ShopProject.Db;
using System.Text.RegularExpressions;

namespace ShopProject.Services
{
    public interface IUserService
    {
        User Register(string name, string email, string password);
        void ChangeRole(Guid userId, Role newRole);
        void ShowProfile();
    }
}

namespace ShopProject.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly ILoggerService _logger;

        public UserService(IUserRepository userRepository, IAuthService authService, ILoggerService logger)
        {
            _userRepository = userRepository;
            _authService = authService;
            _logger = logger;
        }

        public User Register(string name, string email, string password)
        {
            if (_userRepository.Exists(email))
                throw new Exception("РџРѕР»СЊР·РѕРІР°С‚РµР»СЊ СЃ С‚Р°РєРёРј email СѓР¶Рµ СЃСѓС‰РµСЃС‚РІСѓРµС‚");

            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                throw new Exception("РџР°СЂРѕР»СЊ РґРѕР»Р¶РµРЅ СЃРѕРґРµСЂР¶Р°С‚СЊ РјРёРЅРёРјСѓРј 8 СЃРёРјРІРѕР»РѕРІ");

            if (!Regex.IsMatch(password, @"\d"))
                throw new Exception("РџР°СЂРѕР»СЊ РґРѕР»Р¶РµРЅ СЃРѕРґРµСЂР¶Р°С‚СЊ С…РѕС‚СЏ Р±С‹ РѕРґРЅСѓ С†РёС„СЂСѓ");

            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("РРјСЏ РЅРµ РјРѕР¶РµС‚ Р±С‹С‚СЊ РїСѓСЃС‚С‹Рј");

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = email,
                Balance = 0,
                Role = Role.Buyer,
                IsBlocked = false
            };
            newUser.SetPassword(password);

            _userRepository.Add(newUser);
            _logger.Info($"User Created [{newUser.Email}]");
            return newUser;
        }

        public void ChangeRole(Guid userId, Role newRole)
        {
            if (!PermissionService.CanAdministrate(_authService.RequireUser().Role))
            {
                throw new Exception("РЅРµРґРѕСЃС‚Р°С‚РѕС‡РЅРѕ РїСЂР°РІ");
            }
            var user = _userRepository.GetById(userId);
            if (user == null)
                throw new Exception("РџРѕР»СЊР·РѕРІР°С‚РµР»СЊ РЅРµ РЅР°Р№РґРµРЅ");

            user.Role = newRole;
            _userRepository.Update(user);
        }

        public void ShowProfile()
        {
            var currentUser = _authService.RequireUser();

            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"{"РџСЂРѕС„РёР»СЊ РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ",-40}");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"{"ID:",-15} {currentUser.Id}");
            Console.WriteLine($"{"РРјСЏ:",-15} {currentUser.Name}");
            Console.WriteLine($"{"Email:",-15} {currentUser.Email}");
            Console.WriteLine($"{"Р РѕР»СЊ:",-15} {currentUser.Role}");
            Console.WriteLine($"{"Р‘Р°Р»Р°РЅСЃ:",-15} {currentUser.Balance} СЂСѓР±.");
            Console.WriteLine(new string('=', 50));
        }
    }
}