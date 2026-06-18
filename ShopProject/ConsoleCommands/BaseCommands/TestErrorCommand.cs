using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopProject.Services;

namespace ShopProject.ConsoleCommands.BaseCommands
{
    public class TestErrorCommand : BaseCommand
    {
        private readonly LoggerService _logger;
        public override string Name => "test-error";
        public override string Description => "Тестовая ошибка";
        
        public TestErrorCommand(LoggerService logger) => _logger = logger;

        public override void Execute(string[] args)
        {
            throw new Exception("тестовая ошибка");
        }

    }
}
