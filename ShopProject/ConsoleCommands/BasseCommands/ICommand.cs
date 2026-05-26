using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.BasseCommands
{
    public interface ICommand
    {
        string Name { get; } // Имя команды
        string Description { get; } // Описание команды
        void Execute(string[] args); // Метод выполнения
    }
}
