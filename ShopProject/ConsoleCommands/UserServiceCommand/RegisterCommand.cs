using ShopProject.ConsoleCommands.BasseCommands;
using ShopProject.Services.Interfaces;
using System;

namespace ShopProject.ConsoleCommands.UserServiceCommand
{
    public class RegisterCommand : BaseCommand
    {
        private readonly IUserService _userService;
        public override string Name => "register";
        public override string Description => "Регистрация. Использование: register <имя> <email> <пароль>";
        public override bool AvailableForGuest => true;

        public RegisterCommand(IUserService userService) { _userService = userService; }
        public override void Execute(string[] args)
        {
            if (args.Length < 3)
            {
                Error("Укажите имя, email и пароль");
                return;
            }

            if (string.IsNullOrWhiteSpace(args[0]))
            {
                Error("Имя не может быть пустым");
                return;
            }

            if (string.IsNullOrWhiteSpace(args[1]))
            {
                Error("Email не может быть пустым");
                return;
            }

            if (string.IsNullOrWhiteSpace(args[2]))
            {
                Error("Пароль не может быть пустым");
                return;
            }

            try
            {
                var user = _userService.Register(args[0].Trim(), args[1].Trim(), args[2]);
                Success($"Пользователь {user.Name} зарегистрирован!");
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }
    }
}