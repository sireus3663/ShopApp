using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.BasseCommands
{
    public class HelpCommand : BaseCommand
    {
        private readonly AuthService _authService;

        public override string Name => "help";
        public override string Description => "Показать справку";

        public HelpCommand(AuthService authService)
        {
            _authService = authService;
        }

        public override void Execute(string[] args)
        {
            var menuCommand = new MenuCommand(_authService);
            menuCommand.Execute(args);
        }
    }
}
