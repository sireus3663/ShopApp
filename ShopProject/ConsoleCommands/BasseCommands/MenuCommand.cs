using ShopProject.Services.Interfaces;
using ShopProject.Models;
using System;
using System.Linq;

namespace ShopProject.ConsoleCommands.BasseCommands
{
    public class MenuCommand : BaseCommand
    {
        private readonly IAuthService _authService;
        private readonly CommandRegistry _registry;

        public override string Name => "menu";
        public override string Description => "Показать список доступных команд";
        public override bool AvailableForGuest => true;

        public MenuCommand(IAuthService authService, CommandRegistry registry)
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

            var currentUser = _authService.CurrentUser;
            if (currentUser != null)
            {
                Console.WriteLine($"Пользователь: {currentUser.Name ?? "Без имени"}");
                Console.WriteLine($"Роль: {currentUser.Role}");
                Console.WriteLine($"Баланс: {currentUser.Balance:F2} руб.");

                if (currentUser.IsBlocked)
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

            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
            {
                return baseCmd.AvailableForGuest;
            }

            if (currentUser.IsBlocked)
            {
                var blockedCommands = new[] { "profile", "logout", "help", "menu", "clear", "exit" };
                return blockedCommands.Contains(command.Name.ToLower());
            }

            return baseCmd.AvailableFor.Contains(currentUser.Role);
        }
    }
}