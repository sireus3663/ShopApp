using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.Db
{
    internal class UserRoleRepository : BaseRepository<UserRole>
    {
        public UserRoleRepository(AppDbContext context) : base(context) { }
        public UserRole GetByUserId(Guid userId)
        {
            return _dbSet.FirstOrDefault(ur => ur.UserId == userId);
        }
    }
}
