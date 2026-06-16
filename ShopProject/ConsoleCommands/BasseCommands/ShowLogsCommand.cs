using ShopProject.Services.Interfaces;
using System;
using System.IO;
using System.Linq;

namespace ShopProject.ConsoleCommands.BasseCommands
{
    public class ShowLogsCommand : BaseCommand
    {
        private readonly ILoggerService _logger;
        public override string Name => "logs";
        public override string Description => "Показать логи";

        public ShowLogsCommand(ILoggerService logger) => _logger = logger;

        public override void Execute(string[] args)
        {
            const string logPath = "logs/app.log";
            if (!File.Exists(logPath))
            {
                Info("Логи отсутствуют");
                return;
            }

            var lines = File.ReadAllLines(logPath);
            if (lines.Length == 0)
            {
                Info("Лог-файл пуст");
                return;
            }

            var count = lines.Length;
            Info($"Всего {count} записей логов:");
            Console.WriteLine(new string('-', 80));

            foreach (var line in lines)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine(new string('-', 80));
        }
    }
}