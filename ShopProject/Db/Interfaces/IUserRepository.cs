using ShopProject.Models;
using System;
using System.Threading.Tasks;

namespace ShopProject.Db.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        User? GetByEmail(string email);
        bool Exists(string email);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> ExistsAsync(string email);
        Task<User?> GetByLoginAsync(string email, string password);
    }
}