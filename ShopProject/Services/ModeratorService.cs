using System;
using ShopProject.Models;

using ShopProject.Db;
namespace ShopProject.Services
{
    public interface IModeratorService
    {
        void ViewUserProfile(string email);
        void ChangeUserBalance(string email, decimal newBalance);
        void ToggleBlockUser(string email);
    }
}

namespace ShopProject.Services
{
    public class ModeratorService : IModeratorService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;

        public ModeratorService(IUserRepository userRepository, IAuthService authService)
        {
            _userRepository = userRepository;
            _authService = authService;
        }

        public void ViewUserProfile(string email)
        {
            var currentUser = _authService.RequireUser();

            if (!PermissionService.CanModerate(currentUser.Role))
            {
                throw new Exception("РўРѕР»СЊРєРѕ РјРѕРґРµСЂР°С‚РѕСЂС‹ Рё Р°РґРјРёРЅРёСЃС‚СЂР°С‚РѕСЂС‹ РјРѕРіСѓС‚ РїСЂРѕСЃРјР°С‚СЂРёРІР°С‚СЊ РїСЂРѕС„РёР»Рё РґСЂСѓРіРёС… РїРѕР»СЊР·РѕРІР°С‚РµР»РµР№");
            }
            var user = _userRepository.GetByEmail(email);
            if (user == null)
            {
                throw new Exception($"РџРѕР»СЊР·РѕРІР°С‚РµР»СЊ СЃ email '{email}' РЅРµ РЅР°Р№РґРµРЅ");
            }
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"{"РџСЂРѕС„РёР»СЊ РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ",-40}");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"{"ID:",-15} {user.Id}");
            Console.WriteLine($"{"РРјСЏ:",-15} {user.Name}");
            Console.WriteLine($"{"Email:",-15} {user.Email}");
            Console.WriteLine($"{"Р РѕР»СЊ:",-15} {user.Role}");
            Console.WriteLine($"{"Р‘Р°Р»Р°РЅСЃ:",-15} {user.Balance} СЂСѓР±.");
            Console.WriteLine($"{"РЎС‚Р°С‚СѓСЃ:",-15} {(user.IsBlocked ? "Р—Р°Р±Р»РѕРєРёСЂРѕРІР°РЅ" : "РђРєС‚РёРІРµРЅ")}");
            Console.WriteLine(new string('=', 50));
        }

        public void ChangeUserBalance(string email, decimal newBalance)
        {
            var currentUser = _authService.RequireUser();
            if (!PermissionService.CanModerate(currentUser.Role))
            {
                throw new Exception("РўРѕР»СЊРєРѕ РјРѕРґРµСЂР°С‚РѕСЂС‹ Рё Р°РґРјРёРЅРёСЃС‚СЂР°С‚РѕСЂС‹ РјРѕРіСѓС‚ РёР·РјРµРЅСЏС‚СЊ Р±Р°Р»Р°РЅСЃ РїРѕР»СЊР·РѕРІР°С‚РµР»РµР№");
            }
            if (newBalance < 0)
            {
                throw new Exception("Р‘Р°Р»Р°РЅСЃ РЅРµ РјРѕР¶РµС‚ Р±С‹С‚СЊ РѕС‚СЂРёС†Р°С‚РµР»СЊРЅС‹Рј");
            }
            var user = _userRepository.GetByEmail(email);
            if (user == null)
            {
                throw new Exception($"РџРѕР»СЊР·РѕРІР°С‚РµР»СЊ СЃ email '{email}' РЅРµ РЅР°Р№РґРµРЅ");
            }
            var oldBalance = user.Balance;
            user.Balance = newBalance;
            _userRepository.Update(user);
            Console.WriteLine($"Р‘Р°Р»Р°РЅСЃ РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ {user.Name} РёР·РјРµРЅС‘РЅ: {oldBalance} СЂСѓР±. в†’ {newBalance} СЂСѓР±.");
        }

        public void ToggleBlockUser(string email)
        {
            var currentUser = _authService.RequireUser();
            if (!PermissionService.CanModerate(currentUser.Role))
            {
                throw new Exception("РўРѕР»СЊРєРѕ РјРѕРґРµСЂР°С‚РѕСЂС‹ Рё Р°РґРјРёРЅРёСЃС‚СЂР°С‚РѕСЂС‹ РјРѕРіСѓС‚ Р±Р»РѕРєРёСЂРѕРІР°С‚СЊ РїРѕР»СЊР·РѕРІР°С‚РµР»РµР№");
            }
            var user = _userRepository.GetByEmail(email);
            if (user == null)
            {
                throw new Exception($"РџРѕР»СЊР·РѕРІР°С‚РµР»СЊ СЃ email '{email}' РЅРµ РЅР°Р№РґРµРЅ");
            }
            if (user.Id == currentUser.Id)
            {
                throw new Exception("РќРµР»СЊР·СЏ Р·Р°Р±Р»РѕРєРёСЂРѕРІР°С‚СЊ СЃР°РјРѕРіРѕ СЃРµР±СЏ");
            }
            user.IsBlocked = !user.IsBlocked;
            _userRepository.Update(user);

            var status = user.IsBlocked ? "Р·Р°Р±Р»РѕРєРёСЂРѕРІР°РЅ" : "СЂР°Р·Р±Р»РѕРєРёСЂРѕРІР°РЅ";
            Console.WriteLine($"РџРѕР»СЊР·РѕРІР°С‚РµР»СЊ {user.Name} {status}");
        }
    }
}