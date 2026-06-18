using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.BaseCommands
{
    public interface ICommand
    {
        string Name { get; }
        string Description { get; }
        void Execute(string[] args);
    }
}
