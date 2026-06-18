using ShopProject.Services;
using ShopProject.Models;
using System;
using System.Linq;

namespace ShopProject.ConsoleCommands.BaseCommands
{
    public class MenuCommand : BaseCommand
    {
        private readonly AuthService _authService;
        private readonly CommandRegistry _registry;

        public override string Name => "menu";
        public override string Description => "Показать список доступных команд";
        public override bool AvailableForGuest => true; 

        public MenuCommand(AuthService authService, CommandRegistry registry)
        {
            _authService = authService;
            _registry = registry;
        }

        public override void Execute(string[] args)
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(new string('=', 60));
            Console.WriteLine("          ДОСТУПНЫЕ КОМАНДЫ");
            Console.WriteLine(new string('=', 60));
            Console.ResetColor();

            if (_authService.currentUser != null)
            {
                Console.WriteLine($"Пользователь: {_authService.currentUser.Name}");
                Console.WriteLine($"Роль: {_authService.currentUser.Role}");
                Console.WriteLine($"Баланс: {_authService.currentUser.Balance:F2} руб.");

                if (_authService.currentUser.IsBlocked)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ВНИМАНИЕ: Ваш аккаунт заблокирован!");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.WriteLine("Статус: Не авторизован (ГОСТЬ)");
            }

            Console.WriteLine(new string('-', 60));
            Console.WriteLine();

            var availableCommands = _registry.GetAllCommands()
                .Where(cmd => IsCommandAvailable(cmd))
                .OrderBy(cmd => cmd.Name)
                .ToList();

            if (availableCommands.Count == 0)
            {
                Info("Нет доступных команд");
                return;
            }

            Console.WriteLine("Доступные команды:");
            Console.WriteLine(new string('-', 60));

            foreach (var cmd in availableCommands)
            {
                Console.WriteLine($"  {cmd.Name,-15} - {cmd.Description}");
            }

            Console.WriteLine(new string('-', 60));
            Console.WriteLine("\nСправка: введите 'help' для детальной справки");
            Console.WriteLine("        введите 'menu' для показа этого меню");
        }

        private bool IsCommandAvailable(ICommand command)
        {
            var baseCmd = command as BaseCommand;
            if (baseCmd == null) return false;

            if (_authService.currentUser == null)
            {
                return baseCmd.AvailableForGuest;
            }

            if (_authService.currentUser.IsBlocked)
            {
                var blockedCommands = new[] { "profile", "logout", "help", "menu", "clear", "exit" };
                return blockedCommands.Contains(command.Name.ToLower());
            }

            return baseCmd.AvailableFor.Contains(_authService.currentUser.Role);
        }
    }
}
