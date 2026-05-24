using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopProject.Services;

namespace ShopProject.ConsoleCommands
{
    public class TestErrorCommand : BaseCommand
    {
        private readonly LoggerService _logger;
        public override string Name => "test-eror";
        public override string Description => "Тестовая ошибка";
        
        public TestErrorCommand(LoggerService logger) => _logger = logger;

        public override void Execute(string[] args)
        {
            try { throw new Exception("Это тестовая ошибка для проверки логирование"); }
            catch (Exception ex)
            {
                Error(ex.Message);
                _logger.Error("Тестовая ошибка", ex);
            }
        }

    }
}
