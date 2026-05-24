using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands
{
    public class ShowLogsCommand : BaseCommand
    {
        private readonly LoggerService _logger;
        public override string Name => "logs";
        public override string Description => "Показать логи";

        public ShowLogsCommand(LoggerService logger) => _logger = logger;

        public override void Execute(string[] args)
        {
            if (!File.Exists("logs/app.log")) { Info("log отсутвуют"); return; }


            var lines = File.ReadAllLines("logs/app.log");
            var count = lines.Length;

            Info($"всего {Math.Min(count, lines.Length)} запсей логов:");
            Console.WriteLine(new string('-', 80));

            foreach (var line in lines.TakeLast(count)) { Console.WriteLine(line); }
        }
    }
}

