using Microsoft.EntityFrameworkCore;
using ShopProject.Models;
using ShopProject.Db.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.Db
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context) { }

        public User? GetByEmail(string email)
        {
            return _dbSet.FirstOrDefault(u => u.Email == email);
        }

        public bool Exists(string email)
        {
            return _dbSet.Any(u => u.Email == email);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> ExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }

        public async Task<User?> GetByLoginAsync(string email, string password)
        {
            var user = await GetByEmailAsync(email);
            if (user == null) return null;

            if (user.VerifyPassword(password))
                return user;

            return null;
        }
    }
}