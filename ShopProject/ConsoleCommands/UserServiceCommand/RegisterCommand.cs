using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.UserServiceCommand
{
    public class RegisterCommand : BaseCommand
    {
        private readonly UserService _userService;
        public override string Name => "register";
        public override string Description => "Регистрация. Использование: register <имя> <email> <пароль>";
        public override bool AvailableForGuest => true;

        public RegisterCommand(UserService userService) { _userService = userService; }
        public override void Execute(string[] args)
        {
            if (args.Length < 3) { Error("Укажите email и пароль"); return; };
            try
            {
                var user = _userService.Register(args[0], args[1], args[2]);
                Success($"Пользователь {user.Name} зарегистрирован!");
            }
            catch (Exception ex) { Error(ex.Message); }
            
        }
    }
}
