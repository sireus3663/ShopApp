using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands
{
    public class HelpCommand : BaseCommand
    {
        private readonly CommandRegistry _registry;
        public override string Name => "help";
        public override string Description => "Показать справку";

        public HelpCommand(CommandRegistry registry) { _registry = registry; }

        public override void Execute(string[] args) { _registry.ShowHelp(); }
    }
}
