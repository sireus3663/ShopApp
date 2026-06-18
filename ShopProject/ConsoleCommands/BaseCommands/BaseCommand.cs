using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.ConsoleCommands.BaseCommands
{
    public abstract class BaseCommand : ICommand
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract void Execute(string[] args);
        public virtual List<Role> AvailableFor { get; } = new List<Role> { Role.Buyer, Role.Seller, Role.Moderator, Role.Admin };

        public virtual bool AvailableForGuest => false;

        protected void Success(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[OK] {msg}");
            Console.ResetColor();
        }

        protected void Error(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] {msg}");
            Console.ResetColor();
        }

        protected void Info(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[i] {msg}");
            Console.ResetColor();
        }
    }
}
