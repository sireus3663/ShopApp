using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.BasseCommands
{
    public class EchoCommand : BaseCommand
    {
        public override string Name => "echo";
        public override string Description => "Повторить текст. Использование: echo <текст>";
        public override bool AvailableForGuest => true;

        public override void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                Error("Укажите текст");
                return;
            }

            var message = string.Join(" ", args);
            Info($"Вы ввели: {message}");
        }
    }
}
