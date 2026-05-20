using ShopProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.Services
{
    public static class PermissionService
    {
        public static bool CanBuy(Role role)
        {
            return true; //любая роль сейчас может покупать, но возможно потом нам дадут правки, что кто то не может покупать
        }
        public static bool CanSell(Role role)
        {
            return role == Role.Seller || role == Role.Admin;
        }

        public static bool CanModerate(Role role)
        {
            return role == Role.Moderator || role == Role.Admin;
        }

        public static bool CanAdministrate(Role role)
        {
            return role == Role.Admin;
        }
    }
}
