using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.BasseCommands
{
    public class CommandRegistry
    {
        private readonly Dictionary<string, ICommand> _commands = new();
        private readonly LoggerService _logger;

        public CommandRegistry(LoggerService logger)
        {
            _logger = logger;
        }

        public void Register(ICommand command)
        {
            _commands[command.Name.ToLower()] = command;
        }
        public ICommand? Get(string name)
        {
            _commands.TryGetValue(name.ToLower(), out var command);
            return command;
        }
        public void Execute(string commandName, string[] args)
        {
            var command = Get(commandName);

            if (command == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Команда не найдена");
                Console.ResetColor();
                return;
            }

            try
            {
                command.Execute(args);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine(
                    $"Ошибка выполнения: {ex.Message}"
                );
                _logger.Error("ошибка выполнения команды", ex);

                Console.ResetColor();
            }
        }
        public void ShowHelp()
        {
            Console.WriteLine("\n=== Допуступные команды ===");
            foreach (var cmd in _commands.Values.Distinct().OrderBy(c => c.Name))
            {
                Console.WriteLine($"  {cmd.Name,-12} - {cmd.Description}");
            }
            Console.WriteLine("\n  help          - показать справку");
            Console.WriteLine("  exit          - выход");
            Console.WriteLine("\nФормат: команда [аргументы через пробел]\n");
        }
    }
}
