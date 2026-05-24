using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands
{
    public class RegisterCommand : BaseCommand
    {
        private readonly UserService _users;

        public override string Name =>
            "register";

        public override string Description =>
            "Регистрация (name, email, password)";

        public RegisterCommand(UserService users)
        {
            _users = users;
        }

        public override void Execute(string[] args)
        {
            var name = args[0];

            var email = args[1];

            var password = args[2];

            var user = _users.Register(
                    name,
                    email,
                    password
                );

            Success(
                $"Создан {user.Name}"
            );
        }
    }
}
