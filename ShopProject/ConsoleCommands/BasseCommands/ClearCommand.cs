using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.BasseCommands
{
    public class ClearCommand : BaseCommand
    {
        public override string Name => "clear";
        public override string Description => "Очистить консоль";

        public override void Execute(string[] args)
        {
            Console.Clear();
            Success("Консоль очищена");
        }
    }
}
