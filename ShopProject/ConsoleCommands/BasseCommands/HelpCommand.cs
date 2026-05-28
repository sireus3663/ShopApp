using Microsoft.Win32;
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
            // Показываем справку
            _registry.ShowHelp();

            // Пауза, чтобы пользователь успел прочитать
            Console.WriteLine("\nНажмите любую клавишу для открытия меню...");
            Console.ReadKey();

            // Открываем меню
            var menuCommand = new MenuCommand(_authService, _registry);
            menuCommand.Execute(Array.Empty<string>());
        }
    }
}