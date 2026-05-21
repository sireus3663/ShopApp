using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.Db
{
    public class UserRepository : BaseRepository<User>
    {
        public UserRepository(AppDbContext context) : base(context) { }

        public User GetByEmail(string email)
        {
            return _dbSet.FirstOrDefault(u => u.Email == email);
        }

        public User GetByLogin(string email, string password)
        {
            return _dbSet.FirstOrDefault(u => u.Email == email && u.Password == password);
        }

        public bool Exists(string email)
        {
            return _dbSet.Any(u => u.Email == email);
        }
    }
}