using Microsoft.Win32;
using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.BaseCommands
{
    public class HelpCommand : BaseCommand
    {
        private readonly CommandRegistry _registry;
        private readonly AuthService _authService;

        public override string Name => "help";
        public override string Description => "Показать справку и открыть меню";
        public override bool AvailableForGuest => true;

        public HelpCommand(CommandRegistry registry, AuthService authService)
        {
            _registry = registry;
            _authService = authService;
        }

        public override void Execute(string[] args)
        {
            _registry.ShowHelp();

            Console.WriteLine("\nНажмите любую клавишу для открытия меню...");
            Console.ReadKey();

            var menuCommand = new MenuCommand(_authService, _registry);
            menuCommand.Execute(Array.Empty<string>());
        }
    }
}
