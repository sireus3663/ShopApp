using ShopProject.Services.Interfaces;
using System;

namespace ShopProject.ConsoleCommands.BasseCommands
{
    public class TestErrorCommand : BaseCommand
    {
        private readonly ILoggerService _logger;
        public override string Name => "test-error";
        public override string Description => "Тестовая ошибка";

        public TestErrorCommand(ILoggerService logger) => _logger = logger;

        public override void Execute(string[] args)
        {
            _logger.Error("Выполнение тестовой ошибки");
            throw new Exception("Тестовая ошибка");
        }
    }
}