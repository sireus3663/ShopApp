using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.BasseCommands
{
   public class ExitCommand : BaseCommand
    {
        public override string Name => "exit";
        public override string Description => "Выход из консоли";
        public override bool AvailableForGuest => true;

        public override void Execute(string[] args)
        {
            Success("До свидания!");
            Environment.Exit(0);
        }
    }
}
