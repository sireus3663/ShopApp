using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopProject.Db;

namespace ShopProject.Services
{
    public class DbContextFactory
    {
        public AppDbContext Create()
        {
            return new AppDbContext();
        }
    }
}
