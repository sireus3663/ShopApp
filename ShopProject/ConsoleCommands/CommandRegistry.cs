using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands
{
    public class CommandRegistry
    {
        private readonly Dictionary<string, ICommand> _commands = new();

        public void Register(ICommand command)
        {
            _commands[command.Name.ToLower()] = command;
        }
        public ICommand? Get(string name)
        {
            _commands.TryGetValue(name.ToLower(), out var command);
            return command;
        }

        public void ShowHelp()
        {
            Console.WriteLine("\n=== Допуступные команды ===/n");
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
