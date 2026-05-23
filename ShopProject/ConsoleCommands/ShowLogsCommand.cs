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
        public override string Description => "Показать лиги";

        public ShowLogsCommand(LoggerService logger) => _logger = logger;

        public override void Execute(string[] args)
        {
            if (!File.Exists("logs/app.log")) { Info("log отсутвуют"); return; }


            var lines = File.ReadAllLines("logs/app.log");
            var count = args.Length > 0 && int.TryParse(args[0], out var c) ? Math.Min(c, 20) : 10;

            Info($"Последние {Math.Min(count, lines.Length)} запсей логов:");
            Console.WriteLine(new string('-', 80));

            foreach (var line in lines.TakeLast(count)) { Console.WriteLine(line); }
        }
    }
}

