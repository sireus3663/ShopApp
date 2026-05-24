using ShopProject.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands
{
    public static class CommandInitializer
    {
        public static void RegisterAll(
            CommandRegistry registry,
            LoggerService logger,
            UserService userService
            //сюда добавим сервисы
        )
        {
            registry.Register(new EchoCommand());

            registry.Register(new HelpCommand(registry));

            registry.Register(new ExitCommand());

            registry.Register(new ShowLogsCommand(logger));

            registry.Register(new TestErrorCommand(logger));

            registry.Register(new RegisterCommand(userService));
        }
    }
}
